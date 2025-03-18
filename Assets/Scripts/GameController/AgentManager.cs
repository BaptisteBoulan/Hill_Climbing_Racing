using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor.Animations;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField] GameObject agentPrefab;
    [SerializeField] float mutationSpeed;

    [SerializeField] float generationDuration;

    [SerializeField] string jsonFilePath = "./population.json";
    [SerializeField] string resultsFilePath = "./epoch_results.json";

    public float MutationSpeed => mutationSpeed;
    public float currentTime;

    private void Start()
    {
        currentTime = generationDuration;
    }

    public void HandleUpdate()
    {
        currentTime += Time.deltaTime;
        if (currentTime > generationDuration)
        {
            currentTime = 0;
            StartCoroutine(NewGeneration());
        }
    }

    IEnumerator NewGeneration()
    {
        List<AgentResult> epochResults = new List<AgentResult>();

        int i = 0;
        foreach (Transform child in transform)
        {
            NeatAgent agent = child.GetComponent<NeatAgent>();
            CarDriver driver = child.GetComponent<CarDriver>();

            if (agent != null && driver != null)
            {
                epochResults.Add(new AgentResult
                {
                    agent_id = i,
                    DistanceTravelled = agent.Score,
                    IsAlive = driver.IsAlive
                });
                i++;
            }

            Destroy(child.gameObject);
        }

        Debug.Log("on attend");

        if (File.Exists(resultsFilePath))
        {
            // Save epoch results to JSON
            string epochJson = JsonConvert.SerializeObject(epochResults, Formatting.Indented);
            File.WriteAllText(resultsFilePath, epochJson);
        }
        else
        {
            Debug.Log("Fichier JSON non trouvé !");
        }

        yield return new WaitUntil(() => File.Exists(jsonFilePath));

        // Lire le fichier JSON
        string jsonData = File.ReadAllText(jsonFilePath);
        Debug.Log("JSON lu !");
        yield return new WaitUntil(() => !string.IsNullOrWhiteSpace(File.ReadAllText(jsonFilePath)));

        PopulationData population = JsonConvert.DeserializeObject<PopulationData>(jsonData);

        yield return new WaitForSeconds(1f);
        Debug.Log("go go go");
        yield return new WaitUntil(() =>
        {
            jsonData = File.ReadAllText(jsonFilePath);
            population = JsonConvert.DeserializeObject<PopulationData>(jsonData);
            return population != null && population.agents.Count > 0;
        });

        // Créer de nouveaux agents et initialiser avec les poids/biais du JSON
        List<NeatAgent> newAgents = new List<NeatAgent>();


        foreach (var agentData in population.agents)
        {
            GameObject newAgentObject = Instantiate(agentPrefab, Vector2.zero, Quaternion.identity, transform);
            NeatAgent newAgent = newAgentObject.GetComponent<NeatAgent>();

            newAgent.Init(agentData.weights, agentData.bias);
            newAgents.Add(newAgent);
        }

        // Supprimer le contenu du fichier JSON
        File.WriteAllText(jsonFilePath, string.Empty);

        Debug.Log("Nouvelle génération créée avec succès et JSON vidé !");
    }

}

[System.Serializable]
public class PopulationData
{
    public int n_agents;
    public int input_dim;
    public int output_dim;
    public List<AgentData> agents;
}

[System.Serializable]
public class AgentData
{
    public int agent_id;
    public List<float> weights;
    public List<float> bias;
}

[System.Serializable]
public class AgentResult
{
    public int agent_id;
    public float DistanceTravelled;
    public bool IsAlive;
}

