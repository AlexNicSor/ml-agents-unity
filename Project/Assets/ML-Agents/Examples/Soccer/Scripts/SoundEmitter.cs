using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    public float maxVolume = 1.0f;  // Maximum volume of the sound
    public float decayRate = 0.5f;  // Rate at which sound fades over distance
    public float maxHearingRange = 15f; // Maximum distance a sound can be heard

    public float GetVolumeAtDistance(Vector3 listenerPosition)
    {
        // Calculate distance between emitter and listener
        float distance = Vector3.Distance(transform.position, listenerPosition);

        // If outside of max hearing range, return 0
        if (distance > maxHearingRange)
            return 0f;

        // Volume decreases with distance
        return maxVolume * Mathf.Clamp01(1 - (distance / maxHearingRange) * decayRate);
    }
}