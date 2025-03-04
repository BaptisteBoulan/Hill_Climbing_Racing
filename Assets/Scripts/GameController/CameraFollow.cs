using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] public Vector3 offset;

    public Transform player;

    private void Start()
    {
        if (GameController.instance.IsPlayer)
            player = FindAnyObjectByType<CarDriver>().transform;
        else
            transform.position += offset;
    }
    void Update()
    {
        if (player != null)
            transform.position = player.position + offset;
        else
        {
            Vector3 inputs = new Vector3(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical"),
                    0
                );
            transform.position += 3 * Time.deltaTime * 5 * inputs;
        }
    }
}

