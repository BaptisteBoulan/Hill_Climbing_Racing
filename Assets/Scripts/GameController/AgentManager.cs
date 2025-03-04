using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] float mutationSpeed;
    [SerializeField] int numAgent;

    [SerializeField] float generationDuration;
    [SerializeField] float timeSpeed;

    Agent[] agents;

    public float MutationSpeed => mutationSpeed;
    public float currentTime = 0;

    public void InitAgents(Agent[] mothers = null)
    {
        agents = new Agent[numAgent];

        for (int i = 0; i < agents.Length; i++)
        {
            Agent mother = null;
            if (mothers != null && mothers.Length > 0)
                mother = mothers[i * mothers.Length / agents.Length];
            var agent = Instantiate(prefab, transform);
            var car = agent.GetComponent<CarDriver>();
            car.IsPlayer = false;
            car.IsAlive = true;
            agents[i] = agent.GetComponent<Agent>();
            agents[i].Init(mother);
        }
    }

    public void HandleUpdate()
    {
        currentTime += Time.deltaTime;
        if (currentTime > generationDuration)
        {
            currentTime = 0;
            NewGeneration();
        }
    }

    void NewGeneration()
    {
        Time.timeScale = timeSpeed;
        int n = 3;

        List<(Agent agent, float score)> agentScores = new List<(Agent, float)>();


        for (int i = 0; i < agents.Length; i++)
        {
            float score = agents[i].Score;
            agentScores.Add((agents[i].GetComponent<Agent>(), score));
            Destroy(agents[i].gameObject);
        }

        agentScores.Sort((x, y) => y.score.CompareTo(x.score));

        List<Agent> bestAgents = new List<Agent>();
        for (int i = 0; i < Mathf.Min(n, agentScores.Count); i++)
        {
            bestAgents.Add(agentScores[i].agent);
        }

        InitAgents(bestAgents.ToArray());
    }

}
