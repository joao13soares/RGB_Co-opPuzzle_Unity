using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateBehavior : SwitcherBehavior
{
    protected override void OnTriggerExit(Collider other)
    {
        if (other.tag == "thisPlayer" || other.tag == "otherPlayer")
        {
            numberOfPlayersPressing--;
        }

        if (numberOfPlayersPressing <= 0 && isPressed)
        {
            isPressed = false;
            Release();

            GameObject.Find("Pads").GetComponent<PadsManager>().SwitchColors();

            GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.SwitchColors);
        }
    }
}
