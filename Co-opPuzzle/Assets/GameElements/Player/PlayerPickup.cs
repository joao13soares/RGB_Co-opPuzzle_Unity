using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public TcpClientController TcpClientController;

    public GameObject otherPlayer;
    Rigidbody rb;
    public bool hasPickedUp;
    bool oldHasPickedUp;
    public bool isPickedUp;
    bool oldIsPickedUp;

    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (otherPlayer != null)
        {
            // check if picks up
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!hasPickedUp && !isPickedUp
                    && Vector3.Distance(this.transform.position, otherPlayer.transform.position) <= 1.5f
                    //&& this.transform.position.y == otherPlayer.transform.position.y
                    && this.GetComponent<PlayerMovement>().IsGrounded())
                {
                    hasPickedUp = true;

                    otherPlayer.transform.SetParent(this.transform);
                    otherPlayer.transform.position = this.transform.position + Vector3.up * 0.5f * (otherPlayer.transform.localScale.y + this.transform.localScale.y);
                }
                else
                {
                    hasPickedUp = false;

                    otherPlayer.transform.SetParent(null);
                }

                if (hasPickedUp != oldHasPickedUp)
                {
                    UpdateServerWithPickupInfo();

                    // update otherPlayer isPickedup value on UI when thisPlayer hasPickedup value changed
                    otherPlayer.GetComponent<PlayerInfoDisplay>().UpdatePlayerInfoDisplay(hasPickedUp);

                    GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.Pickup);
                }
                oldHasPickedUp = hasPickedUp;
            }

            // check if is picked up
            if (!isPickedUp)
            {
                rb.constraints &= ~RigidbodyConstraints.FreezePositionX;
                rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
                this.transform.SetParent(null);
            }
            else if (!hasPickedUp)
            {
                rb.constraints |= RigidbodyConstraints.FreezePositionX;
                rb.constraints |= RigidbodyConstraints.FreezePositionY;
                this.transform.SetParent(otherPlayer.transform);
                this.transform.position = otherPlayer.transform.position + Vector3.up * 0.5f * (this.transform.localScale.y + otherPlayer.transform.localScale.y);
            }

            if (isPickedUp != oldIsPickedUp)
            {
                GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.Pickup);
            }
            oldIsPickedUp = isPickedUp;
        }
    }

    void UpdateServerWithPickupInfo()
    {
        TcpClientController.thisPlayer.PlayerInfo.HasPickedUp = this.hasPickedUp;

        Message pickupMsg = new Message(MessageType.PlayerPickup, TcpClientController.thisPlayer.PlayerInfo, "");

        TcpClientController.thisPlayer.SendMessage(pickupMsg);
    }
}
