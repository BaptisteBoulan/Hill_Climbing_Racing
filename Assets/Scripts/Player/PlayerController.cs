using UnityEngine;

public class PlayerController : MonoBehaviour, ControllerInterface
{
    CarDriver carDriver;

    private void Awake()
    {
        carDriver = GetComponent<CarDriver>();
    }

    // Update is called once per frame
    public void Action()
    {
        carDriver.input = Input.GetAxisRaw("Horizontal");
    }
}
