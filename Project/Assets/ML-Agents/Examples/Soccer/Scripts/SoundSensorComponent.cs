// using UnityEngine;
// using Unity.MLAgents.Sensors;

// [RequireComponent(typeof(SoundSensor))]
// public class SoundSensorComponent : SensorComponent
// {
//     public LayerMask soundEmitterLayer;
//     public float detectionRadius = 15.0f;

//     private SoundSensor soundSensor;

//     public override ISensor[] CreateSensors()
// {
//     soundSensor = new SoundSensor(); // Use default constructor
//     soundSensor.soundEmitterLayer = soundEmitterLayer; // Pass layer info
//     soundSensor.detectionRadius = detectionRadius; // Pass radius info
//     return new ISensor[] { soundSensor };
// }

//     private void FixedUpdate()
//     {
//         if (soundSensor != null)
//         {
//             soundSensor.Update();
//         }
//     }

//     public void SetSoundDirection(Vector3 direction)
//     {
//         if (soundSensor != null)
//         {
//             soundSensor.SetSoundDirection(direction);
//         }
//     }
// }