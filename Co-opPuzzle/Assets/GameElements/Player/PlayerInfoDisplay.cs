using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoDisplay : MonoBehaviour
{
    [SerializeField] Text nameAndLivesText;
    int numLives;
    bool isPickedUp;

    public void UpdatePlayerInfoDisplay(int numLives, bool isPickedUp)
    {
        this.numLives = numLives;
        this.isPickedUp = isPickedUp;

        SendUpdatedPlayerInfoToUI();
    }

    public void UpdatePlayerInfoDisplay(int numLives)
    {
        this.numLives = numLives;

        SendUpdatedPlayerInfoToUI();
    }

    public void UpdatePlayerInfoDisplay(bool isPickedUp)
    {
        this.isPickedUp = isPickedUp;

        SendUpdatedPlayerInfoToUI();
    }

    private void SendUpdatedPlayerInfoToUI()
    {
        nameAndLivesText.text =
            this.transform.name.Split(' ')[0] + "\n" +
            numLives.ToString() + "\n" +
            (isPickedUp ? "P" : " ");
    }
}
