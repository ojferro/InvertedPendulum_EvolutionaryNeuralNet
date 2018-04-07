//TODO: Serialise NN
//TODO: That's All Folks

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;


public class WorldController : MonoBehaviour
{
    public bool training;
    public int totalEpochs = 1000; // set to -1 for unlimited # of epochs
    private int numEpochs = 0;
    public uint[] topology = { 6, 50, 50, 50, 50, 2 };
    public const int numClones = 25;

    private AgentTrainer[] potentialParents = new AgentTrainer[2];
    List<AgentTrainer> agentGraveyard = new List<AgentTrainer>();

    public GameObject agentPrefab;    //Assign a prefab in the editor.
    private GameObject[] agents = new GameObject[numClones];
    private AgentTrainer[] newTrainers = new AgentTrainer[numClones];

    private Evolver evolver = new Evolver();

    private void instantiateAgents()
    {
        for (int i = 0; i < numClones; i++)
        {
            agents[i] = Instantiate(agentPrefab);
            MeshRenderer pendulumMesh = agents[i].GetComponent<MeshRenderer>();
            if (pendulumMesh != null)
            {
                pendulumMesh.material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);//colours[i];//Random.Range(0, 1) * 255, Random.Range(0, 1) * 255, Random.Range(0, 1) * 255, 1);
            }

            AgentTrainer trainer = agents[i].GetComponent<AgentTrainer>();
            trainer.Initialize(newTrainers[i].getNeuralNetwork(), Random.Range(-15, 15));
            //Debug.Log("Fitness: " + trainer.fitness);
        }
    }

    void Start()
    {
        //Note: Different instances are set to not collide given Edit>ProjectSettings>Physics>LayerCollisionMatrix settings
        Debug.Log("Starting! Godspeed");
        Time.timeScale = 5f;

        //TODO: Make random neural networks or read in NNs from file
        NeuralNetwork[] initNetworks = new NeuralNetwork[numClones];
        for (int i = 0; i < initNetworks.Length; i++)
        {
            initNetworks[i] = new NeuralNetwork(topology);
            initNetworks[i].SetRandomWeights(-10.0, 10.0);
        }

        for (int i = 0; i < numClones; i++)
        {
            newTrainers[i] = new AgentTrainer(initNetworks[i]);
        }

        instantiateAgents();
    }


    void FixedUpdate()
    {
        if (training)
        {
            if (totalEpochs == -1 || numEpochs <= totalEpochs)
            {
                GameObject[] deadAgents = agents.Where(a => a != null && !a.GetComponent<AgentTrainer>().isAlive()).ToArray<GameObject>();

                foreach (GameObject deadAgent in deadAgents)
                {
                    agentGraveyard.Add(deadAgent.GetComponent<AgentTrainer>());
                    Destroy(deadAgent);
                }

                if (agentGraveyard.Count == numClones)//All are dead
                {
                    Debug.Log("Disactivating");

                    //Evolve all agents
                    newTrainers = evolver.evolve(agentGraveyard.ToArray());
                    agentGraveyard.Clear();

                    //Instantiate new agents
                    instantiateAgents();

                    numEpochs++;
                    Debug.Log(numEpochs);
                }
            }
            else
            {
                Debug.Log("Training done. Saving Neural Network...");
                string path = "./";

                AgentTrainer[] trainers = agents.Select(n => n.GetComponent<AgentTrainer>()).ToArray();
                trainers.OrderByDescending(t => t.fitness);
                NeuralNetwork motherNetwork = trainers[0].getNeuralNetwork();
                NeuralNetwork fatherNetwork = trainers[1].getNeuralNetwork();

                File.WriteAllText(path + "motherNetwork.txt", motherNetwork.ToString());
                File.WriteAllText(path + "fatherNetwork.txt", fatherNetwork.ToString());
                Debug.Log("Saved.");
            }
        }
        else //if not training
        {
            //TODO play
        }
    }
}
