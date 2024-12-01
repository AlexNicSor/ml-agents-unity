using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

//decouple the agent from the ball

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

    [HideInInspector]
    public Team team;
    float m_KickPower;
    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    EnvironmentParameters m_ResetParams;

    public GameObject ball; // Reference to the ball GameObject

    private bool isSearching = false;

    public override void Initialize()
    {
        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();
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
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.0f;
        }
        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }

    private float timer = 0f;
    private bool turn1 = false;
    private bool turn2 = false;
    private bool turn3 = false;
    private bool turn4 = false;
    
    void Update()
    {
        if (!isSearching)  
        {
            timer += Time.deltaTime;

            if (timer >= 3f && timer < 3.3f && !turn1)  
            {
                transform.Rotate(Vector3.up * 90f); 
                turn1 = true;
            }
            else if (timer >= 3.3f && timer < 3.6f && !turn2)  
            {
                transform.Rotate(Vector3.up * -90f);  
                turn2 = true;
            }
            else if (timer >= 3.6f && timer < 3.9f && !turn3)  
            {
                transform.Rotate(Vector3.up * -90f);  
                turn3 = true;
            }
            else if (timer >= 3.9f && timer < 4.2f && !turn4)  
            {
                transform.Rotate(Vector3.up * 90f);  
                turn4 = true;
            }
            else if (timer >= 4.2f)  
            {
                timer = 0f;
                turn1 = false;
                turn2 = false;
                turn3 = false;
                turn4 = false;
            }
        }
    }

   
    private bool IsBallVisible()
    {
        if (ball != null)
        {
            float distance = Vector3.Distance(transform.position, ball.transform.position);
            return distance < 20f;
        }
        return false;
    }

    private Vector3 GetSearchRotation()
    {
        isSearching = true;
        float rotationAmount = 0f;
        
        if (timer >= 0f && timer < 0.3f)
        {
            rotationAmount = 90f;
        }
        else if (timer >= 0.3f && timer < 0.6f)
        {
            rotationAmount = -90f;
        }
        else if (timer >= 0.6f && timer < 0.9f)
        {
            rotationAmount = -90f;
        }
        else if (timer >= 0.9f && timer < 1.2f)
        {
            rotationAmount = 90f;
        }
        else
        {
            timer = 0f;
        }
        
        timer += Time.deltaTime;
        return Vector3.up * rotationAmount;
    }

    
    private Vector3 CalculateMovementDirection(int forwardAxis, int rightAxis)
    {
        var dirToGo = Vector3.zero;
        m_KickPower = 0f;

        if (forwardAxis == 1)
        {
            dirToGo = transform.forward * m_ForwardSpeed;
            m_KickPower = 1f;
        }
        else if (forwardAxis == 2)
        {
            dirToGo = transform.forward * -m_ForwardSpeed;
        }

        if (rightAxis == 1)
        {
            dirToGo += transform.right * m_LateralSpeed;
        }
        else if (rightAxis == 2)
        {
            dirToGo += transform.right * -m_LateralSpeed;
        }

        return dirToGo;
    }

    // Decision system
    public void MoveAgent(ActionSegment<int> act)
    {
        var forwardAxis = act[0];
        var rightAxis = act[1];
        
        bool canSeeBall = IsBallVisible();
        if (canSeeBall)
        {
            isSearching = false;  // Reset searching when ball is found
        }
        Vector3 rotateDir = canSeeBall ? Vector3.zero : GetSearchRotation();
        Vector3 dirToGo = CalculateMovementDirection(forwardAxis, rightAxis);

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)

    {

        if (position == Position.Goalie)
        {
            // Existential bonus for Goalies.
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            // Existential penalty for Strikers
            AddReward(-m_Existential);
        }
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
