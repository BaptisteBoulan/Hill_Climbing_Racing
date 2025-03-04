using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FuelManager : MonoBehaviour
{
    public static FuelManager instance;

    [SerializeField] Image fuelImage;

    [SerializeField] float maxFuel = 100;
    [SerializeField, Range (0.1f,5)] float fluelConsumtion;
    [SerializeField] Gradient gradient;

    float currentFuel;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        currentFuel = maxFuel;
        UpdateUI();
    }

    public void HandleUpdate()
    {
        currentFuel -= fluelConsumtion * Time.deltaTime;
        UpdateUI();
        CheckForDead();
    }

    void UpdateUI()
    {
        fuelImage.fillAmount = currentFuel/maxFuel;
        fuelImage.color = gradient.Evaluate(currentFuel / maxFuel);
    }

    void CheckForDead()
    {
        if (currentFuel <= 0)
        {
            GameController.instance.GameOver(FindAnyObjectByType<CarDriver>());
        }
    }

    public void Refuel()
    {
        currentFuel = maxFuel;
        UpdateUI();
    }
}
