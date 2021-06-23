using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersColors : MonoBehaviour
{
    public enum Colors
    {
        Green,
        Red
    }

    GameObject[] colorsObj;
    public static Material[] colors;

    void Start()
    {
        colorsObj = GameObject.FindGameObjectsWithTag("Color");
        colors = new Material[colorsObj.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = colorsObj[i].GetComponent<Renderer>().material;
        }
    }

    public static Material GetColorFromIndex(int index)
    {
        return colors[Mathf.Clamp(index, 0, colors.Length - 1)];
    }
}
