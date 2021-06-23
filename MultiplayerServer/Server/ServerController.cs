using Common;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ServerController
    {
        TcpListener _tcpListener;
        List<Player> _playerList;

        public ServerController()
        {
            _playerList = new List<Player>();
        }

        // StartServer
        public void StartServer(string IpAddress, int Port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(IpAddress), Port);
            _tcpListener.Start();
            Console.WriteLine("Server started");
        }

        // RunServer
        public void RunServer()
        {
            while(_tcpListener != null)
            {
                HandlePendingConnections();

                HandleCurrentPlayers();
            }
        }

        // HandlePendingConnections
        async void HandlePendingConnections()
        {
            if (_tcpListener.Pending())
            {
                Console.WriteLine("\nNew pending connection");
                //_tcpListener.BeginAcceptTcpClient(AcceptTcpClient, _tcpListener);
                await AcceptTcpClientAsync(_tcpListener);

            }
        }

        // AcceptTcpClientAsync
        async Task AcceptTcpClientAsync(TcpListener tcpListener)
        {
            TcpClient newTcpClient = await tcpListener.AcceptTcpClientAsync();

            if (newTcpClient.Connected)
            {
                Console.WriteLine("\nAccepted new connection");

                Player newPlayer = new Player(newTcpClient, PlayerState.Connecting);

                Message newPlayerIdMsg = new Message(MessageType.Information, newPlayer.PlayerInfo, "Hello new player");
                string newPlayerIdMsgJson = newPlayer.SendMessage(newPlayerIdMsg);
                Console.WriteLine(newPlayerIdMsgJson);

                _playerList.Add(newPlayer);
            }
            else
            {
                Console.WriteLine("\nConnection refused");
            }
        }

        // HandleCurrentPlayers
        void HandleCurrentPlayers()
        {
            foreach (Player player in _playerList)
            {
                switch (player.State)
                {
                    case PlayerState.Connecting:
                        NewPlayerConnecting(player);
                        break;
                    case PlayerState.Syncing:
                        NewPlayerSyncing(player);
                        break;
                    case PlayerState.Playing:
                        PlayerPlaying(player);
                        break;
                }
            }
        }

        // NewPlayerConnecting
        void NewPlayerConnecting(Player newPlayer)
        {
            if (newPlayer.DataAvailable())
            {
                Message newPlayerNameMsg = newPlayer.ReceiveMessage();

                // save Name data
                newPlayer.PlayerInfo.Name = newPlayerNameMsg.PlayerInfo.Name;

                // notify all players about a new player connecting and send its entry order in the game, so they know what color it should have
                Message newPlayerConnectingMsg = new Message(MessageType.NewPlayer, newPlayer.PlayerInfo, _playerList.IndexOf(newPlayer).ToString());
                MessageAllPlayers(newPlayerConnectingMsg);

                newPlayer.State = PlayerState.Syncing;
            }
        }

        // NewPlayerSyncing
        void NewPlayerSyncing(Player newPlayer)
        {
            Console.WriteLine("\nNew player sync");

            MessageType[] messageTypesToSync = {
                MessageType.NewPlayer,
                MessageType.PlayerMovement,
                MessageType.PlayerLife };

            // sync newPlayer with all other potential already existing players and their movement and life
            foreach (Player p in _playerList)
            {
                if (p.State == PlayerState.Playing)
                {
                    foreach (MessageType messageTypeToSync in messageTypesToSync)
                    {
                        // adding (in the message description) the entry orders of other potential already existing players in the game, so the newPlayer knows what color they should have
                        Message thereIsAnotherPlayerInfoMsg = new Message(messageTypeToSync, p.PlayerInfo, _playerList.IndexOf(p).ToString());
                        newPlayer.SendMessage(thereIsAnotherPlayerInfoMsg);
                    }
                }
            }

            // notify syncing player and send its entry order in the game, so it knows what color should have
            Message newPlayerSyncedMsg = new Message(MessageType.FinishedSync, newPlayer.PlayerInfo, _playerList.IndexOf(newPlayer).ToString());
            newPlayer.SendMessage(newPlayerSyncedMsg);

            newPlayer.State = PlayerState.Playing;
        }

        // PlayerPlaying
        void PlayerPlaying(Player player)
        {
            if (player.DataAvailable())
            {
                Message playerUpdateMsg = player.ReceiveMessage();

                // send new info to all players
                if (playerUpdateMsg.MessageType == MessageType.PlayerMovement
                    || playerUpdateMsg.MessageType == MessageType.PlayerPickup
                    || playerUpdateMsg.MessageType == MessageType.PlayerLife)
                {
                    // save X & Y data
                    player.PlayerInfo.X = playerUpdateMsg.PlayerInfo.X;
                    player.PlayerInfo.Y = playerUpdateMsg.PlayerInfo.Y;

                    // save HasPickedUp data
                    player.PlayerInfo.HasPickedUp = playerUpdateMsg.PlayerInfo.HasPickedUp;

                    // save NumLives data
                    player.PlayerInfo.NumLives = playerUpdateMsg.PlayerInfo.NumLives;

                    MessageAllPlayers(playerUpdateMsg);
                }

                // check if game over
                if (playerUpdateMsg.MessageType == MessageType.PlayerLife)
                {
                    CheckIfGameOver(playerUpdateMsg);
                }

                // check if level completed
                if (playerUpdateMsg.MessageType == MessageType.PlayerLevelEnd)
                {
                    // save CompletedLevel data
                    player.PlayerInfo.CompletedLevel = playerUpdateMsg.PlayerInfo.CompletedLevel;

                    CheckIfLevelCompleted();
                }
            }
        }

        // CheckIfGameOver
        void CheckIfGameOver(Message playerLifeMsg)
        {
            if(playerLifeMsg.PlayerInfo.NumLives <= 0)
            {
                // a player has died... game over
                Message gameOverMsg = new Message(MessageType.GameOver, playerLifeMsg.PlayerInfo, "");
                MessageAllPlayers(gameOverMsg);
            }
        }

        // CheckIfLevelCompleted
        void CheckIfLevelCompleted()
        {
            bool allPlayersCompletedLevel = true;
            foreach (Player player in _playerList)
            {
                if (!player.PlayerInfo.CompletedLevel)
                {
                    // at least one player didn't complete the level yet
                    allPlayersCompletedLevel = false;
                    break;
                }
            }

            if (_playerList.Count > 1 && allPlayersCompletedLevel)
            {
                // all players completed the level
                Message levelCompletedMsg = new Message(MessageType.LevelCompleted, null, "");
                MessageAllPlayers(levelCompletedMsg);
            }
        }

        void MessageAllPlayers(Message msg)
        {
            string msgJson = "";
            foreach (Player p in _playerList)
            {
                if (msg.MessageType == MessageType.NewPlayer || p.State == PlayerState.Playing)
                {
                    msgJson = p.SendMessage(msg);
                }
            }
            Console.WriteLine("\n" + msg.MessageType + " update\n" + msgJson);
        }
    }
}
