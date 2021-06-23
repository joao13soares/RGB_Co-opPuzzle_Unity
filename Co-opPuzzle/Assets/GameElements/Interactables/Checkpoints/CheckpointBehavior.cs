using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointBehavior : SwitcherBehavior
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.tag == "thisPlayer" || other.tag == "otherPlayer")
        {
            numberOfPlayersPressing++;

            if (other.tag == "thisPlayer")
            {
                other.gameObject.GetComponent<PlayerLife>().UpdateCurrentCheckpoint(this.transform.parent.gameObject);
            }
        }

        if (numberOfPlayersPressing > 0 && !isPressed)
        {
            isPressed = true;
            Press();

            GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.Checkpoint);
        }
    }
}
