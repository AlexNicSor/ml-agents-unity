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
    private float timer360 = 0f;
    private SoccerEnvController envController;

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;
    
    
    EnvironmentParameters m_ResetParams;

    public GameObject ball; // Reference to the ball GameObject



    private float timer = 0f;
    private bool turn1 = false;
    private bool turn2 = false;
    private bool turn3 = false;
    private bool turn4 = false;

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

        // Check if the ball is visible
        bool ballVisible = IsBallVisible();

        if (!ballVisible)
        {

            float scanSpeed = 5f; // Adjust the speed of scanning
            rotateDir = Vector3.up * Mathf.Sin(Time.time * scanSpeed); // Oscillate between left and right


            timer360 += Time.deltaTime;

            if (timer360 >= 5f)
            {
                transform.Rotate(Vector3.up * 180f * Time.deltaTime);
            }
        }
        else
        {
            // Allow forward/backward movement based on action input when the ball is visible
            switch (forwardAxis)
            {
                case 1:
                    dirToGo = transform.forward * m_ForwardSpeed; // Move forward
                    m_KickPower = 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -m_ForwardSpeed; // Move backward
                    break;
            }
        }


        // Lateral movement logic
        switch (rightAxis)
        {
            case 1:
                dirToGo += transform.right * m_LateralSpeed; // Move right
                break;
            case 2:
                dirToGo += transform.right * -m_LateralSpeed; // Move left
                break;
        }

        // Apply rotation and movement
        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed, ForceMode.VelocityChange);
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


    void Update()
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

}
