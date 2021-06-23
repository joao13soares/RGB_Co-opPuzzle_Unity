using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    public TcpClientController TcpClientController;

    public int numLives;
    public GameObject currentCheckpoint;
    public GameObject currentLevel;

    [SerializeField] PlayerInfoDisplay playerInfoDisplay;

    void Start()
    {
        currentLevel = currentCheckpoint.transform.root.gameObject;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Pad")
        {
            if (!PadsManager.SameColors(other.gameObject, this.gameObject))
            {
                numLives--;

                playerInfoDisplay.UpdatePlayerInfoDisplay(numLives);
                UpdateServerWithLifeInfo();

                GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.Damage);

                if (numLives > 0)
                {
                    RespawnOnLastCheckpoint();
                }
                else
                {
                    Debug.Log("Died");
                }
            }
        }
    }

    void UpdateServerWithLifeInfo()
    {
        TcpClientController.thisPlayer.PlayerInfo.NumLives = this.numLives;

        Message numLivesMsg = new Message(MessageType.PlayerLife, TcpClientController.thisPlayer.PlayerInfo, "");

        TcpClientController.thisPlayer.SendMessage(numLivesMsg);
    }

    public void RespawnOnLastCheckpoint()
    {
        // respawn player on Y according to its index on the list of players
        this.transform.position = currentCheckpoint.transform.position + Vector3.up * this.transform.localScale.y * (TcpClientController.IndexOfPlayer(this.gameObject) + 1.0f);

        Debug.Log("Respawned");
    }

    public void UpdateCurrentCheckpoint(GameObject checkpoint)
    {
        if (currentCheckpoint == null 
            || currentCheckpoint.transform.GetSiblingIndex() < checkpoint.transform.GetSiblingIndex())
        {
            currentCheckpoint = checkpoint;

            Debug.Log("Current checkpoint: " + currentCheckpoint.name + " (" + currentCheckpoint.transform.GetSiblingIndex() + ")");
        }
    }

    public void ChangeLevel(MessageType changeLevelMsgType)
    {
        numLives = 3;
        playerInfoDisplay.UpdatePlayerInfoDisplay(numLives);
        UpdateServerWithLifeInfo();

        if (changeLevelMsgType == MessageType.GameOver)
        {
            currentCheckpoint = TcpClientController.SpawPoint;
        }
        else if (changeLevelMsgType == MessageType.LevelCompleted)
        {
            currentCheckpoint = currentCheckpoint.GetComponentInChildren<LevelEndBehavior>().nextSpawnPoint;
        }
        Debug.Log("Current checkpoint: " + currentCheckpoint.name + " (" + currentCheckpoint.transform.GetSiblingIndex() + ")");
        currentLevel = currentCheckpoint.transform.root.gameObject;
        RespawnOnLastCheckpoint();
    }
}
