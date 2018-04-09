using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentTrainer : MonoBehaviour
{

    public bool alive;// = true;
    public float fitness;

    private NeuralNetwork neuralNetwork;
    private int numInputs;
    //public List<double> neuralNetworkInputs;
    public double[] neuralNetworkOutputs;

    //Partuculars of implementation of project//
    //(things that are not universal)//
    public Rigidbody cartRB;
    public GameObject pendulum;

    public float accelMultiplier;
    public float maxSpeed;
    public float targetAngle;
    public float targetPositionX;
    public float survivalAngleRange;
    public float rewardAngleRange;
    private float rewardPositionRange;
    private float punishmentForDeath;

    private double prevAngle;
    public double currentAngle;
    private double prevPositionX;
    public double currentPositionX;
    private double prevVelocity;
    private double currentVelocity;
    private double angularVelocity;
    //Partuculars of implementation of project//

    public double rew;

    public AgentTrainer(NeuralNetwork neuralNetwork)
    {
        this.neuralNetwork = neuralNetwork;
        alive = true;
        prevAngle = 0f;
        prevVelocity = 0f;
        currentAngle = 0f;
    }

    public void Initialize(NeuralNetwork neuralNetwork, float startingAngle)
    {
        this.neuralNetwork = neuralNetwork;
        WorldController w = new WorldController();
        numInputs = (int)w.topology[0];

        alive = true;
        fitness = 0f;
        prevAngle = 0f;
        prevVelocity = 0f;
        currentAngle = 0f;
        prevPositionX = 0f;
        currentPositionX = 0f;
        targetPositionX = 0.0f;
        accelMultiplier = 200.0f;
        maxSpeed = 100.0f;
        targetAngle = 0f;
        survivalAngleRange = 90f;
        rewardAngleRange = 20f;
        rewardPositionRange = 10f;
        punishmentForDeath = 100f;
        pendulum.transform.eulerAngles = new Vector3(0f, 0f, startingAngle);
        currentVelocity = 0f;
        angularVelocity = 0f;
    }

    public bool isAlive()
    {
        return alive;
    }

    public NeuralNetwork getNeuralNetwork()
    {
        return neuralNetwork;
    }

    public void setNeuralNetwork(NeuralNetwork newNeuralNetwork)
    {
        neuralNetwork = newNeuralNetwork;
    }

    private double[] setInputs()
    {
        //prevAngleNorm = currentAngleNorm;
        //currentAngleNorm = (pendulum.transform.eulerAngles.z) > 180.0f ? (360 - pendulum.transform.eulerAngles.z) / 180.0f : (pendulum.transform.eulerAngles.z) / 180.0f;

        prevAngle = currentAngle;
        currentAngle = (pendulum.transform.eulerAngles.z);
        prevPositionX = currentPositionX;
        currentPositionX = cartRB.position.x;
        prevVelocity = currentVelocity;
        currentVelocity = cartRB.velocity.x;
        angularVelocity = pendulum.GetComponent<Rigidbody>().angularVelocity.z;
        float centreOfMassX = pendulum.GetComponent<Rigidbody>().centerOfMass.x;
        float COMtoCartDelta = cartRB.position.x-centreOfMassX;

        double[] neuralNetworkInputs = new double[numInputs];
        neuralNetworkInputs[0] = prevAngle;
        neuralNetworkInputs[1] = currentAngle;
        neuralNetworkInputs[2] = prevVelocity;
        neuralNetworkInputs[3] = currentVelocity;
        neuralNetworkInputs[4] = angularVelocity;
        neuralNetworkInputs[5] = currentPositionX;
        //neuralNetworkInputs[6] = COMtoCartDelta;

        return neuralNetworkInputs;
    }

    private void performActions(double[] actions)
    {
        Vector3 movement = new Vector3((float)actions[0] - 0.5f, 0.0f, 0.0f);
        cartRB.AddForce(movement * accelMultiplier);

        if (cartRB.velocity.magnitude > maxSpeed)
        {
            cartRB.velocity = cartRB.velocity.normalized * maxSpeed;
        }

    }

    public float calculateReward(float angleDelta, float positionDelta)
    {
        //float reward;// = (rewardAngleRange - Mathf.Abs(angleDelta)) / rewardAngleRange; //value 0to1 (0 = 20+ degrees from target, 1 = 0 degrees from target)
        //reward = angleDelta > rewardAngleRange ? 0 : (rewardAngleRange - Mathf.Abs(angleDelta)) / rewardAngleRange; //angleReward > 0 ? angleReward : 0;


        float angleReward = (pendulum.transform.eulerAngles.z) > 180.0f ? (360 - pendulum.transform.eulerAngles.z) / 180.0f : (pendulum.transform.eulerAngles.z) / 180.0f;//bottom 180/180 = 1, top 0/180 = 0
        angleReward -= 1.0f;
        angleReward = Mathf.Abs(angleReward);

        rew = angleReward;
        float positionReward = (rewardPositionRange - Mathf.Abs(positionDelta)) / rewardPositionRange;
        //positionReward = positionReward > 0 ? positionReward : 0;

        return angleReward + positionReward;
    }

    private void reward()
    {
        fitness += calculateReward(Mathf.Abs((float)currentAngle - targetAngle), cartRB.position.x - targetPositionX);
    }

    private void checkState()
    {
        float normalisedAngle = (pendulum.transform.eulerAngles.z) > 180.0f ? (360 - pendulum.transform.eulerAngles.z) / 180.0f : (pendulum.transform.eulerAngles.z) / 180.0f;//bottom 180/180=1, top 0/180=0
        //if (Mathf.Abs((float)targetAngle - (float)currentAngle) > survivalAngleRange)
        if (normalisedAngle > 0.5)
        {
            fitness -= punishmentForDeath;
            alive = false;
        }
    }

    void FixedUpdate()
    {
        double[] neuralNetworkInputs = setInputs();
        neuralNetworkOutputs = neuralNetwork.ProcessInputs(neuralNetworkInputs);
        performActions(neuralNetworkOutputs);
        reward();
        checkState();
    }
}
