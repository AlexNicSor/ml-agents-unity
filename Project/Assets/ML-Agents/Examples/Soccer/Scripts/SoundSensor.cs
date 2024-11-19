using System.Collections.Generic;
using UnityEngine;

public class SoundSensor : MonoBehaviour
{
    public float hearingThreshold = 0.1f;  // Minimum volume required to "hear" a sound
    private List<SoundEmitter> soundEmitters;  // List of all sound sources in the scene

    void Start()
    {
        // Find all sound emitters in the scene (players and ball)
        soundEmitters = new List<SoundEmitter>(FindObjectsOfType<SoundEmitter>());
    }

    public Vector3 GetSoundDirection()
    {
        Vector3 directionToSound = Vector3.zero;
        float highestVolume = 0f;

        foreach (var emitter in soundEmitters)
        {
            float volume = emitter.GetVolumeAtDistance(transform.position);
            if (volume > hearingThreshold && volume > highestVolume)
            {
                // Calculate direction and volume-weighted vector
                directionToSound = (emitter.transform.position - transform.position).normalized * volume;
                highestVolume = volume;
            }
        }

        return directionToSound;
    }
}