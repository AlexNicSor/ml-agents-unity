using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Soccer;
using Unity.Sentis.Layers;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
public enum Team
{
    Blue = 0,
    Purple = 1
}

public class AgentSoccer : Agent
{
    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player
    public enum Position
    {
        Striker,
        Goalie,
        Generic
    }
    public Team team;

    [HideInInspector]
    float m_KickPower;
    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;
    private SoccerEnvController envController;

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    [Header("Vision Settings")]
    public float visionRange = 10f;
    public float visionAngle = 90f;
    public float visionRotationSpeed = 45f;
    private float currentVisionAngle = 0f;

    [SerializeField]
    private Transform visionConeTransform;

    [SerializeField]
    private LayerMask visionMask;
    
    EnvironmentParameters m_ResetParams;

    public GameObject ball; // Reference to the ball GameObject
    //private bool isSearching = false;



    // private float timer = 0f;
    // private bool turn1 = false;
    // private bool turn2 = false;
    // private bool turn3 = false;
    // private bool turn4 = false;

    [Header("Debug Visualization")]
    public bool showRotationDebug = true;
    public float debugLineLength = 2f;

    private RayPerceptionSensorComponent3D rayPerceptionSensor;

    public override void Initialize()
    {
        envController = GetComponentInParent<SoccerEnvController>();
        
        if (envController != null)
        {
            m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            m_Existential = 1f / MaxStep;
        }

        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.position.x - 5f, .5f, transform.position.z);
            rotSign = 1f;
        }
        else
        {
            team = Team.Purple;
            initialPos = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotSign = -1f;
        }
        if (position == Position.Goalie)
        {
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.0f;
        }
        else if (position == Position.Striker)
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.3f;
        }
        else
        {
            m_LateralSpeed = 0.5f;
            m_ForwardSpeed = 1.0f;
        }
        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;
        
        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        var discreteActionSpec = ActionSpec.MakeDiscrete(3, 3, 3, 3);
        m_BehaviorParameters.BrainParameters.ActionSpec = discreteActionSpec;

    // Initialize vision cone
    if (visionConeTransform == null)
    {
        visionConeTransform = transform.Find("VisionCone");
    }
    currentVisionAngle = transform.eulerAngles.y;

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        // Update how we find the ray perception sensor
        if (visionConeTransform != null)
        {
            rayPerceptionSensor = visionConeTransform.GetComponent<RayPerceptionSensorComponent3D>();
            if (rayPerceptionSensor == null)
            {
                rayPerceptionSensor = visionConeTransform.GetComponentInChildren<RayPerceptionSensorComponent3D>();
            }
        }

        if (rayPerceptionSensor == null)
        {
            Debug.LogError("RayPerceptionSensorComponent3D not found on vision cone or its children!");
        }
    }

    private void AdaptRoleBasedOnBallPosition()
    {
        FieldZone ballZone = envController.GetBallZone();
        Debug.Log($"{team} - {position} | Ball is in {ballZone} zone.");

        if (team == Team.Blue)
        {
            if (ballZone == FieldZone.BlueGoal)
            {
                position = Position.Goalie;
            }
            else
            {
                position = Position.Striker;
            }
        }
        else
        {
            if (ballZone == FieldZone.PurpleGoal)
            {
                position = Position.Goalie;
            }
            else
            {
                position = Position.Striker;
            }
        }
    }




    private void AdaptRoleBasedOnBallPositionAndTeammates()
    {//print agents list
        foreach (var player in envController.AgentsList)
        {
            if (player == null) continue;
            Debug.Log($"Agent: {player.Agent.name} | Team: {player.Agent.team} | Position: {player.Agent.position}");
        }
        if (envController == null)
        {
            Debug.LogError("envController is null!");
            return;
        }

        FieldZone ballZone = envController.GetBallZone();

        // Null check for ball
        if (envController.ball == null)
        {
            Debug.LogError("Ball object is null!");
            return;
        }

        float closestDistanceToBall = Mathf.Infinity;
        AgentSoccer closeAgentBlue = null;
        AgentSoccer secondAgentBlue = null;
        AgentSoccer closeAgentPurple = null;
        AgentSoccer secondAgentPurple = null;


        // Null check for AgentsList
        if (envController.AgentsList == null || envController.AgentsList.Count == 0)
        {
            Debug.LogError("AgentsList is empty or null!");
            return;
        }

        // Find the closest agent to the ball for each team
        foreach (var player in envController.AgentsList)
        {
            if (player == null) continue;
            if (player.Agent.team == Team.Blue)
            {
                float distance = Vector3.Distance(player.Agent.transform.position, envController.ball.transform.position);
                if (distance < closestDistanceToBall)
                {
                    secondAgentBlue = closeAgentBlue;
                    closeAgentBlue = player.Agent;
                    closestDistanceToBall = distance;
                }
            }
            else
            {
                float distance = Vector3.Distance(player.Agent.transform.position, envController.ball.transform.position);
                if (distance < closestDistanceToBall)
                {
                    secondAgentPurple = closeAgentPurple;
                    closeAgentPurple = player.Agent;
                    closestDistanceToBall = distance;
                }
            }
        }

        // Adapt the role based on the ball's position and the closest agent to the ball
        // if ball close to own goal, second agent is goalie, closest agent is generic
        // if ball close to opposing goal, second agent is striker, closest agent is generic
        // if ball in middle, closest agent is striker, second agent is goalie
        if (team == Team.Blue)
        {
            if (ballZone == FieldZone.BlueGoal)
            {
                if (closeAgentBlue == this)
                {
                    position = Position.Goalie;
                }
                else if (secondAgentBlue == this)
                {
                    position = Position.Striker;
                }
                else
                {
                    position = Position.Generic;
                }
            }
            else if (ballZone == FieldZone.PurpleGoal)
            {
                if (closeAgentPurple == this)
                {
                    position = Position.Striker;
                }
                else if (secondAgentPurple == this)
                {
                    position = Position.Goalie;
                }
                else
                {
                    position = Position.Generic;
                }
            }
            else
            {
                if (closeAgentBlue == this)
                {
                    position = Position.Striker;
                }
                else if (secondAgentBlue == this)
                {
                    position = Position.Goalie;
                }
                else
                {
                    position = Position.Generic;
                }
            }
        }
        else
        {
            if (ballZone == FieldZone.PurpleGoal)
            {
                if (closeAgentPurple == this)
                {
                    position = Position.Goalie;
                }
                else if (secondAgentPurple == this)
                {
                    position = Position.Striker;
                }
                else
                {
                    position = Position.Generic;
                }
            }
            else if (ballZone == FieldZone.BlueGoal)
            {
                if (closeAgentBlue == this)
                {
                    position = Position.Striker;
                }
                else if (secondAgentBlue == this)
                {
                    position = Position.Goalie;
                }
                else
                {
                    position = Position.Generic;
                }
            }
            else
            {
                if (closeAgentPurple == this)
                {
                    position = Position.Striker;
                }
                else if (secondAgentPurple == this)
                {
                    position = Position.Goalie;
                }
                else
                {
                    position = Position.Generic;
                }
            }
        }



    }


    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];
        var visionConeAxis = act[3];

        // Apply team-specific direction for forward/backward movement
        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * m_ForwardSpeed * rotSign; // Use rotSign to flip direction
                m_KickPower = 1f;
                break;
            case 2:
                dirToGo = transform.forward * -m_ForwardSpeed * rotSign; // Use rotSign to flip direction
                break;
        }

        // Right/Left movement might also need to be flipped depending on your field layout
        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * m_LateralSpeed * rotSign; // Use rotSign to flip direction
                break;
            case 2:
                dirToGo = transform.right * -m_LateralSpeed * rotSign; // Use rotSign to flip direction
                break;
        }

      
        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

      
        switch (visionConeAxis)
        {
            case 1:
                AdjustVisionCone(1);
                break;
            case 2:
                AdjustVisionCone(-1);
                break;
            default:
                AdjustVisionCone(0);
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
            ForceMode.VelocityChange);
    }
    private void AdjustVisionCone(int direction)
    {
        currentVisionAngle += direction * visionRotationSpeed * Time.deltaTime;
        
        // Update vision cone visualization
        if (visionConeTransform != null)
        {
            visionConeTransform.localRotation = Quaternion.Euler(0, currentVisionAngle, 0);
        }
    }

    // New method to check if the ball is visible
    private bool IsBallVisible()
    {
        if (ball != null)
        {
            float distance = Vector3.Distance(transform.position, ball.transform.position);
            return distance < 20f; // Adjust the distance threshold as needed
        }
        return false;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        if (position == Position.Goalie)
        {
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.0f;
            // Existential bonus for Goalies.
            AddReward(m_Existential);
            //print m_Existential
            Debug.Log($"Existential: {m_Existential}");

            if (team == Team.Blue)
            {
                if (envController.GetBallZone() == FieldZone.BlueGoal)
                {
                    AddReward(-0.1f);
                }
            }
            else
            {
                if (envController.GetBallZone() == FieldZone.PurpleGoal)
                {
                    AddReward(-0.1f);
                }
            }
        }
        else if (position == Position.Striker)
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.3f;
            // Existential penalty for Strikers
            AddReward(-m_Existential);
            Debug.Log($"Existential: {m_Existential}");
            if (team == Team.Blue)
            {
                if (envController.GetBallZone() == FieldZone.PurpleGoal)
                {
                    AddReward(0.1f);
                }
            }
            else
            {
                if (envController.GetBallZone() == FieldZone.BlueGoal)
                {
                    AddReward(0.1f);
                }
            }
        }
        else
        {
            m_LateralSpeed = 0.5f;
            m_ForwardSpeed = 1.0f;
        }
        //AdaptRoleBasedOnBallPosition();
        AdaptRoleBasedOnBallPositionAndTeammates();
        MoveAgent(actionBuffers.DiscreteActions);
    }

     public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        // Initialize all actions to 0 (no action)
        for (int i = 0; i < discreteActionsOut.Length; i++)
        {
            discreteActionsOut[i] = 0;
        }

        // Movement Controls
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;  // Forward
            Debug.Log("Forward");
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;  // Backward
            Debug.Log("Backward");
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;  // Rotate Left
            Debug.Log("Rotate Left");
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;  // Rotate Right
            Debug.Log("Rotate Right");
        }
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;  // Strafe Right
            Debug.Log("Strafe Right");
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;  // Strafe Left
            Debug.Log("Strafe Left");
        }

        // Vision Controls
        if (Input.GetKey(KeyCode.Z))
        {
            discreteActionsOut[3] = 1;  // Vision Left
            Debug.Log("Vision Left");
        }
        if (Input.GetKey(KeyCode.C))
        {
            discreteActionsOut[3] = 2;  // Vision Right
            Debug.Log("Vision Right");
        }

     
    }
    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        var force = k_Power * m_KickPower;
        if (position == Position.Goalie)
        {
            force = k_Power;
        }
        if (c.gameObject.CompareTag("ball"))
        {
            AddReward(.5f * m_BallTouch);
            var dir = c.contacts[0].point - transform.position;
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);


        //reward for passing the ball to teammates in their field
        //gets the positions of the ball and checks the current and last possessor 
        var previousPlayer = envController.GetLastPossessor();
        envController.SetCurrentPossessor(this);

        //if there is successfull pass 
        if (previousPlayer != null && previousPlayer != this){

             // if ball goes to the teammate  
            if(previousPlayer.team == team){
                //it happens in the enemy field
                if ((envController.GetBallZone() == FieldZone.PurpleGoal && team == Team.Blue) ||
                    (envController.GetBallZone() == FieldZone.BlueGoal && team == Team.Purple)){
                    AddReward(.05f);
                    envController.SetPassOccured();
                }
            }else {
                envController.ResetPassOccured();
                AddReward(-.5f);
            }
            envController.SetLastPossessor(this);
        }

        }
    }

    public override void OnEpisodeBegin()
    {
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }


    private List<GameObject> nearbyAgents = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("blueAgent") || other.CompareTag("ball") || other.CompareTag("purpleAgent")) && !nearbyAgents.Contains(other.gameObject))
        {
            nearbyAgents.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (nearbyAgents.Contains(other.gameObject))
        {
            nearbyAgents.Remove(other.gameObject);
        }
    }


    // void Update()
    // {
    //     if (!isSearching)  
    //     {
    //         timer += Time.deltaTime;

    //         if (timer >= 3f && timer < 3.3f && !turn1)  
    //         {
    //             transform.Rotate(Vector3.up * 90f); 
    //             turn1 = true;
    //         }
    //         else if (timer >= 3.3f && timer < 3.6f && !turn2)  
    //         {
    //             transform.Rotate(Vector3.up * -90f);  
    //             turn2 = true;
    //         }
    //         else if (timer >= 3.6f && timer < 3.9f && !turn3)  
    //         {
    //             transform.Rotate(Vector3.up * -90f);  
    //             turn3 = true;
    //         }
    //         else if (timer >= 3.9f && timer < 4.2f && !turn4)  
    //         {
    //             transform.Rotate(Vector3.up * 90f);  
    //             turn4 = true;
    //         }
    //         else if (timer >= 4.2f)  
    //         {
    //             timer = 0f;
    //             turn1 = false;
    //             turn2 = false;
    //             turn3 = false;
    //             turn4 = false;
    //         }
    //     }
    // }

    public override void CollectObservations(VectorSensor sensor)
    {
        int countObservations = 0;
        if (nearbyAgents.Count == 0)
        {
            sensor.AddObservation(0);
            sensor.AddObservation(Vector3.zero);
            countObservations++;
        }

        foreach (GameObject nearbyAgent in nearbyAgents)
        {
            if (IsBallVisible())
            {
            AddReward(0.1f);  // Reward for seeing the ball
            }
            // float angleToBall = Vector3.Angle(visionConeTransform.forward, ball.transform.position - transform.position);
            // if (angleToBall < visionAngle / 2)
            // {
            // AddReward(0.05f);  // Reward for keeping the ball centered
            // }
               if (!IsBallVisible())
            {
            AddReward(-0.1f);  // Penalty for losing sight of the ball
            }

            if (nearbyAgent.CompareTag("ball")) // reward for being near the ball
            {
                AddReward(0.2f);
            }
            
            // if ball moves towards opponent's goal, give a small reward
            // otherwise, give a small penalty
            if (nearbyAgent.gameObject.CompareTag("ball"))
            {
                // get the blue and purple goals
                GameObject blueGoal = GameObject.FindGameObjectsWithTag("blueGoal")[0];
                GameObject purpleGoal = GameObject.FindGameObjectsWithTag("purpleGoal")[0];
                // ball's moving direction
                Vector3 movingDir = nearbyAgent.gameObject.GetComponent<Rigidbody>().velocity.normalized;
                // direction to blue and purple goals from ball
                Vector3 dirToBlueGoal = (blueGoal.transform.position - nearbyAgent.transform.position).normalized;
                Vector3 dirToPurpleGoal = (purpleGoal.transform.position - nearbyAgent.transform.position).normalized;

                if ((Vector3.Dot(movingDir, dirToBlueGoal) > 0 && team == Team.Purple) || (Vector3.Dot(movingDir, dirToPurpleGoal) > 0 && team == Team.Blue))
                {
                    AddReward(0.2f);
                }
                else if ((Vector3.Dot(movingDir, dirToBlueGoal) < 0 && team == Team.Purple) || (Vector3.Dot(movingDir, dirToPurpleGoal) < 0 && team == Team.Blue))
                {
                    AddReward(-0.2f);
                }
            }

            Vector3 relativePosition = transform.InverseTransformPoint(nearbyAgent.transform.position);
            sensor.AddObservation(nearbyAgent.CompareTag("ball") ? 1 : nearbyAgent.CompareTag("blueAgent") ? 2 : 3);
            sensor.AddObservation(relativePosition);
            countObservations++;
        }

        while (countObservations < 4) // adds observations until there are 16 total across all agents (4 per agent)
        {
            sensor.AddObservation(0);
            sensor.AddObservation(Vector3.zero);
            countObservations++;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showRotationDebug) return;

        // Body direction (world space) - Blue
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * debugLineLength);

        // Vision direction (combined rotation) - Red
        Gizmos.color = Color.red;
        if (visionConeTransform != null)
        {
            Gizmos.DrawLine(transform.position, 
                transform.position + visionConeTransform.forward * debugLineLength);
        }

        // Draw an arc to show the vision angle
        Gizmos.color = Color.yellow;
        DrawVisionArc();
    }

    private void DrawVisionArc()
    {
        if (visionConeTransform == null) return;

        float radius = debugLineLength;
        int segments = 20;
        float halfAngle = visionAngle / 2;
        
        Vector3 forward = visionConeTransform.forward;
        Vector3 right = visionConeTransform.right;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = -halfAngle + (i * visionAngle / segments);
            float angle2 = -halfAngle + ((i + 1) * visionAngle / segments);
            
            Vector3 direction1 = Quaternion.AngleAxis(angle1, Vector3.up) * forward;
            Vector3 direction2 = Quaternion.AngleAxis(angle2, Vector3.up) * forward;
            
            Gizmos.DrawLine(
                transform.position + direction1 * radius,
                transform.position + direction2 * radius
            );
        }
    }

}
