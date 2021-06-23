using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace Common
{
    public enum PlayerState
    {
        Disconnected,
        Connecting,
        Connected,
        Syncing,
        Playing
    }

    public class Player
    {
        public PlayerInfo PlayerInfo { get; set; }
        public List<Message> MessageList { get; set; }
        [JsonIgnore]
        public TcpClient TcpClient { get; set; }
        [JsonIgnore]
        public BinaryReader BinaryReader { get; set; }
        [JsonIgnore]
        public BinaryWriter BinaryWriter { get; set; }
        public PlayerState State { get; set; }
        public PlayerState OldState { get; set; }

        public Player(TcpClient tcpClient, PlayerState state)
        {
            PlayerInfo = new PlayerInfo();
            TcpClient = tcpClient;

            if (state != PlayerState.Disconnected)
            {
                MessageList = new List<Message>();
                BinaryReader = new System.IO.BinaryReader(TcpClient.GetStream());
                BinaryWriter = new System.IO.BinaryWriter(TcpClient.GetStream());
            }

            State = state;
            OldState = (PlayerState)(-1);
        }

        public bool DataAvailable()
        {
            return TcpClient.GetStream().DataAvailable;
        }

        public Message ReceiveMessage()
        {
            string msgJson = BinaryReader.ReadString();
            Message msg = JsonConvert.DeserializeObject<Message>(msgJson);
            MessageList.Add(msg);
            return msg;
        }

        public string SendMessage(Message msg)
        {
            string msgJson = JsonConvert.SerializeObject(msg);
            BinaryWriter.Write(msgJson);
            MessageList.Add(msg);
            return msgJson;
        }
    }
}
