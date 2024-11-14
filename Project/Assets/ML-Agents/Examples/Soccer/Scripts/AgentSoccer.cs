using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class AgentSoccer : Agent
{
    public Team team;
    [SerializeField] private Position position;  // Now visible in the Inspector
    public float rotSign;
    public Vector3 initialPos;

    private Rigidbody agentRb;
    private SoccerEnvController envController;
    private bool isActingAsGoalie = true;
    private float m_Existential;
    private float m_LateralSpeed;
    private float m_ForwardSpeed;

    public enum Position
    {
        Goalie,
        Striker
    }

    public override void Initialize()
    {
        envController = GetComponentInParent<SoccerEnvController>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        // Initial setup based on team and position
        if (team == Team.Blue)
        {
            initialPos = new Vector3(transform.position.x - 5f, 0.5f, transform.position.z);
            rotSign = 1f;
        }
        else
        {
            initialPos = new Vector3(transform.position.x + 5f, 0.5f, transform.position.z);
            rotSign = -1f;
        }

        // Set initial speeds based on position
        if (position == Position.Goalie)
        {
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.0f;
        }
        else
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.0f;
        }
    }

    private void AdaptRoleBasedOnBallPosition()
    {
        FieldZone ballZone = envController.GetBallZone();
        Debug.Log($"{team} - {position} | Ball is in {ballZone} zone.");

        if (team == Team.Blue)
        {
            if (ballZone == FieldZone.PurpleGoal || ballZone == FieldZone.Middle)
            {
                position = Position.Striker;
                isActingAsGoalie = false;
                Debug.Log("Blue agent switched to Striker, ball is in opponent's goal zone.");
            }
            else if (ballZone == FieldZone.BlueGoal)
            {
                position = Position.Goalie;
                isActingAsGoalie = true;
                Debug.Log("Blue agent switched to Goalie, ball is in their goal zone.");
            }
        }
        else if (team == Team.Purple)
        {
            if (ballZone == FieldZone.BlueGoal || ballZone == FieldZone.Middle)
            {
                position = Position.Striker;
                isActingAsGoalie = false;
                Debug.Log("Purple agent switched to Striker, ball is in opponent's goal zone.");
            }
            else if (ballZone == FieldZone.PurpleGoal)
            {
                position = Position.Goalie;
                isActingAsGoalie = true;
                Debug.Log("Purple agent switched to Goalie, ball is in their goal zone.");
            }
        }
        Debug.Log($"Final position for {team}: {position} (Goalie={isActingAsGoalie})");
    }

    // private void CalculateRewards()
    // {
    //     Vector3 ballPosition = envController.ball.transform.position;
    //     float distanceToBall = Vector3.Distance(transform.position, ballPosition);
    //     FieldZone ballZone = envController.GetBallZone();

    //     // Existential reward for surviving steps
    //     AddReward(m_Existential);

    //     // For goalkeepers
    //     if (isActingAsGoalie)
    //     {
    //         if ((team == Team.Blue && ballZone != FieldZone.BlueGoal) || (team == Team.Purple && ballZone != FieldZone.PurpleGoal))
    //         {
    //             AddReward(0.05f); // Staying on the defensive side
    //         }

    //         if ((team == Team.Blue && ballPosition.x < envController.purpleGoalLineX) || (team == Team.Purple && ballPosition.x > envController.blueGoalLineX))
    //         {
    //             AddReward(50f); // Successfully sending the ball to the opponent's side
    //             Debug.Log($"{team} goalie sent the ball to the opponent's side.");
    //         }

    //         // Reward for deflecting or being close to the ball
    //         if (distanceToBall < 1.0f)
    //         {
    //             AddReward(5f);
    //             Debug.Log($"{team} goalie defended the ball!");
    //         }

    //         if (distanceToBall > 1.5f)
    //         {
    //             AddReward(-50f); // Penalty for being too far from the ball
    //         }
    //     }

    //     // For strikers
    //     if (!isActingAsGoalie)
    //     {
    //         if ((team == Team.Blue && ballPosition.x > envController.purpleGoalLineX) || (team == Team.Purple && ballPosition.x < envController.blueGoalLineX))
    //         {
    //             AddReward(50f); // Ball is moving toward the enemy goal
    //             Debug.Log($"{team} striker pushed the ball towards the enemy goal.");
    //         }

    //         // Proximity reward for moving close to the ball
    //         float rewardForDistance = Mathf.Clamp(10f / distanceToBall, 0, 10f);
    //         AddReward(rewardForDistance);
    //         Debug.Log($"{team} striker rewarded for being close to the ball.");

    //         if (distanceToBall > 0.1f)
    //         {
    //             AddReward(-50f); // Penalty for being too far from the ball
    //         }
    //     }

    //     // Penalty for unproductive movement
    //     if (distanceToBall > 1.5f && Vector3.Dot(agentRb.velocity, transform.forward) < 0.5f)
    //     {
    //         AddReward(-0.1f);
    //         Debug.Log($"{team} penalized for unproductive movement.");
    //     }

    //     // Scoring reward and own goal penalty
    //     if (envController.CheckGoal(team))
    //     {
    //         AddReward(200f); // Reward for scoring a goal
    //         Debug.Log($"{team} scored a goal!");
    //     }
    //     if (envController.CheckOwnGoal(team))
    //     {
    //         AddReward(-100f); // Penalty for own-goal
    //         Debug.Log($"{team} made an own goal!");
    //     }
    // }


    private void CalculateRewards()
    {
        Vector3 ballPosition = envController.ball.transform.position;
        float distanceToBall = Vector3.Distance(transform.position, ballPosition);
        FieldZone ballZone = envController.GetBallZone();

        // Existential reward: small reward for each step to encourage agents to stay active.
        AddReward(0.001f);

        // Reward for positioning relative to dynamically assigned roles
        if (isActingAsGoalie)
        {
            // Goalies are rewarded for staying near their goal when the ball is close.
            if ((team == Team.Blue && ballZone == FieldZone.BlueGoal) ||
                (team == Team.Purple && ballZone == FieldZone.PurpleGoal))
            {
                AddReward(0.3f);  // Reward for maintaining defensive position.
            }
            else
            {
                AddReward(-0.1f);  // Penalty if out of position.
            }

            // Reward for approaching the ball within a reasonable range.
            if (distanceToBall < 1.5f)
            {
                AddReward(0.5f);  // Defending closer to the ball as goalie.
            }
            else if (distanceToBall > 3.0f)
            {
                AddReward(-0.2f);  // Penalty if too far from the ball as goalie.
            }
        }
        else
        {
            // Strikers are rewarded for advancing the ball toward the opponent’s goal.
            if ((team == Team.Blue && ballPosition.x > envController.purpleGoalLineX) ||
                (team == Team.Purple && ballPosition.x < envController.blueGoalLineX))
            {
                AddReward(0.5f);  // Ball advancing toward opponent’s goal.
            }

            // Reward for staying close to the ball and taking scoring opportunities.
            if (distanceToBall < 1.5f)
            {
                AddReward(0.2f);  // Reward for staying close to the ball.
            }
            else
            {
                AddReward(-0.05f);  // Mild penalty for drifting too far from the ball.
            }
        }

        // Scoring Rewards and Own Goal Penalties
        if (envController.CheckGoal(team))
        {
            AddReward(1f); // High reward for scoring a goal.
        }
        if (envController.CheckOwnGoal(team))
        {
            AddReward(-1f); // Large penalty for an own goal.
        }
    }


    public void MoveAgent(ActionSegment<int> act)
    {
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        int forwardAxis = act[0];
        int rightAxis = act[1];
        int rotateAxis = act[2];

        if (forwardAxis == 1) dirToGo = transform.forward * m_ForwardSpeed;
        else if (forwardAxis == 2) dirToGo = transform.forward * -m_ForwardSpeed;

        if (rightAxis == 1) dirToGo += transform.right * m_LateralSpeed;
        else if (rightAxis == 2) dirToGo += transform.right * -m_LateralSpeed;

        if (rotateAxis == 1) rotateDir = transform.up * -1f;
        else if (rotateAxis == 2) rotateDir = transform.up * 1f;

        agentRb.AddForce(dirToGo, ForceMode.VelocityChange);
        transform.Rotate(rotateDir, Time.deltaTime * 100f);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        AdaptRoleBasedOnBallPosition();
        Debug.Log($"{team} - {position} | Current Agent Position: {transform.position} | Ball Position: {envController.ball.transform.position}");

        if (float.IsInfinity(m_Existential) || float.IsNaN(m_Existential))
        {
            Debug.LogWarning("Invalid reward value detected, skipping reward update.");
            return; // Skip adding rewards if the value is invalid
        }

        CalculateRewards();
        MoveAgent(actionBuffers.DiscreteActions);
    }

    private void LogPositionAndRole()
    {
        Debug.Log($"{team} - {position} at position: {transform.position}");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 2;
        if (Input.GetKey(KeyCode.A)) discreteActionsOut[2] = 1;
        if (Input.GetKey(KeyCode.D)) discreteActionsOut[2] = 2;
        if (Input.GetKey(KeyCode.E)) discreteActionsOut[1] = 1;
        if (Input.GetKey(KeyCode.Q)) discreteActionsOut[1] = 2;
    }

    public override void OnEpisodeBegin()
    {
        if (MaxStep > 0)
        {
            m_Existential = 0.01f; // Small existential reward per step
        }
        else
        {
            m_Existential = 0f;
        }
    }
}
