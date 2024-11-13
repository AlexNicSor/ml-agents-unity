using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public enum FieldZone
{
    PurpleGoal,
    Middle,
    BlueGoal
}

public class SoccerEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public AgentSoccer Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;
    public GameObject ball;
    public float purpleGoalLineX = 6f;  // Purple goal line
    public float blueGoalLineX = -6f; // Blue goal line


    private int m_ResetTimer;
    private SoccerSettings m_SoccerSettings;
    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;
    private Vector3 m_BallStartingPos;

    [HideInInspector]
    public Rigidbody ballRb;
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    void Start()
    {
        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        m_BlueAgentGroup = new SimpleMultiAgentGroup();
        m_PurpleAgentGroup = new SimpleMultiAgentGroup();
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = ball.transform.position;

        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();

            // Ensure the team is correctly assigned for each agent
            if (item.Agent.team == Team.Blue)
            {
                m_BlueAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                m_PurpleAgentGroup.RegisterAgent(item.Agent);
            }
        }
        ResetScene();
    }


    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_BlueAgentGroup.GroupEpisodeInterrupted();
            m_PurpleAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    public FieldZone GetBallZone()
    {
        // Debugging the ball's position and which zone it's in
        Debug.Log($"Ball position: {ball.transform.position.x}");

        if (ball.transform.position.x > purpleGoalLineX)
        {
            Debug.Log("Ball is in PurpleGoal zone.");
            return FieldZone.PurpleGoal;
        }
        if (ball.transform.position.x < blueGoalLineX)
        {
            Debug.Log("Ball is in BlueGoal zone.");
            return FieldZone.BlueGoal;
        }
        else
        {
            Debug.Log("Ball is in Middle zone.");
            return FieldZone.Middle;
        }
    }


    public void ResetScene()
    {
        m_ResetTimer = 0;
        foreach (var item in AgentsList)
        {
            var randomPosX = Random.Range(-5f, 5f);
            var newStartPos = item.Agent.initialPos + new Vector3(randomPosX, 0f, 0f);
            var rot = item.Agent.rotSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);

            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }
        ResetBall();
    }

    public void ResetBall()
    {
        ball.transform.position = m_BallStartingPos;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
    }

    public void GoalTouched(Team scoredTeam)
    {
        if (scoredTeam == Team.Blue)
        {
            m_BlueAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_PurpleAgentGroup.AddGroupReward(-1);
        }
        else
        {
            m_PurpleAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_BlueAgentGroup.AddGroupReward(-1);
        }
        m_BlueAgentGroup.EndGroupEpisode();
        m_PurpleAgentGroup.EndGroupEpisode();
        ResetScene();
    }
}