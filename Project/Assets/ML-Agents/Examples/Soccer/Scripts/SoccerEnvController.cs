using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using Soccer;
using Unity.Sentis.Layers;
using System;
using Random = UnityEngine.Random;
using System.Threading;
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





    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    /// <summary>
    /// The area bounds.
    /// </summary>

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>

    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;
    Vector3 m_BallStartingPos;

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    private SoccerSettings m_SoccerSettings;
    public float purpleGoalLineX = 6f;  // Purple goal line
    public float blueGoalLineX = -6f;

    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;

    private AgentSoccer lastPlayer = null;
    private AgentSoccer currentPlayer = null;

    private int m_ResetTimer;

    private bool passBeforeGoal = false; 


   
    

    void Start()
    {
        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        // Initialize TeamManager
        m_BlueAgentGroup = new SimpleMultiAgentGroup();
        m_PurpleAgentGroup = new SimpleMultiAgentGroup();
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);
        foreach (var item in AgentsList)
        {
            item.Agent.ball = ball;
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
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


    public void ResetBall()
    {
        var randomPosX = Random.Range(-2.5f, 2.5f);
        var randomPosZ = Random.Range(-2.5f, 2.5f);

        ball.transform.position = m_BallStartingPos + new Vector3(randomPosX, 0f, randomPosZ);
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

    }

    public void GoalTouched(Team scoredTeam)
    {
        //base reward
        float baseReward = Math.Max((3- (float)m_ResetTimer/MaxEnvironmentSteps), 1.5f);

        //bonus points for passing before the goal
        float passBonus = passBeforeGoal? .5f : 0f;


        if (scoredTeam == Team.Blue)
        {
            m_BlueAgentGroup.AddGroupReward(baseReward + passBonus);
            m_PurpleAgentGroup.AddGroupReward(-3);
        }
        else
        {
            m_PurpleAgentGroup.AddGroupReward(baseReward + passBonus);
            m_BlueAgentGroup.AddGroupReward(-3);

        }
        m_PurpleAgentGroup.EndGroupEpisode();
        m_BlueAgentGroup.EndGroupEpisode();
        ResetScene();

    }


    public void ResetScene()
    {
        m_ResetTimer = 0;

        //Reset Agents
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

        //Reset Ball
        ResetBall();
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
    public AgentSoccer GetCurrentPossessor(){
        return currentPlayer;
    }

    public void SetCurrentPossessor(AgentSoccer player){
        currentPlayer = player;
    }

    public AgentSoccer GetLastPossessor(){
        return lastPlayer;
    }

    public void SetLastPossessor(AgentSoccer player){
        lastPlayer = player;
    }
    public void SetPassOccured(){
        passBeforeGoal = true;
    }
    public void ResetPassOccured(){
        passBeforeGoal = false;
    }


}
