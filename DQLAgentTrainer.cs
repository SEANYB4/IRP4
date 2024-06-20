using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DQLAgentTrainer : MonoBehaviour
{
    NeuralNetwork qNetwork;
    NeuralNetwork targetNetwork;

    public int inputSize = 3; // Define according to your specific environment
    private int outputSize = 5; // Define according to your specific environment
    float discountFactor = 0.9f; // Emphasize or de-emphasize future rewards
    float explorationRate = 2.0f;
    float explorationDecay = 0.995f;
    float minExplorationRate = 0.01f;
    List<float[]> stateMemory = new List<float[]>(); // S
    List<int> actionMemory = new List<int>(); // A
    List<float> rewardMemory = new List<float>(); // R
    List<float[]> nextStateMemory = new List<float[]>(); // S1


    public int batchSize = 32; // The size of the batch to train on each frame



    // To reference other scripts

    public GameObject enemy;
    public GameObject trainer;


    // Reference to state machine
    private TrainerStateMachine trainerStateMachine;


    // Idling variables

    private const float idlingThreshold = 4.0f;

    
    private float resetHeight = -10f; // Height at which the enemy will reset




    // Variables for weights training

     private String filePathForWeights = "Assets/Weights/network_weights.json";
    private bool shouldLoadWeights = true;
    private int episodeCount = 0;
    private int loadWeightThreshold = 50;



    // Start is called before the first frame update
    void Start()
    {
        
        qNetwork = new NeuralNetwork(new int[] { inputSize, 64, 64, outputSize });
        targetNetwork = new NeuralNetwork(new int[] { inputSize, 64, 64, outputSize });

        if (shouldLoadWeights)
        {
            qNetwork.LoadWeights(filePathForWeights);
            targetNetwork.LoadWeights(filePathForWeights);
        }

        enemy = GameObject.FindGameObjectWithTag("Enemy");
        trainer = GameObject.FindGameObjectWithTag("Trainer");

        trainerStateMachine = new TrainerStateMachine();

        trainerStateMachine.ChangeState(new TrainerChaseState(gameObject, GameObject.FindGameObjectWithTag("Enemy").transform, trainerStateMachine)); // Start in the chase state
    }

    // Update is called once per frame
    void Update()
    {

        // Calling these in update because new enemy objects are created after death
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        trainer = GameObject.FindGameObjectWithTag("Trainer");

        // Check if the enemy has fallen below the reset height
        if (transform.position.y < resetHeight)
        {
            // Reset position to (0, 0, 0) or any other specified respawn point
            transform.position = Vector3.zero;
            
            // Optionally, reset velocity if using a Rigidbody
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
            }
        }


        // Method to be called every frame (or decision step)
        float[] currentState = GetStateFromEnvironment();
        int action = ChooseAction(currentState);
        PerformAction(action);
        float reward = GetRewardFromEnvironment();
        float[] nextState = GetStateFromEnvironment();

        Remember(currentState, action, reward, nextState);
        Replay(); // Train on a mini-batch every frame

        trainerStateMachine.Update();


        if (ShouldLoadTargetWeights())
        {
            qNetwork.LoadWeights(filePathForWeights);
            targetNetwork.LoadWeights(filePathForWeights);
        }
        
    }

    int ChooseAction(float[] state)
    {

        if (UnityEngine.Random.value < explorationRate)
        {
            return UnityEngine.Random.Range(0, outputSize); // Random action    
        }
        else 
        {
            float[] qValues = qNetwork.FeedForward(state);
            
            
            //for (int i = 0; i < qValues.Length; i++)
            //{
              //  Debug.Log(qValues[i]);
            //}
            
            
            return Array.IndexOf(qValues, qValues.Max()); 
        }
        
    }

    void Remember(float[] state, int action, float reward, float[] nextState)
    {
        stateMemory.Add(state);
        actionMemory.Add(action);
        rewardMemory.Add(reward);
        nextStateMemory.Add(nextState);
    }


    void Replay()
    {
        
        if (stateMemory.Count > batchSize)
        {
            List<int> sampleIndices = Enumerable.Range(0, stateMemory.Count).OrderBy(x => UnityEngine.Random.value).Take(batchSize).ToList();


            foreach (int index in sampleIndices)
            {
                Train(stateMemory[index], actionMemory[index], rewardMemory[index], nextStateMemory[index]);
            }
        }

        // Optionally clear the memory if it grows too large
        if (stateMemory.Count > 50000) // Arbitrary number, adjust based on your requirements
        {
            Debug.Log("CLEARING STATE MEMORY");
            stateMemory.Clear();
            actionMemory.Clear();
            rewardMemory.Clear();
            nextStateMemory.Clear();
        }


        // Update exploration rate
        if (explorationRate > minExplorationRate)
        {
            explorationRate *= explorationDecay;
        }
    }


    void Train(float[] inputs, int action, float reward, float[] nextInputs)
    {
        
        float[] currentQs = qNetwork.FeedForward(inputs);
        float[] nextQs = targetNetwork.FeedForward(nextInputs);
        float maxNextQ = nextQs.Max();

        float[] newQs = (float[])currentQs.Clone();
        newQs[action] = reward + discountFactor * maxNextQ;

        qNetwork.BackPropagate(newQs);
    }


    float[] GetStateFromEnvironment()
    {
        // Check if the player or enemy is not assigned
        if (enemy == null || trainer == null)
        {
            Debug.LogError("Trainer or Enemy reference not set in DQL Agent Trainer.");
            return new float[0]; // Return an empty array or handle this case as needed
        }

        float enemyHealth = enemy.GetComponent<Enemy>().health;
        float trainerHealth = trainer.GetComponent<Trainer>().health;
        float distanceToEnemy = Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position);
        
        return new float[] { enemyHealth, trainerHealth, distanceToEnemy };

    }


    void PerformAction(int action)
    {
        
        switch (action)
        {

            case 0:
                trainerStateMachine.ChangeState(new TrainerHideInCoverState(gameObject, GameObject.FindGameObjectWithTag("Enemy").transform, trainerStateMachine));
                break;

            case 1:
                trainerStateMachine.ChangeState(new TrainerAttackState(gameObject, GameObject.FindGameObjectWithTag("Enemy").transform, trainerStateMachine));
                break;

            case 2:
                trainerStateMachine.ChangeState(new TrainerRetreatState(gameObject, GameObject.FindGameObjectWithTag("Enemy").transform, trainerStateMachine));
                break;

            case 3:
                trainerStateMachine.ChangeState(new TrainerTakeCoverState(gameObject, GameObject.FindGameObjectWithTag("Enemy").transform, trainerStateMachine));
                break;

            case 4:
                trainerStateMachine.ChangeState(new TrainerChaseState(gameObject, GameObject.FindGameObjectWithTag("Enemy").transform, trainerStateMachine));
                break;

            default:
                Debug.Log("EXTRA OUTPUT NODE IN TRAINER DQL");
                break;
        }       

    }


    float GetRewardFromEnvironment()
{
    float reward = 0f;
    Enemy enemyHealth = enemy.GetComponent<Enemy>();
    Trainer trainerHealth = trainer.GetComponent<Trainer>();

    if (enemyHealth.WasHit)
    {
        reward += 0.04f; // Smaller reward for hitting the enemy
        enemyHealth.WasHit = false;
    }

    if (trainerHealth.WasHit)
    {
        reward -= 0.15f; // Slightly larger penalty for getting hit
        trainerHealth.WasHit = false;
    }

    if (enemyHealth.IsDead)
    {
        reward += 0.05f; // Smaller lump sum for enemy death
        enemyHealth.IsDead = false;
    }

    if (trainerHealth.IsDead)
    {
        reward -= 0.05f; // Reduce the impact of death
        trainerHealth.IsDead = false;
    }

    if (trainerHealth.WasHealed && trainerHealth.health < 100)
    {
        reward += 0.01f; // Incremental reward for healing
        trainerHealth.WasHealed = false;
    } else if (trainerHealth.WasHealed && trainerHealth.health >= 100)
        {
            reward -= 0.01f;
        }

    if (Time.time - trainerStateMachine.lastActionTime > idlingThreshold)
    {
        reward -= 0.01f; // Reduced penalty for idling
    }

    return reward;
}


public bool ShouldLoadTargetWeights()
{
    if (episodeCount > loadWeightThreshold)
    {
        episodeCount = 0;
        return true;
    }
    episodeCount++;
    return false;
}
    
    
}
