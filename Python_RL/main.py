import numpy as np
import matplotlib.pyplot as plt
import time
import json

population_path = r"C:\Users\bapti\Documents\RL\hill_climbing\Assets\Ressources\Data\population.json"
result_path = r"C:\Users\bapti\Documents\RL\hill_climbing\Assets\Ressources\Data\epoch_results.json"

population_size = 6  # number of candidate agents per generation
n_generations = 50  # number of generations to run
mutation_strength = 0.1  # noise standard deviation
elite_fraction = 0.2  # fraction of best agents to select
use_dummy_simulation = False

class Agent():
    def __init__(self, input_dim=5, hidden_dim=16, output_dim=2):
        self.weights = np.random.randn(input_dim * hidden_dim + hidden_dim * output_dim)
        self.bias = np.random.randn(hidden_dim + output_dim)
        self.input_dim = input_dim
        self.hidden_dim = hidden_dim
        self.output_dim = output_dim

class SimpleAgent():
    def __init__(self, input_dim=5, output_dim=2):
        self.weights = np.random.randn(input_dim*output_dim)
        self.bias = np.random.randn(output_dim)
        self.input_dim = input_dim
        self.output_dim = output_dim


def create_agent():
    return SimpleAgent(input_dim=5, output_dim=2)

def save_population_weights_to_json(population, filename="population.json"):
    """
    Save weights of all agents in the population to a JSON file.
    Each agent is saved as a dictionary with agent_id and parameters.
    JSON head should contain the number of agents, hidden_dim, and output_dim.
    """

    all_agents = []
    for agent_id, agent in enumerate(population):
        agent_dict = {
            'agent_id': agent_id,
            'weights': agent.weights.tolist(),
            'bias': agent.bias.tolist()
        }
        all_agents.append(agent_dict)
    
    # Save to JSON file
    with open(filename, 'w') as f:
        json.dump({
            'n_agents': len(population),
            'input_dim': population[0].input_dim,
            'output_dim': population[0].output_dim,
            'agents': all_agents
        }, f, indent=4)
    

def evaluate_dummy_population(population):
    """
    A dummy evaluation function that returns fitness values for the entire population.
    """
    fitnesses = []
    for model in population:
        # Generate deterministic but varied fitness based on weights
        fitness = np.sum(np.abs(model.weights)) + np.sum(np.abs(model.bias))
        fitnesses.append(fitness)
    return np.array(fitnesses)

def evaluate_population(population):    
    if use_dummy_simulation:
        # In dummy mode, call the dummy evaluation function
        fitnesses = evaluate_dummy_population(population)
    else:
        returned = False
        while not returned:
            with open(result_path) as f:
                results = json.load(f)
                print(results)
                if len(results) != 0:
                    print("Results returned.")
                    returned = True
            time.sleep(1)
    fitnesses = [i["DistanceTravelled"] for i in results]
    print(fitnesses)

    return np.array(fitnesses)

def initialize_population(pop_size):
    """
    Create an initial population of agents.
    """
    return [create_agent() for _ in range(pop_size)]

def mutate_noise(model, strength=mutation_strength):
    """
    Mutate an agent by adding Gaussian noise to its flat weights.
    Returns a new Agent instance with mutated weights.
    """
    new_model = create_agent()
    weights = model.weights
    bias = model.bias
    weight_noise = np.random.randn(len(weights)) * strength
    bias_noise = np.random.randn(len(bias)) * strength
    mutated_weights = weights + weight_noise
    mutated_bias = bias + bias_noise
    new_model.weights = mutated_weights
    new_model.bias = mutated_bias
    return new_model

def evolve_population_neat(population, fitnesses, elite_fraction=elite_fraction):
    """
    Evolve the population by keeping the top elite_fraction of candidates and
    reproducing them with mutation to refill the population.
    """
    # Number of elites
    n_elite = max(1, int(len(population) * elite_fraction))
    # Sort indices of population by fitness (highest fitness first)
    sorted_indices = np.argsort(fitnesses)[::-1]
    elites = [population[i] for i in sorted_indices[:n_elite]]
    new_population = elites.copy()

    # Produce offspring until we refill the population
    while len(new_population) < len(population):
        parent = elites[np.random.randint(0, n_elite)]
        child = mutate_noise(parent)
        new_population.append(child)

    return new_population, elites[0], fitnesses[sorted_indices[0]]

def evolve_population_cem(population, fitnesses, elite_fraction=elite_fraction):
    """
    Evolve the population using Cross-Entropy Method (CEM).
    """
    # Number of elites
    n_elite = int(len(population) * elite_fraction)
    # Sort indices of population by fitness (highest fitness first)
    sorted_indices = np.argsort(fitnesses)[::-1]
    elites = [population[i] for i in sorted_indices[:n_elite]]
    new_population = elites.copy()

    # Compute elite mean and std
    elite_weights = np.array([agent.weights for agent in elites])
    elite_bias = np.array([agent.bias for agent in elites])
    mean_weights = np.mean(elite_weights, axis=0)
    mean_bias = np.mean(elite_bias, axis=0)
    std_weights = np.std(elite_weights, axis=0)
    std_bias = np.std(elite_bias, axis=0)

    # Produce offspring until we refill
    while len(new_population) < len(population):
        child = create_agent()
        child.weights = np.random.normal(mean_weights, std_weights)
        child.bias = np.random.normal(mean_bias, std_bias)
        new_population.append(child)
    
    return new_population, elites[0], fitnesses[sorted_indices[0]]

