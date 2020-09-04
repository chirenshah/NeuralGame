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

    private Vector3 parking;

    private playerArea playerArea;
    private Rigidbody2D rb;
    private float parkRadius = 0f;
    private float flag = 1f;

    public void getrandom()
    {
        int fb = Random.Range(1, 4);
        switch (fb)
        {
            case 1:
                parking = new Vector3(-9, 5f, 0f);
                break;
            case 2:
                parking = new Vector3(-9, 0.5f, 0f);
                break;
            case 3:
                parking = new Vector3(-9, -5f, 0f);
                break;
        }
    }

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
        float forwardAmount = 0f;
        // Convert the first action to forward movement
        if (vectorAction[0] == 1f)
        {
            forwardAmount = 1f;
        }

        if (vectorAction[0] == 2f)
        {
            forwardAmount = -1f;
        }

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
            AddReward(flag*(-1f/maxStep) - (flag*0.001f*distance));
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

        if (Input.GetKey(KeyCode.S))
        {
            // move forward
            forwardAction = 2f;
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
        flag = Academy.Instance.FloatProperties.GetPropertyWithDefault("flag", 1f);
    }
    /// <summary>
    /// Collect all non-Raycast observations
    /// </summary>
    public override void CollectObservations()
    {
        //Distance to collision (1 float = 1 value)
        RaycastHit2D hitw = Physics2D.Raycast(transform.position , transform.TransformDirection(Vector2.up) , Mathf.Infinity);
        if (hitw){
            AddVectorObs(hitw.distance);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.up) * hitw.distance, Color.red);
        }

        // distance to collision(1 float = 1 value)
        RaycastHit2D hits = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.down), Mathf.Infinity);
        if (hits)
        {
            AddVectorObs(hits.distance);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.down) * hits.distance, Color.red);
        }

        //distance to collision (1 float = 1 value)
        RaycastHit2D hitr = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.right), Mathf.Infinity);
        if (hitr)
        {
            AddVectorObs(hitr.distance);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.right) * hitr.distance, Color.red);
        }

        //distance to collision (1 float = 1 value)
        RaycastHit2D hitl = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.left), Mathf.Infinity);
        if (hitl)
        {
            AddVectorObs(hitl.distance);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.left) * hitl.distance, Color.red);
        }

        // Distance to the Parking (1 float = 1 value)
        AddVectorObs(Vector2.Distance(parking, transform.position));

        // Direction to Parking (1 Vector3 = 3 values)
        AddVectorObs((parking - transform.position));
        Debug.DrawRay(transform.position, (parking - transform.position) * (Vector2.Distance(parking, transform.position)), Color.green);

        // Direction Car is facing (1 Vector3 = 3 values)
        AddVectorObs(transform.forward);

        //  1+ 1 + 1 + 1 +1 + 3 + 3 = 11 total values
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
        if (Vector3.Distance(parking, transform.localPosition) < parkRadius )
        {
            AddReward(10f);
            Debug.Log("woho " + GetCumulativeReward());
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
//ml-agents-0.14.0>mlagents-learn config/trainer_config.yaml --curriculum config/curricula/parking.yaml --run-id 300803 --train