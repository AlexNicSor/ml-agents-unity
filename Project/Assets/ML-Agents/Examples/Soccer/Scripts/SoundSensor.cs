using UnityEngine;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Collider))]
public class SoundSensor : SensorComponent
{
    [Tooltip("Layer mask to filter which objects emit sound.")]
    public LayerMask soundEmitterLayer;

    [Tooltip("Detection radius to consider sound emitters.")]
    public float detectionRadius = 10f;

    private Vector3 soundDirection = Vector3.zero;
    private VectorSensor vectorSensor;

    public override ISensor[] CreateSensors()
    {
        // Create a VectorSensor with 3 dimensions for X, Y, Z sound direction
        vectorSensor = new VectorSensor(3, $"{name}_SoundSensor");
        return new ISensor[] { vectorSensor };
    }

    private void Update()
    {
        // Regularly update sound detection logic
        UpdateSoundDetection();
    }

    private void UpdateSoundDetection()
    {
        // Find all emitters within the detection radius
        Collider[] emitters = Physics.OverlapSphere(transform.position, detectionRadius, soundEmitterLayer);

        if (emitters.Length > 0)
        {
            Collider closestEmitter = null;
            float closestDistance = Mathf.Infinity;

            // Identify the closest emitter
            foreach (var emitter in emitters)
            {
                float distance = Vector3.Distance(transform.position, emitter.transform.position);
                if (distance < closestDistance)
                {
                    closestEmitter = emitter;
                    closestDistance = distance;
                }
            }

            // Compute the direction to the closest emitter
            if (closestEmitter != null)
            {
                soundDirection = (closestEmitter.transform.position - transform.position).normalized;
            }
        }
        else
        {
            soundDirection = Vector3.zero; // No emitters in range
        }

        // Update the vector sensor with the sound direction
        if (vectorSensor != null)
        {
            vectorSensor.AddObservation(soundDirection);
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
