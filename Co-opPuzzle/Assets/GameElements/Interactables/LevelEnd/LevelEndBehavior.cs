using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndBehavior : SwitcherBehavior
{
    public TcpClientController TcpClientController;

    [SerializeField] bool completedLevel;
    public GameObject nextSpawnPoint;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.tag == "thisPlayer" || other.tag == "otherPlayer")
        {
            numberOfPlayersPressing++;

            if (other.tag == "thisPlayer")
            {
                completedLevel = true;
                UpdateServerWithLevelEndInfo();

                other.gameObject.GetComponent<PlayerLife>().UpdateCurrentCheckpoint(this.transform.parent.gameObject);

                GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.LevelEndPress);
            }
        }

        if (numberOfPlayersPressing > 0 && !isPressed)
        {
            isPressed = true;
            Press();

            ToggleOtherLevelEnds(false);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (other.tag == "thisPlayer" || other.tag == "otherPlayer")
        {
            numberOfPlayersPressing--;

            if (other.tag == "thisPlayer")
            {
                completedLevel = false;
                UpdateServerWithLevelEndInfo();
            }
        }

        if (numberOfPlayersPressing <= 0 && isPressed)
        {
            isPressed = false;
            Release();

            ToggleOtherLevelEnds(true);
        }
    }

    void ToggleOtherLevelEnds(bool toggle)
    {
        GameObject lobbyCheckpoints = this.transform.parent.parent.gameObject;
        int numLevels = lobbyCheckpoints.transform.childCount;
        for (int i = 0; i < numLevels; i++)
        {
            GameObject levelEndi = lobbyCheckpoints.transform.GetChild(i).gameObject;
            GameObject thisLevelEnd = this.transform.parent.gameObject;
            if (i != thisLevelEnd.transform.GetSiblingIndex() && levelEndi.name.StartsWith("LevelEnd"))
            {
                levelEndi.SetActive(toggle);
            }
        }
    }

    void UpdateServerWithLevelEndInfo()
    {
        TcpClientController.thisPlayer.PlayerInfo.CompletedLevel = this.completedLevel;

        Message levelEndMsg = new Message(MessageType.PlayerLevelEnd, TcpClientController.thisPlayer.PlayerInfo, "");

        TcpClientController.thisPlayer.SendMessage(levelEndMsg);
    }
}
