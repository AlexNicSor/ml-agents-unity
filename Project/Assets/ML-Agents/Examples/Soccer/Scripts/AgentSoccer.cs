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
        // Get the current field zone based on the ball's position
        FieldZone ballZone = envController.GetBallZone();

        // Debugging ball position and agent's current role
        Debug.Log($"{team} - {position} | Ball is in {ballZone} zone.");

        // Check if the agent is on the correct team and position
        if (team == Team.Blue)
        {
            // If the ball is in the purple goal zone, Blue agent should switch to Striker
            if (ballZone == FieldZone.PurpleGoal)
            {
                position = Position.Striker;  // Switch to generic role
                isActingAsGoalie = false;
                Debug.Log("Blue agent switched to Striker, ball is in opponent's goal zone.");
            }
            // If the ball is in the blue goal zone, Blue agent should remain as Goalie
            else if (ballZone == FieldZone.BlueGoal)
            {
                position = Position.Goalie;  // Switch to goalie role
                isActingAsGoalie = true;
                Debug.Log("Blue agent switched to Goalie, ball is in their goal zone.");
            }
        }
        else if (team == Team.Purple)
        {
            // If the ball is in the blue goal zone, Purple agent should switch to Striker
            if (ballZone == FieldZone.BlueGoal)
            {
                position = Position.Striker;  // Switch to generic role
                isActingAsGoalie = false;
                Debug.Log("Purple agent switched to Striker, ball is in opponent's goal zone.");
            }
            // If the ball is in the purple goal zone, Purple agent should remain as Goalie
            else if (ballZone == FieldZone.PurpleGoal)
            {
                position = Position.Goalie;  // Switch to goalie role
                isActingAsGoalie = true;
                Debug.Log("Purple agent switched to Goalie, ball is in their goal zone.");
            }
        }

        // Log the final role after evaluation
        Debug.Log($"Final position for {team}: {position} (Goalie={isActingAsGoalie})");
    }

    // Method to calculate rewards
    private void CalculateRewards()
    {
        Vector3 ballPosition = envController.ball.transform.position;
        float distanceToBall = Vector3.Distance(transform.position, ballPosition);
        FieldZone ballZone = envController.GetBallZone();

        // For goalkeepers
        if (isActingAsGoalie)
        {
            // Reward for staying on the goalie side of the field (defending)
            if ((team == Team.Blue && ballZone != FieldZone.BlueGoal) || (team == Team.Purple && ballZone != FieldZone.PurpleGoal))
            {
                AddReward(0.1f); // Staying on the defensive side
            }

            // Reward for clearing the ball toward the opponent's goal
            if ((team == Team.Blue && ballPosition.x < envController.purpleGoalLineX) || (team == Team.Purple && ballPosition.x > envController.blueGoalLineX))
            {
                AddReward(0.5f); // Successfully sending the ball to the opponent's side
                Debug.Log($"{team} goalie sent the ball to the opponent's side.");
            }
        }

        // For strikers
        if (!isActingAsGoalie)
        {
            // Reward for pushing the ball towards the enemy goal
            if ((team == Team.Blue && ballPosition.x > envController.purpleGoalLineX) || (team == Team.Purple && ballPosition.x < envController.blueGoalLineX))
            {
                AddReward(0.5f); // Ball is moving toward the enemy goal
                Debug.Log($"{team} striker pushed the ball towards the enemy goal.");
            }
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
        // Adapt the agent's role based on the ball position
        AdaptRoleBasedOnBallPosition();

        // Debugging the agent's position and role before applying rewards
        Debug.Log($"{team} - {position} | Current Agent Position: {transform.position} | Ball Position: {envController.ball.transform.position}");

        // Ensure reward values are finite before applying
        if (float.IsInfinity(m_Existential) || float.IsNaN(m_Existential))
        {
            Debug.LogWarning("Invalid reward value detected, skipping reward update.");
            return; // Skip adding rewards if the value is invalid
        }

        // Calculate and apply rewards based on the agent's role
        CalculateRewards();

        // Move the agent based on its decision
        MoveAgent(actionBuffers.DiscreteActions);
    }

    private void LogPositionAndRole()
    {
        Debug.Log($"{team} - {position} at position: {transform.position}");
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Handle heuristic controls
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
        // Ensure MaxStep is greater than zero to avoid infinity
        if (MaxStep > 0)
        {
            m_Existential = 1f / MaxStep;
        }
        else
        {
            m_Existential = 0f; // Default to 0 if MaxStep is zero or invalid
        }
    }
}
