using System.Collections.Generic;
using UnityEngine;

public class NeatAgent : MonoBehaviour, ControllerInterface
{
    [SerializeField] Rigidbody2D tireF;
    [SerializeField] Rigidbody2D tireB;
    [SerializeField] LayerMask ground;

    AgentManager manager;
    CarDriver carDriver;
    Rigidbody2D rb;

    public float[] Weights;
    public float Score = 0;

    float input;
    float positiveInput;
    float negativeInput;

    public void Init(List<float> weights = null, List<float> bias = null)
    {
        manager = GetComponentInParent<AgentManager>();
        carDriver = GetComponentInParent<CarDriver>();
        rb = GetComponent<Rigidbody2D>();

        int weightSize = 10;
        int biasSize = 2;

        if (weights != null && bias != null)
        {
            Weights = weights.ToArray();
        }
        else
        {
            Weights = new float[weightSize];
            for (int i = 0; i < Weights.Length; i++)
            {
                Weights[i] = Random.Range(-1f, 1f);
            }
        }
    }

    private void Update()
    {
        Action();
        Score = Mathf.Max(Score, transform.position.x);
    }

    private void Action()
    {
        positiveInput = 0;
        negativeInput = 0;

        float[] inputs = new float[5];
        inputs[0] = rb.rotation;
        inputs[1] = rb.angularVelocity;
        inputs[2] = (tireF.IsTouchingLayers(ground) || tireB.IsTouchingLayers(ground)) ? 1 : 0;

        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, 10, ground);
        inputs[3] = hit.collider != null ? hit.distance : 10f;
        inputs[4] = rb.linearVelocity.magnitude;

        for (int i = 0; i < 5; i++)
        {
            positiveInput += inputs[i] * Weights[i];
            negativeInput += inputs[i] * Weights[i + 5];
        }

        positiveInput += Weights[5];  // Biais positif
        negativeInput += Weights[6];  // Biais négatif

        if (Mathf.Max(positiveInput, negativeInput) < 1)
            input = 0;
        else
            input = (positiveInput > negativeInput) ? 1 : -1;

        carDriver.input = input;
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 500, 30), $"Positive Input: {positiveInput:F2}", style);
        GUI.Label(new Rect(10, 40, 500, 30), $"Negative Input: {negativeInput:F2}", style);
        GUI.Label(new Rect(10, 70, 500, 30), $"Final Input: {input}", style);
    }
}
