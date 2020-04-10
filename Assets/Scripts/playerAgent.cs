using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using TMPro;

public class playerAgent : Agent
{
    [Tooltip("How fast the agent moves forward")]
    public float accelerationPower = 5f;

    [Tooltip("How fast the agent turns")]
    public float steeringPower = 5f;

    float steeringAmount, speed, direction;

    private Vector3 parking = new Vector3(-9f, -3.5f, 0f);

    private playerArea playerArea;
    private Rigidbody2D rb;
    private float parkRadius = 0f;


    /// <summary>
    /// Initial setup, called when the agent is enabled
    /// </summary>
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        playerArea = GetComponentInParent<playerArea>();
        rb = GetComponent<Rigidbody2D>();
    }
    /// <summary>
    /// Perform actions based on a vector of numbers
    /// </summary>
    /// <param name="vectorAction">The list of actions to take</param>
    public override void AgentAction(float[] vectorAction)
    {
        // Convert the first action to forward movement
        float forwardAmount = vectorAction[0];

        // Convert the second action to turning left or right
        float turnAmount = 0f;
        if (vectorAction[1] == 1f)
        {
            turnAmount = 1f;
        }
        else if (vectorAction[1] == 2f)
        {
            turnAmount = -1f;
        }

        // Apply movement
        steeringAmount = turnAmount;
        speed = forwardAmount * accelerationPower;
        direction = Mathf.Sign(Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up)));
        rb.rotation += steeringAmount * steeringPower * rb.velocity.magnitude * direction;

        rb.AddRelativeForce(Vector2.up * speed);

        rb.AddRelativeForce(-Vector2.right * rb.velocity.magnitude * steeringAmount / 2);

        // Apply a tiny negative reward every step to encourage action
        if (maxStep > 0)
        {
            float distance = Vector3.Distance(parking, transform.position);
            AddReward((-1f/maxStep) - 0.0001f*distance);
            //Debug.Log();
        }
    }
    /// <summary>
    /// Read inputs from the keyboard and convert them to a list of actions.
    /// This is called only when the player wants to control the agent and has set
    /// Behavior Type to "Heuristic Only" in the Behavior Parameters inspector.
    /// </summary>
    /// <returns>A vectorAction array of floats that will be passed into <see cref="AgentAction(float[])"/></returns>
    public override float[] Heuristic()
    {
        float forwardAction = 0f;
        float turnAction = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            // move forward
            forwardAction = 1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            // turn left
            turnAction = 1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // turn right
            turnAction = 2f;
        }

        // Put the actions into an array and return
        return new float[] { forwardAction, turnAction };
    }
    /// <summary>
    /// Reset the agent and area
    /// </summary>
    public override void AgentReset()
    {
        playerArea.ResetArea();
        parkRadius = Academy.Instance.FloatProperties.GetPropertyWithDefault("park_radius", 0f);
    }
    /// <summary>
    /// Collect all non-Raycast observations
    /// </summary>
    public override void CollectObservations()
    {
        // Distance to the Parking (1 float = 1 value)
        AddVectorObs(Vector3.Distance(parking, transform.position));

        // Direction to Parking (1 Vector3 = 3 values)
        AddVectorObs((parking - transform.position).normalized);

        // Direction Car is facing (1 Vector3 = 3 values)
        AddVectorObs(transform.forward);

        //  1 + 3 + 3 = 7 total values
    }
    private void FixedUpdate()
    {
      
        // Request a decision every 5 steps. RequestDecision() automatically calls RequestAction(),
        // but for the steps in between, we need to call it explicitly to take action using the results
        // of the previous decision
        if (GetStepCount() % 5 == 0)
        {
            RequestDecision();
        }
        else
        {
            RequestAction();
        }
        //Debug.Log(Vector3.Distance(parking, transform.localPosition));
        //Debug.Log(parkRadius+1f);
        if (Vector3.Distance(parking, transform.localPosition) < parkRadius+1f)
        {
            AddReward(4f);
            Debug.Log("woho" + GetCumulativeReward());
            Done();
        }
    }
    /// <summary>
    /// When the agent collides with something, take action
    /// </summary>
    /// <param name="collision">The collision info</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Brick") || collision.transform.CompareTag("Obstacle"))
        {
            AddReward(-1f);
            Done();
        }
    }
}
