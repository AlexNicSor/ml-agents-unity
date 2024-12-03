using UnityEngine;

public class SoundSoccerBallController : MonoBehaviour
{
    public GameObject area;
    [HideInInspector]
    public SoundSoccerEnvController envController;
    private SoundEmitter soundEmitter;

    public string purpleGoalTag;
    public string blueGoalTag;

    public static bool BallTouched { get; private set; } = false; // Static flag

    void Start()
    {
        ResetBallTouched();
        envController = area.GetComponent<SoundSoccerEnvController>();
        soundEmitter = GetComponent<SoundEmitter>();
        soundEmitter.maxVolume = 0.1f; 
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag(purpleGoalTag))
        {
            envController.GoalTouched(Team.Blue);
        }
        if (col.gameObject.CompareTag(blueGoalTag))
        {
            envController.GoalTouched(Team.Purple);
        }

        // Mark ball as touched
        BallTouched = true;

        // Emit sound when ball is touched
        if (soundEmitter != null)
        {
            soundEmitter.maxVolume = 1.0f; // Activate sound
        }
    }

    public static void ResetBallTouched()
    {
        BallTouched = false;
    }

    public void ResetSoundEmitter()
    {
        if (soundEmitter != null)
        {
            soundEmitter.maxVolume = 0.1f; // Reset sound
        }
    }
}

