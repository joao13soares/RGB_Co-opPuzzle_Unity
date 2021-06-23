using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PadBehavior : MonoBehaviour
{
    [SerializeField] PlayersColors.Colors currentColor;
    PlayersColors.Colors defaultColor;

    void Start()
    {
        defaultColor = currentColor;
    }

    void Update()
    {

    }

    public void SwitchColor()
    {
        currentColor = (int)currentColor >= PlayersColors.colors.Length - 1 ? 0 : currentColor + 1;
        ApplyCurrentColor();
    }

    public void ResetToDefaultColor()
    {
        currentColor = defaultColor;
        ApplyCurrentColor();
    }

    private void ApplyCurrentColor()
    {
        this.GetComponent<Renderer>().material = PlayersColors.colors[(int)currentColor];
    }
}

