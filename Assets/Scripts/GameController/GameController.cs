using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [SerializeField] GameObject restartCanvas;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject dqnPrefab;
    [SerializeField] AgentManager agentManager;

    [SerializeField] ControllerClass controllerClass;

    public ControllerClass Controller => controllerClass;

    public bool isDead = false;

    // to be changed
    // PlayerController player;
    ControllerInterface player;

    private void Awake()
    {
        instance = this;
        if (controllerClass == ControllerClass.Player)
        {
            player = Instantiate(playerPrefab, transform.position, transform.rotation).GetComponent<ControllerInterface>();
            Time.timeScale = 1;
        }
        else if (controllerClass == ControllerClass.NeatAgent)
        {
            
            Time.timeScale = 2;
        }
        else
        {
            player = Instantiate(dqnPrefab, transform.position, transform.rotation).GetComponent<ControllerInterface>();
            Time.timeScale = 1;
        }
    }

    public void GameOver(CarDriver car)
    {
        car.IsAlive = false;
        if (controllerClass == ControllerClass.Player)
        {
            isDead = true;
            restartCanvas.SetActive(true);
        }
        else if (controllerClass == ControllerClass.DqnAgent)
        {
            isDead = true;
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (controllerClass != ControllerClass.NeatAgent)
        {
            FuelManager.instance.HandleUpdate();
            player.Action();
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
