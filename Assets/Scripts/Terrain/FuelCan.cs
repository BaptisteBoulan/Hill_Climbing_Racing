using UnityEngine;

public class FuelCan : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FuelManager.instance.Refuel();
            Destroy(gameObject);
        }
    }

}
