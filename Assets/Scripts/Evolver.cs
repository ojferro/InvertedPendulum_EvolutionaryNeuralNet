using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Evolver : MonoBehaviour
{

    private float mutationRange;
    private float mutationProbability;
    private float biasTowardsMother;
    private float percentRandomChildren;

    public void Initialize()
    {
        mutationProbability = 1f;
        mutationRange = 10f;
        biasTowardsMother = 0.6f;
        percentRandomChildren = 0.05f;
    }

    /**Validates that dimensions of mother and father NN are the same*/
    public bool validateNetworks(NeuralNetwork mother, NeuralNetwork father)
    {
        bool valid = true;
        if (mother.Topology.Length == father.Topology.Length)
        {
            for (int i = 0; i < mother.Topology.Length; i++)
            {
                if (mother.Topology[i] != father.Topology[i])
                {
                    valid = false;
                }
            }
        }
        else
        {
            valid = false;
        }

        return valid;
    }

    private bool mutate()
    {
        return Random.Range(0.0f, 1.0f) < mutationProbability ? true : false;
    }

    public AgentTrainer[] evolve(AgentTrainer[] trainers)
    {
        if (trainers.Length < 2)
        {
            throw new System.Exception("Not enough trainers passed to Evolver.evolve(). Cannot breed.");
        }
        trainers = trainers.OrderByDescending(t => t.fitness).ToArray<AgentTrainer>();
        NeuralNetwork mother = trainers[0].getNeuralNetwork();
        NeuralNetwork father = trainers[1].getNeuralNetwork();

        NeuralNetwork[] childrenNetworks = new NeuralNetwork[trainers.Length];

        if (!validateNetworks(mother, father))
        {
            throw new System.Exception("Invalid parents");
        }

        //////////////////////////////////////////////////////

        for (int i = 0; i < childrenNetworks.Length; i++)
            childrenNetworks[i] = new NeuralNetwork(mother.Topology);

        foreach (NeuralNetwork childNetwork in childrenNetworks)
        {
            for (int layer = 0; layer < childNetwork.Layers.Length; layer++)
            {
                NeuralLayer Layer = childNetwork.Layers[layer];
                for (int weightRow = 0; weightRow < Layer.Weights.GetLength(0); weightRow++)
                {
                    for (int weightCol = 0; weightCol < Layer.Weights.GetLength(1); weightCol++)
                    {
                        if (!mutate())
                        {
                            double parentWeight = Mathf.RoundToInt(Random.Range(0, 1)) >= biasTowardsMother ? mother.Layers[layer].Weights[weightRow, weightCol] : father.Layers[layer].Weights[weightRow, weightCol];
                            childNetwork.Layers[layer].Weights[weightRow, weightCol] = parentWeight;
                        }
                        else
                        {
                            childNetwork.Layers[layer].Weights[weightRow, weightCol] = Random.Range(-mutationRange, mutationRange);
                            Debug.Log("MUTATED!");
                        }
                    }
                }
            }
        }

        NeuralNetwork[] randomChildren = childrenNetworks.Take((int)(percentRandomChildren * childrenNetworks.Length)).ToArray();
        foreach (NeuralNetwork randomChild in randomChildren)
        {
            randomChild.SetRandomWeights(-mutationRange, mutationRange);
        }

        //////////////////////////////////////////////////////

        AgentTrainer[] newTrainers = new AgentTrainer[trainers.Length];
        for (int trainer = 0; trainer < newTrainers.Length; trainer++)
        {
            newTrainers[trainer] = new AgentTrainer(childrenNetworks[trainer]);
        }

        return newTrainers;
    }

}
