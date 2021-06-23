using Common;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class TcpClientController : MonoBehaviour
{

    [HideInInspector]
    public Player thisPlayer;
    Dictionary<Guid, GameObject> _playerObjDict;
    
    public GameObject SpawPoint;
    public GameObject PlayerPrefab;
    public GameObject ConnectionUI;
    public Text PlayerNameInputText;
    public GameObject PopUpTextUI;
    public Text PopUpText;
    public string IpAddress;
    public int Port;

    void Awake()
    {
        thisPlayer = new Player(new TcpClient(), PlayerState.Disconnected);
        _playerObjDict = new Dictionary<Guid, GameObject>();
    }

    // StartTcpClient (called by pressing down Enter key or by clicking on the "Join Game" button in the ConnectionUI)
    public void StartTcpClient()
    {
        thisPlayer.TcpClient.BeginConnect(IPAddress.Parse(IpAddress), Port, AcceptConnection, thisPlayer.TcpClient);
        thisPlayer.State = PlayerState.Connecting;
    }

    // AcceptConnection
    void AcceptConnection(IAsyncResult ar)
    {
        TcpClient tcpClient = (TcpClient)ar.AsyncState;
        tcpClient.EndConnect(ar);

        if (tcpClient.Connected)
        {
            Debug.Log("Client connection accepted");
            thisPlayer.BinaryReader = new System.IO.BinaryReader(tcpClient.GetStream());
            thisPlayer.BinaryWriter = new System.IO.BinaryWriter(tcpClient.GetStream());
            thisPlayer.MessageList = new List<Message>();
        }
        else
        {
            Debug.Log("Client connection refused");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && thisPlayer.State == PlayerState.Disconnected && ConnectionUI.active == true)
        {
            StartTcpClient();
        }

        if (thisPlayer.TcpClient.Connected)
        {
            switch (thisPlayer.State)
            {
                case PlayerState.Connecting:
                    Connecting();
                    break;
                case PlayerState.Connected:
                    Connected();
                    break;
                case PlayerState.Syncing:
                    Syncing();
                    break;
                case PlayerState.Playing:
                    Playing();
                    break;
            }
        }

        // print State updates only once
        if (thisPlayer.State != thisPlayer.OldState)
        {
            Debug.Log(thisPlayer.State.ToString());
            thisPlayer.OldState = thisPlayer.State;
        }
    }

    // Connecting
    void Connecting()
    {
        if (thisPlayer.DataAvailable())
        {
            // get thisPlayer ID given by server
            Message thisPlayerIdMsg = thisPlayer.ReceiveMessage();
            thisPlayer.PlayerInfo.Id = thisPlayerIdMsg.PlayerInfo.Id;

            // get thisPlayer Name from InputText on ConnectionUI
            thisPlayer.PlayerInfo.Name = PlayerNameInputText.text;

            // get thisPlayer NumLives
            thisPlayer.PlayerInfo.NumLives = 3;

            // update Server with newPlayer's name
            Message thisPlayerNameMsg = new Message(MessageType.PlayerName, thisPlayer.PlayerInfo, "");
            thisPlayer.SendMessage(thisPlayerNameMsg);

            thisPlayer.State = PlayerState.Connected;
        }
    }

    // Connected
    void Connected()
    {
        if (thisPlayer.DataAvailable())
        {
            Message msg = thisPlayer.ReceiveMessage();
            Debug.Log(msg.Description);

            thisPlayer.State = PlayerState.Syncing;
        }
    }

    // Syncing
    void Syncing()
    {
        if (thisPlayer.DataAvailable())
        {
            Message msg = thisPlayer.ReceiveMessage();
            
            if (msg.MessageType == MessageType.NewPlayer)
            {
                // spawn otherPlayer when thisPlayer is syncing
                GameObject otherPlayerObj = SpawnOtherPlayer(msg);

                // add otherPlayer to list
                _playerObjDict.Add(msg.PlayerInfo.Id, otherPlayerObj);
            }
            else if (msg.MessageType == MessageType.PlayerMovement)
            {
                if (_playerObjDict.ContainsKey(msg.PlayerInfo.Id))
                {
                    // update otherPlayer movement when thisPlayer is syncing
                    _playerObjDict[msg.PlayerInfo.Id].transform.position =
                        new Vector3(msg.PlayerInfo.X, msg.PlayerInfo.Y);
                }
            }
            else if (msg.MessageType == MessageType.PlayerLife)
            {
                if (_playerObjDict.ContainsKey(msg.PlayerInfo.Id))
                {
                    // update otherPlayer numLives on UI when thisPlayer is syncing
                    _playerObjDict[msg.PlayerInfo.Id].GetComponent<PlayerInfoDisplay>().UpdatePlayerInfoDisplay(
                        msg.PlayerInfo.NumLives);
                }
            }
            else if (msg.MessageType == MessageType.FinishedSync)
            {
                // desactivate ConnectionUI
                ConnectionUI.SetActive(false);

                // spawn thisPlayer
                GameObject thisPlayerObj = SpawnThisPlayer(msg);

                // add thisPlayer to list
                _playerObjDict.Add(thisPlayer.PlayerInfo.Id, thisPlayerObj);

                thisPlayer.State = PlayerState.Playing;
            }
        }
    }

    // Playing
    void Playing()
    {
        if (thisPlayer.DataAvailable())
        {
            Message msg = thisPlayer.ReceiveMessage();
            if (msg.MessageType == MessageType.NewPlayer)
            {
                // spawn otherPlayer when thisPlayer is playing
                GameObject otherPlayerObj = SpawnOtherPlayer(msg);

                // add otherPlayer to list
                _playerObjDict.Add(msg.PlayerInfo.Id, otherPlayerObj);
            }
            else if (msg.MessageType == MessageType.PlayerMovement)
            {
                if (_playerObjDict.ContainsKey(msg.PlayerInfo.Id) 
                    && msg.PlayerInfo.Id != thisPlayer.PlayerInfo.Id
                    && !_playerObjDict[thisPlayer.PlayerInfo.Id].GetComponent<PlayerPickup>().hasPickedUp)
                {
                    // update otherPlayer movement info when thisPlayer is playing
                    _playerObjDict[msg.PlayerInfo.Id].transform.position = 
                        new Vector3(msg.PlayerInfo.X, msg.PlayerInfo.Y);
                }
            }
            else if (msg.MessageType == MessageType.PlayerLife)
            {
                if (_playerObjDict.ContainsKey(msg.PlayerInfo.Id)
                    && msg.PlayerInfo.Id != thisPlayer.PlayerInfo.Id)
                {
                    // update otherPlayer numLives on UI when thisPlayer is playing
                    _playerObjDict[msg.PlayerInfo.Id].GetComponent<PlayerInfoDisplay>().UpdatePlayerInfoDisplay(
                        msg.PlayerInfo.NumLives);
                }
            }
            else if (msg.MessageType == MessageType.PlayerPickup)
            {
                if (_playerObjDict.ContainsKey(msg.PlayerInfo.Id)
                    && msg.PlayerInfo.Id != thisPlayer.PlayerInfo.Id)
                {
                    // update thisPlayer isPickedup value when otherPlayer hasPickedup value changed
                    _playerObjDict[thisPlayer.PlayerInfo.Id].GetComponent<PlayerPickup>().isPickedUp = 
                        msg.PlayerInfo.HasPickedUp;

                    // update thisPlayer isPickedup value on UI when otherPlayer hasPickedup value changed
                    _playerObjDict[thisPlayer.PlayerInfo.Id].GetComponent<PlayerInfoDisplay>().UpdatePlayerInfoDisplay(
                        msg.PlayerInfo.HasPickedUp);
                }
            }
            else if (msg.MessageType == MessageType.GameOver)
            {
                if (_playerObjDict.ContainsKey(msg.PlayerInfo.Id))
                {
                    Debug.Log("Game Over! (player " + msg.PlayerInfo.Name + " died)");

                    GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.GameOver);

                    StartCoroutine(ChangeLevel(msg.MessageType));
                }
            }
            else if (msg.MessageType == MessageType.LevelCompleted)
            {
                Debug.Log("Level Completed");

                GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.LevelCompleted);

                StartCoroutine(ChangeLevel(msg.MessageType));
            }
        }
    }

    // SpawnThisPlayer
    GameObject SpawnThisPlayer(Message msg)
    {
        // spawn thisPlayer
        GameObject thisPlayerObj = Instantiate(
            PlayerPrefab,
            SpawPoint.transform.position,
            Quaternion.identity);

        // position thisPlayer according to its index on list of players
        thisPlayerObj.transform.position += Vector3.up * thisPlayerObj.transform.localScale.y * (_playerObjDict.Count + 1.0f);

        // handle thisPlayer components and scripts
        thisPlayerObj.name = thisPlayer.PlayerInfo.Name + " " + thisPlayer.PlayerInfo.Id;
        thisPlayerObj.tag = "thisPlayer";
        thisPlayerObj.GetComponent<PlayerMovement>().TcpClientController = this;
        thisPlayerObj.GetComponent<PlayerPickup>().TcpClientController = this;
        thisPlayerObj.GetComponent<PlayerLife>().TcpClientController = this;
        thisPlayerObj.GetComponent<PlayerLife>().numLives = msg.PlayerInfo.NumLives;
        thisPlayerObj.GetComponent<PlayerLife>().UpdateCurrentCheckpoint(SpawPoint);
        thisPlayerObj.GetComponent<PlayerInfoDisplay>().UpdatePlayerInfoDisplay(msg.PlayerInfo.NumLives);
        thisPlayerObj.GetComponent<Renderer>().material = PlayersColors.GetColorFromIndex(int.Parse(msg.Description));

        GameObject otherPlayerObj = GameObject.FindGameObjectWithTag("otherPlayer");
        if (otherPlayerObj != null)
        {
            thisPlayerObj.GetComponent<PlayerPickup>().otherPlayer = otherPlayerObj;
        }

        GameObject[] levelEnds = GameObject.FindGameObjectsWithTag("LevelEnd");
        foreach (GameObject levelEnd in levelEnds)
        {
            levelEnd.GetComponentInChildren<LevelEndBehavior>().TcpClientController = this;
        }

        return thisPlayerObj;
    }

    // SpawnOtherPlayer
    GameObject SpawnOtherPlayer(Message msg)
    {
        // spawn otherPlayer
        GameObject otherPlayerObj = Instantiate(
            PlayerPrefab,
            new Vector3(msg.PlayerInfo.X, msg.PlayerInfo.Y),
            Quaternion.identity);

        // position otherPlayer according to its index on list of players
        otherPlayerObj.transform.position += Vector3.up * otherPlayerObj.transform.localScale.y * (_playerObjDict.Count + 1.0f);

        // handle otherPlayer components and scripts
        otherPlayerObj.name = msg.PlayerInfo.Name + " " + msg.PlayerInfo.Id;
        otherPlayerObj.tag = "otherPlayer";
        otherPlayerObj.GetComponent<Rigidbody>().isKinematic = true;
        Component.Destroy(otherPlayerObj.GetComponent<PlayerMovement>());
        Component.Destroy(otherPlayerObj.GetComponent<PlayerPickup>());
        Component.Destroy(otherPlayerObj.GetComponent<PlayerLife>());
        otherPlayerObj.GetComponent<PlayerInfoDisplay>().UpdatePlayerInfoDisplay(msg.PlayerInfo.NumLives, msg.PlayerInfo.HasPickedUp);
        otherPlayerObj.GetComponent<Renderer>().material = PlayersColors.GetColorFromIndex(int.Parse(msg.Description));

        GameObject thisPlayerObj = GameObject.FindGameObjectWithTag("thisPlayer");
        if (thisPlayerObj != null)
        {
            thisPlayerObj.GetComponent<PlayerPickup>().otherPlayer = otherPlayerObj;
        }

        return otherPlayerObj;
    }

    // ChangeLevel
    IEnumerator ChangeLevel(MessageType levelChangeMsgType)
    {
        // deactivate thisPlayer's control scripts
        _playerObjDict[thisPlayer.PlayerInfo.Id].GetComponent<PlayerMovement>().enabled = false;
        _playerObjDict[thisPlayer.PlayerInfo.Id].GetComponent<PlayerPickup>().enabled = false;

        // show Loading Level or Game Over or Level Completed text (for 3 seconds)
        string levelChangeText = Regex.Replace(levelChangeMsgType.ToString(), @"([a-z])([A-Z])", "$1 $2");
        PopUpText.text = _playerObjDict[thisPlayer.PlayerInfo.Id].GetComponent<PlayerLife>().currentLevel.name == "Lobby" ? "Loading Level" : levelChangeText;
        PopUpTextUI.SetActive(true);
        yield return new WaitForSeconds(3);
        PopUpTextUI.SetActive(false);

        // respawn thisPlayer on new Level's SpawnPoint
        _playerObjDict[thisPlayer.PlayerInfo.Id].GetComponent<PlayerLife>().ChangeLevel(levelChangeMsgType);

        // change camera position to current Level
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        camera.transform.position = new Vector3(_playerObjDict[thisPlayer.PlayerInfo.Id].GetComponent<PlayerLife>().currentLevel.transform.position.x, camera.transform.position.y, camera.transform.position.z);

        // reset color of all previous level's pads
        GameObject.Find("Pads").GetComponent<PadsManager>().ResetToDefaultColor();

        // reactivate thisPlayer's control scripts
        _playerObjDict[thisPlayer.PlayerInfo.Id].GetComponent<PlayerMovement>().enabled = true;
        _playerObjDict[thisPlayer.PlayerInfo.Id].GetComponent<PlayerPickup>().enabled = true;
    }

    public int IndexOfPlayer(GameObject playerObj)
    {
        return Array.IndexOf(_playerObjDict.Values.ToArray<GameObject>(), playerObj);
    }
}
