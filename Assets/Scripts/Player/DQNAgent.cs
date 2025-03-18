using System.Collections;
using System.Collections.Generic; // Added for List
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;

public class CarMLAgent : Agent, ControllerInterface
{
    [SerializeField] private Rigidbody2D tireF;
    [SerializeField] private Rigidbody2D tireB;
    [SerializeField] private LayerMask ground;
    [SerializeField] private float raycastDistance = 5f; // Distance for forward raycasts
    [SerializeField] private int numRaycasts = 5;       // Number of forward raycasts
    private Rigidbody2D rb;
    private CarDriver carDriver;
    public float Score = 0;

    private Vector3 startPosition; //for reset the car position

    private float goal = 85f;

    public override void Initialize()
    {
        carDriver = GetComponent<CarDriver>();
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        // Reset environment
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        Score = 0;
        carDriver.IsAlive = true;
        FuelManager.instance.Refuel();

        tireF.angularVelocity = 0;
        tireB.angularVelocity = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Normalized Rotation (-1 to 1)
        sensor.AddObservation(Mathf.Clamp(rb.rotation / 180f, -1f, 1f));
        // Normalized Angular Velocity (adjust the divisor as needed)
        sensor.AddObservation(Mathf.Clamp(rb.angularVelocity / 10f, -1f, 1f));

        // Ground Check
        float isTouchingGround = (tireF.IsTouchingLayers(ground) || tireB.IsTouchingLayers(ground)) ? 1 : 0;
        sensor.AddObservation(isTouchingGround);

        // Normalized Distance to Ground (clamp to a reasonable range)
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, raycastDistance, ground);
        float distanceToGround = hit.collider != null ? hit.distance : raycastDistance;
        sensor.AddObservation(Mathf.Clamp(distanceToGround / raycastDistance, 0f, 1f));

        // Normalized Velocity
        sensor.AddObservation(Mathf.Clamp(rb.linearVelocity.magnitude / 5f, 0f, 1f)); // Adjust divisor for expected max speed

        // Forward Raycasts (Terrain Awareness)
        for (int i = 0; i < numRaycasts; i++)
        {
            // Calculate raycast direction (angled forward)
            float angle = -45f + (i * (90f / (numRaycasts - 1))); // Example: -45 to 45 degrees
            Vector2 direction = Quaternion.Euler(0, 0, angle) * transform.right;

            RaycastHit2D forwardHit = Physics2D.Raycast(rb.position, direction, raycastDistance, ground);
            Debug.DrawRay(rb.position, direction * raycastDistance, Color.green); // Visualize raycasts

            // Add normalized distance to observation (or 0 if no hit)
            float forwardDistance = forwardHit.collider != null ? forwardHit.distance : raycastDistance;
            sensor.AddObservation(Mathf.Clamp(forwardDistance / raycastDistance, 0f, 1f));
        }

    }



    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        // Convert discrete action to input
        float inputValue = 0f;
        if (action == 0)
        {
            inputValue = -1f; // Left
            AddReward(-1f);
        }
        else if (action == 1)
        {
            inputValue = 0f; // Neutral
            AddReward(-0.3f);
        }
        else if (action == 2)
        {
            inputValue = 1f; // Right
            AddReward(1f);
        }

        if (!carDriver.IsAlive)
        {
            if (FuelManager.instance.CurrentFuel > 0)
            {
                Debug.Log($"Cause of Death: Head Crushed, Reward: {GetCumulativeReward()}");
                AddReward(-100f);
            }
            else
            {
                Debug.Log($"Cause of Death: Lack of Fuel, Reward: {GetCumulativeReward()}");
            }
            AddReward(5*(transform.position.x - goal));
            EndEpisode();
        }

        carDriver.input = inputValue;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteAction = actionsOut.DiscreteActions;

        int input = 1; // 1 = Neutral by default
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            input = 0; // Left (-1)
        }
        else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            input = 2; // Right (1)
        }

        discreteAction[0] = input;
    }
}