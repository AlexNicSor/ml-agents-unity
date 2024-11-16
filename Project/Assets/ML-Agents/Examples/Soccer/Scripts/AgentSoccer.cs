using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Soccer;
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

    EnvironmentParameters m_ResetParams;

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

        m_ResetParams = Academy.Instance.EnvironmentParameters;
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

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * m_ForwardSpeed;
                m_KickPower = 1f;
                break;
            case 2:
                dirToGo = transform.forward * -m_ForwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * m_LateralSpeed;
                break;
            case 2:
                dirToGo = transform.right * -m_LateralSpeed;
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

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
            ForceMode.VelocityChange);
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
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
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
            AddReward(.2f * m_BallTouch);
            var dir = c.contacts[0].point - transform.position;
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }

    public override void OnEpisodeBegin()
    {
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }

}
