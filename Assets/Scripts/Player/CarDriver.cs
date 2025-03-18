using UnityEngine;

public enum ControllerClass
{
    Player,
    NeatAgent,
    DqnAgent
}
public class CarDriver : MonoBehaviour
{
    [Header("Tires")]
    [SerializeField] Rigidbody2D tireF;
    [SerializeField] Rigidbody2D tireB;
    [SerializeField] float speed = 150;

    [Header("Car")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float rotationSpeed = 300;
    [SerializeField] LayerMask ground;

    public float input { get; set; }
    public ControllerClass controllerClass = ControllerClass.Player;

    // Agent agent;
    ControllerInterface controller;

    public bool IsAlive;
    private void Start()
    {
        IsAlive = true;
        if (controllerClass == ControllerClass.Player)
        {
            controller = GetComponent<PlayerController>();
        }
        else if (controllerClass == ControllerClass.NeatAgent)
        {
            controller = GetComponent<NeatAgent>();
        }
        else
        {
            controller = GetComponent<CarMLAgent>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsAlive)
        {
            controller.Action();
            Action();
        }
    }


    private void Action()
    {
        tireF.AddTorque(-input * speed * Time.deltaTime);
        tireB.AddTorque(-input * speed * Time.deltaTime);
        rb.AddTorque(input * rotationSpeed * Time.deltaTime);

        tireF.angularVelocity = Mathf.Clamp(tireF.angularVelocity, -2000, 2000);
        tireB.angularVelocity = Mathf.Clamp(tireB.angularVelocity, -2000, 2000);
    }

    private void PlayerAction()
    {
        input = Input.GetAxisRaw("Horizontal");
    }
}
