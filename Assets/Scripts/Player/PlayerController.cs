using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CarDriver carDriver;

    private void Awake()
    {
        carDriver = GetComponent<CarDriver>();
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        carDriver.input = Input.GetAxisRaw("Horizontal");
    }
}
