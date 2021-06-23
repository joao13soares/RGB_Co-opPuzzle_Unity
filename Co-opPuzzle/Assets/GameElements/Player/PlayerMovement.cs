using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public TcpClientController TcpClientController;
    Vector3 _oldPosition;

    [SerializeField] Rigidbody rb;
    [SerializeField] float speed;
    [SerializeField] bool isGrounded;

    void FixedUpdate()
    {
        CheckMovementInputs();

        UpdateServerWithMovementInfo();
    }

    void UpdateServerWithMovementInfo()
    {
        if (this.transform.position != _oldPosition)
        {
            TcpClientController.thisPlayer.PlayerInfo.X = this.transform.position.x;
            TcpClientController.thisPlayer.PlayerInfo.Y = this.transform.position.y;

            Message newPositionMsg = new Message(MessageType.PlayerMovement, TcpClientController.thisPlayer.PlayerInfo, "");

            TcpClientController.thisPlayer.SendMessage(newPositionMsg);
        }
        
        _oldPosition = this.transform.position;
    }

    void CheckMovementInputs()
    {
        if (Input.GetKey(KeyCode.A))
        {
            rb.velocity = new Vector3(-speed, rb.velocity.y, 0);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rb.velocity = new Vector3(speed, rb.velocity.y, 0);
        }

        if (Input.GetKey(KeyCode.W) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, speed * 1.7f, 0);

            if (!GameObject.Find("SoundEffects").GetComponent<AudioSource>().isPlaying)
                GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.Jump);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Pad")
        {
            if (PadsManager.SameColors(other.gameObject, this.gameObject))
            {
                rb.velocity = new Vector3(rb.velocity.x, speed * 2.7f, 0);

                GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.JumpPad);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ground" && IsHigherThan(other.transform)
            || other.tag == "Switcher" || other.tag == "PressurePlate" || other.tag == "Checkpoint" || other.tag == "LevelEnd")
        {
            isGrounded = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ground"
            || other.tag == "Switcher" || other.tag == "PressurePlate" || other.tag == "Checkpoint" || other.tag == "LevelEnd")
        {
            isGrounded = false;
        }
    }

    bool IsHigherThan(Transform t)
    {
        return (this.transform.position.y - this.transform.localScale.y / 2)
            >= (t.transform.position.y + t.transform.localScale.y / 2);
    }

    public bool IsGrounded() => isGrounded;
}
