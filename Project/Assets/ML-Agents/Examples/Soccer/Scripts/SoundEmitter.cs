using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [Tooltip("The maximum volume of the sound.")]
    public float maxVolume = 1.0f;

    [Tooltip("The maximum range of the sound.")]
    public float maxRange = 15.0f;

    /// <summary>
    /// Computes the sound intensity and direction relative to a listener's position.
    /// </summary>
    public Vector3 GetSound(Vector3 listenerPosition)
    {
        Vector3 direction = transform.position - listenerPosition;
        float distance = direction.magnitude;

        if (distance > maxRange)
            return Vector3.zero; // No sound beyond max range

        float intensity = maxVolume * (1.0f - (distance / maxRange));
        return direction.normalized * intensity;
    }

    private void OnDrawGizmos()
    {
        // Visualize the emitter's range for debugging
        Gizmos.color = new Color(1, 0, 0, 0.2f); // Transparent red
        //Gizmos.DrawSphere(transform.position, maxRange);
    }
    
}
