using UnityEngine;

public class DriverHead : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            GameController.instance.GameOver(GetComponentInParent<CarDriver>());
    }
}
