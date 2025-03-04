using System;
using UnityEngine;

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
    public bool IsPlayer = true;

    Agent agent;
    public bool IsAlive;
    private void Start()
    {
        IsAlive = true;
        agent = GetComponent<Agent>();
    }

    // Update is called once per frame
    void Update()
    {
        Action();
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
