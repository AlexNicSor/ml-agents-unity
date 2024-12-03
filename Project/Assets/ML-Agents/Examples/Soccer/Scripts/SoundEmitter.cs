using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [Tooltip("The maximum volume of the sound.")]
    public float maxVolume = 1.0f;

    [Tooltip("The maximum range of the sound.")]
    public float maxRange = 20.0f;

    public Vector3 GetSound(Vector3 listenerPosition)
    {
        Vector3 direction = transform.position - listenerPosition;
        float distance = direction.magnitude;

        if (distance > maxRange || maxVolume <= 0.0f)
            return Vector3.zero; // No sound beyond max range or if muted

        float intensity = maxVolume * (1.0f - (distance / maxRange));
        Vector3 sound = direction.normalized * intensity;

        Debug.Log($"SoundEmitter {name}: Sound = {sound}, Intensity = {intensity}, Distance = {distance}");
        return sound;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.2f); 
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }
}