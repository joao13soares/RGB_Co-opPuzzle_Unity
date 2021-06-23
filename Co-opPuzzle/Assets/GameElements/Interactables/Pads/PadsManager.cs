using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PadsManager : MonoBehaviour
{
    GameObject[] pads;

    void Start()
    {
        pads = GameObject.FindGameObjectsWithTag("Pad");
    }
    
    void Update()
    {

    }

    public void SwitchColors()
    {
        foreach (GameObject pad in pads)
        {
            pad.GetComponent<PadBehavior>().SwitchColor();
        }
    }

    public void ResetToDefaultColor()
    {
        foreach (GameObject pad in pads)
        {
            pad.GetComponent<PadBehavior>().ResetToDefaultColor();
        }
    }

    public static bool SameColors(GameObject obj1, GameObject obj2)
    {
        return obj1.GetComponent<Renderer>().material.color == obj2.GetComponent<Renderer>().material.color;
    }
}
