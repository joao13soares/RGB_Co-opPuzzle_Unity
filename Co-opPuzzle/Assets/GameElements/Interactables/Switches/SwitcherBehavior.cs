using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherBehavior : MonoBehaviour
{
    [SerializeField] protected bool isPressed;
    protected int numberOfPlayersPressing;
    Transform switcherRenderTransform;

    void Start()
    {
        switcherRenderTransform = this.transform.parent.GetChild(1);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.tag == "thisPlayer" || other.tag == "otherPlayer")
        {
            numberOfPlayersPressing++;
        }

        if (numberOfPlayersPressing > 0 && !isPressed)
        {
            isPressed = true;
            Press();

            GameObject.Find("Pads").GetComponent<PadsManager>().SwitchColors();

            GameObject.Find("SoundEffects").GetComponent<SoundEffects>().PlaySoundEffect(SoundEffects.Sounds.SwitchColors);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.tag == "thisPlayer" || other.tag == "otherPlayer")
        {
            numberOfPlayersPressing--;
        }

        if (numberOfPlayersPressing <= 0 && isPressed)
        {
            isPressed = false;
            Release();
        }
    }

    protected void Press()
    {
        float yScallingFactor = 0.75f;

        switcherRenderTransform.localPosition -= Vector3.up * this.transform.localScale.y * yScallingFactor * 0.5f;
        switcherRenderTransform.localScale -= Vector3.up * this.transform.localScale.y * yScallingFactor;

        switcherRenderTransform.GetComponent<Collider>().enabled = true;
    }

    protected void Release()
    {
        switcherRenderTransform.localPosition = this.transform.localPosition;
        switcherRenderTransform.localScale = this.transform.localScale;

        switcherRenderTransform.GetComponent<Collider>().enabled = false;
    }
}