def evolve_population_saes(population, fitnesses, sigma, elite_fraction=elite_fraction, n_parents=2):
    """
    Evolve the population using Simple Adaptive Evolution Strategy (SAES).
    """
    # Number of elites
    n_elite = int(len(population) * elite_fraction)
    # Sort indices of population by fitness (highest fitness first)
    sorted_indices = np.argsort(fitnesses)[::-1]
    elites = [population[i] for i in sorted_indices[:n_elite]]
    elites_sigma = [sigma[i] for i in sorted_indices[:n_elite]]
    new_population = elites.copy()
    new_sigma = elites_sigma.copy()

    """
    Explication :
    Pour le SAES, chaque individu doit avoir une centaine de poids & biais
    (comme d'habitude), mais chaque individu doit AUSSI avoir une centaine
    de "sigma", un pour chaque poids & biais, qui correspond à sa vitesse
    de mutation.
    Les sigma mutent à une vitesse constante (et sont sélectionnés avec les
    individus), et les poids & biais mutent à vitesse sigma
    On stocke donc les poids & biais dans des individus fictif : chaque
    individu a un alter ego de sigma
    """

    # Produce offspring until we refill
    while len(new_population) < len(population):
        child = create_agent()
        child_sigma = create_agent()
        
        ### 1. randomly select ρ parents among the elite ###

        parents_indices = np.random.randint(0, n_elite, size=n_parents)
        parents = [elites[i] for i in parents_indices]    
        sigma_parents = [elites_sigma[i] for i in parents_indices]    

        ### 2. (x,σ)← recombination of selected parents (if ρ>1) ###

        # For now, we update the weights independently
        # Maybe we should select the same parents for all weights & bias
        # That share a same neuron ?

        for id_weight in range(len(child.weights)):
            id_parent = np.random.randint(0, n_parents)
            child.weights[id_weight] = parents[id_parent].weights[id_weight]
            child_sigma.weights[id_weight] = sigma_parents[id_parent].weights[id_weight]
        for id_bias in range(len(child.bias)):
            id_parent = np.random.randint(0, n_parents)
            child.bias[id_bias] = parents[id_parent].bias[id_bias]
            child_sigma.bias[id_bias] = sigma_parents[id_parent].bias[id_bias]

        ### 3. mutation of σ (individual strategy) : σ←σ eτN(0,1) ###
        child_sigma = mutate_noise(child_sigma, strength=mutation_strength)

        ### 4. mutation of x (objective param) : x←x+σ N(0,1) ###
        child.weights +=  np.random.randn(len(child.weights)) * child_sigma.weights
        child.bias += np.random.randn(len(child.bias)) * child_sigma.bias
        

        new_population.append(child)
        new_sigma.append(child)
    
    return new_population, elites[0], fitnesses[sorted_indices[0]], new_sigma



def train(evolve_function=evolve_population_cem):
    population = initialize_population(population_size)
    save_population_weights_to_json(population, population_path)
    if evolve_function == evolve_population_saes:
        sigma = initialize_population(population_size)
    best_fitness_history = []
    avg_fitness_history = []
    best_agent = None
    best_fitness_so_far = -np.inf
    evolve_population = evolve_function


    for gen in range(n_generations):
        # Evaluate the entire population at once
        fitnesses = evaluate_population(population)
        
        # Print individual fitness values
        for i, fitness in enumerate(fitnesses):
            pass
            #print(f"Generation {gen+1}, Agent {i+1}: Fitness = {fitness:.3f}")
        
        # Compute statistics
        avg_fitness = np.mean(fitnesses)
        best_idx = np.argmax(fitnesses)
        gen_best_fitness = fitnesses[best_idx]
        print(f"Generation {gen+1}: Avg Fitness = {avg_fitness:.3f}, Best Fitness = {gen_best_fitness:.3f}")
        
        best_fitness_history.append(gen_best_fitness)
        avg_fitness_history.append(avg_fitness)
        
        # Evolve population
        if evolve_population == evolve_population_saes:
            population, best_candidate, current_best_fitness, sigma = evolve_population(population, fitnesses, sigma)
        else: # evolve_population_noise & evolve_population_cem
            population, best_candidate, current_best_fitness = evolve_population(population, fitnesses)
        
        # Update global best if needed
        if current_best_fitness > best_fitness_so_far:
            best_fitness_so_far = current_best_fitness
            best_agent = best_candidate
        
        
        save_population_weights_to_json(population, population_path)
        # Clear the contents of epoch_results.json
        with open(result_path, 'w') as f:
            json.dump([], f)

    # Save the best agent
    if best_agent is not None:
        # Create a DataFrame with single row for best agent
        best_weights = best_agent.weights.tolist()
        best_bias = best_agent.bias.tolist()
        #Save to json
        with open("best_agent.json", 'w') as f:
            json.dump({
                'input_dim': best_agent.input_dim,
                'output_dim': best_agent.output_dim,
                'weights': best_weights,
                'bias': best_bias
            }, f, indent=4)
        print("Best agent saved.")
    else:
        print("No best agent found.")

    # Plot the results
    plt.figure(figsize=(10, 5))
    plt.plot(range(1, n_generations+1), avg_fitness_history, label="Average Fitness")
    plt.plot(range(1, n_generations+1), best_fitness_history, label="Best Fitness")
    plt.xlabel("Generation")
    plt.ylabel("Fitness")
    plt.title("Evolutionary Algorithm Performance")
    plt.legend()
    plt.show()

train(evolve_population_saes)