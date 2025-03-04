using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [SerializeField] GameObject restartCanvas;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] AgentManager agentManager;

    [SerializeField] bool isPlayer;

    public bool IsPlayer => isPlayer;

    public bool isDead = false;

    PlayerController player;

    private void Awake()
    {
        instance = this;
        if (isPlayer)
        {
            player = Instantiate(playerPrefab, transform.position, transform.rotation).GetComponent<PlayerController>();
            Time.timeScale = 1;
        }
        else
        {
            agentManager.InitAgents();
            Time.timeScale = 5;
        }
    }

    public void GameOver(CarDriver car)
    {
        car.IsAlive = false;
        if (isPlayer)
        {
            isDead = true;
            restartCanvas.SetActive(true);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (isPlayer)
        {
            FuelManager.instance.HandleUpdate();
            player.HandleUpdate();
        }
        else
        {
            agentManager.HandleUpdate();
        }
        Quit();
    }

    private void Quit()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
