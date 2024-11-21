using UnityEngine;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Collider))]
public class SoundSensor : SensorComponent
{

    public Vector3 SoundDirection { get; private set; } = Vector3.zero;
    public float SoundIntensity { get; private set; } = 0f;
    public Vector3 RelativeVelocity { get; private set; } = Vector3.zero;

    [Tooltip("Layer mask to filter which objects emit sound.")]
    public LayerMask soundEmitterLayer;

    [Tooltip("Detection radius to consider sound emitters.")]
    public float detectionRadius = 10f;

    private Vector3 soundDirection = Vector3.zero;

    public override ISensor[] CreateSensors()
    {
        // Create a VectorSensor with 3 dimensions for X, Y, Z sound direction
        return new ISensor[] { new VectorSensor(3, $"{name}_SoundSensor") };
    }

    private void Update()
    {
        // Regularly update sound detection logic
        UpdateSoundDetection();
    }

    public void UpdateSoundDetection()
    {
        Collider[] emitters = Physics.OverlapSphere(transform.position, detectionRadius, soundEmitterLayer);

        if (emitters.Length > 0)
        {
            Collider closestEmitter = null;
            float closestDistance = Mathf.Infinity;

            foreach (var emitter in emitters)
            {
                float distance = Vector3.Distance(transform.position, emitter.transform.position);

                // Calculate sound intensity
                float intensity = 1.0f / (1.0f + distance * distance);

                // Check for Rigidbody to compute relative velocity
                Rigidbody emitterRigidbody = emitter.GetComponent<Rigidbody>();
                Vector3 relativeVelocity = Vector3.zero;
                if (emitterRigidbody != null)
                {
                    relativeVelocity = emitterRigidbody.velocity - GetComponent<Rigidbody>().velocity;
                }

                // Find the closest emitter
                if (distance < closestDistance)
                {
                    closestEmitter = emitter;
                    closestDistance = distance;
                    SoundIntensity = intensity;
                    RelativeVelocity = relativeVelocity;
                }
            }

            if (closestEmitter != null)
            {
                SoundDirection = (closestEmitter.transform.position - transform.position).normalized;
            }
        }
        else
        {
            SoundDirection = Vector3.zero;
            SoundIntensity = 0f;
            RelativeVelocity = Vector3.zero;
        }
    }
    public Vector3 GetSoundDirection()
    {
        Debug.Log($"SoundSensor {name}: Current soundDirection = {soundDirection}");
        return soundDirection;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the detection radius for debugging
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}