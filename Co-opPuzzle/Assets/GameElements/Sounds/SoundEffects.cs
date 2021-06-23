using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour
{
    public enum Sounds
    {
        Jump,
        Pickup,
        Checkpoint,
        JumpPad,
        Damage,
        SwitchColors,
        LevelEndPress,
        LevelCompleted,
        GameOver
    }
    
    [SerializeField] AudioClip[] soundEffects;

    public void PlaySoundEffect(Sounds sound)
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffects[(int)sound]);
    }
}
