using UnityEngine;
using UnityEngine.Windows;

public class Agent : MonoBehaviour
{
    [SerializeField] Rigidbody2D tireF;
    [SerializeField] Rigidbody2D tireB;

    [SerializeField] LayerMask ground;

    AgentManager manager;
    CarDriver carDriver;
    Rigidbody2D rb;

    public float[] Weigths;
    public float Score = 0;

    float input;

    public void Init(Agent mother = null)
    {
        manager = GetComponentInParent<AgentManager>();
        carDriver = GetComponentInParent<CarDriver>();
        rb = GetComponent<Rigidbody2D>();


        if (mother == null)
        {
            Weigths = new float[6] {
                Random.Range(-1,1),
                Random.Range(-1,1),
                Random.Range(-1,1),
                Random.Range(-1,1),
                Random.Range(-1,1),
                Random.Range(-1,1),
            };
        }
        else
        {
            Weigths = new float[6];
            var mutSpeed = manager.MutationSpeed;
            for (int i = 0; i < Weigths.Length; i++)
            {
                Weigths[i] = mother.Weigths[i] + Random.Range(-mutSpeed, mutSpeed);
            }
        }
    }

    private void Update()
    {
        AgentAction();
        Score = Mathf.Max(Score, transform.position.x);
    }

    private void AgentAction()
    {
        input = 0;

        input += rb.rotation * Weigths[0]; // angle
        input += rb.angularVelocity * Weigths[1]; // anglar velocity

        float isTouchingGround = (tireF.IsTouchingLayers(ground) || tireB.IsTouchingLayers(ground)) ? 1 : 0;
        input += isTouchingGround * Weigths[2]; // is touching the ground

        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, 10, ground);
        var distanceToGround = 10f;
        if (hit.collider != null)
            distanceToGround = hit.distance;
        input += distanceToGround * Weigths[3]; // distance to the ground

        input += rb.linearVelocity.magnitude * Weigths[5]; // speed
        input += Weigths[5]; // bias

        if (input > 0.5) input = 1;
        else if (input < -0.5) input = -1;
        else input = 0;

        carDriver.input = input;
    }
}
