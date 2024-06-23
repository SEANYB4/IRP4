using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public class DQLAgent : MonoBehaviour
{
    NeuralNetwork qNetwork;
    NeuralNetwork targetNetwork;


    public int inputSize = 3; // Define according to your specific environment
    private int outputSize = 5; // Define according to your specific environment
    float discountFactor = 0.9f; // Emphasize or de-emphasize future rewards
    float explorationRate = 0.2f;
    float explorationDecay = 0.995f;
    float minExplorationRate = 0.01f;
    List<float[]> stateMemory = new List<float[]>(); // S
    List<int> actionMemory = new List<int>(); // A
    List<float> rewardMemory = new List<float>(); // R
    List<float[]> nextStateMemory = new List<float[]>(); // S1

    

   

 


    public int batchSize = 32; // The size of the batch to train on each frame



    // Variables for saving weights

    private int episodeCount = 0;


    private bool shouldLoadWeights = true;
    
    private int loadWeightThreshold = 50;
    
    

    // To reference other scripts

    public GameObject player;
    public GameObject enemy;




  

    // Reference to EnemyDQLController

    private EnemyDQLController enemyDQLController;


    // Action change

    public float actionChangeThreshold = 1.0f;

    public float lastActionChange;
    

    
    private float resetHeight = -10f; // Height at which the enemy will reset



    // Start is called before the first frame update
    void Start()
    {
        
        qNetwork = new NeuralNetwork(new int[] { inputSize, 64, 64, outputSize });
        targetNetwork = new NeuralNetwork(new int[] { inputSize, 64, 64, outputSize});


        if (shouldLoadWeights)
        {
            string savedWeights = PlayerPrefs.GetString("QNetworkWeights", "");

            if (!string.IsNullOrEmpty(savedWeights))
            {
                qNetwork.WeightsFromJson(savedWeights);
                targetNetwork.WeightsFromJson(savedWeights);
            }
        } else
        {
            Debug.Log("No Weights to Load....");
            targetNetwork.weights = qNetwork.weights;
        }


         


        string weightsJson = qNetwork.WeightsToJson();
        PlayerPrefs.SetString("QNetworkWeights", weightsJson);
        Debug.Log(PlayerPrefs.GetString("QNetworkWeights"));
        PlayerPrefs.Save(); // Make sure to save PlayerPrefs changes
        Debug.Log("Weights saved to PlayerPrefs");


        player = GameObject.FindGameObjectWithTag("Player");
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        

        enemyDQLController = enemy.GetComponent<EnemyDQLController>();

        
        lastActionChange = Time.time;
        
    }




    // Update is called once per frame
    void Update()
    {

        // Calling these in update because new enemy objects are created after death
        player = GameObject.FindGameObjectWithTag("Player");
        enemy = GameObject.FindGameObjectWithTag("Enemy");

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


        //if (Time.time > lastActionChange + actionChangeThreshold)
        //{

        
            float[] currentState = GetStateFromEnvironment();
            int action = ChooseAction(currentState);
            PerformAction(action);
            float reward = GetRewardFromEnvironment();
            float[] nextState = GetStateFromEnvironment();
            Remember(currentState, action, reward, nextState);
            Replay(); // Train on a mini-batch every frame
        //}
        
    
            
        

        
        




        // Periodically load network weights into target network and save weights to
        // file at the same time
        
        if (ShouldLoadTargetWeights())
        {
            string weightsJson = qNetwork.WeightsToJson();
            targetNetwork.weights = qNetwork.weights;
            PlayerPrefs.SetString("QNetworkWeights", weightsJson);
            
            
            //Debug.Log(weightsJson);  

            PlayerPrefs.Save(); // Make sure to save PlayerPrefs changes

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
            
            
            // Print Q Values
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
        // Debug.Log(stateMemory.Count);
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
        float[] nextQs = targetNetwork.FeedForward(nextInputs); // use target network for stability
        float maxNextQ = nextQs.Max();

        float[] newQs = (float[])currentQs.Clone();
        newQs[action] = reward + discountFactor * maxNextQ;

        qNetwork.BackPropagate(newQs);
    }


    float[] GetStateFromEnvironment()
    {
        // Check if the player or enemy is not assigned
        if (player == null || enemy == null)
        {
            Debug.LogError("Player or Enemy reference not set in DQL Agent.");
            return new float[0]; // Return an empty array or handle this case as needed
        }

        float playerHealth = player.GetComponent<PlayerHealth>().health;
        float enemyHealth = enemy.GetComponent<Enemy>().health;
        float distanceToEnemy = Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position);
        
        return new float[] { playerHealth, enemyHealth, distanceToEnemy };

    }


    void PerformAction(int action)
    {
        
        switch (action)
        {

            case 0:
                enemyDQLController.TryShoot();
                lastActionChange = Time.time;
                break;

            case 1:
                enemyDQLController.Heal();
                lastActionChange = Time.time;
                break;

            case 2:
                enemyDQLController.Chase();
                lastActionChange = Time.time;
                break;

            case 3:
                enemyDQLController.Retreat();
                lastActionChange = Time.time;
                break;

            case 4:
                enemyDQLController.MoveToCover();
                lastActionChange = Time.time;
                break;

            default:
                Debug.Log("EXTRA OUTPUT NODE");
                break;
        }       

    }


    float GetRewardFromEnvironment()
{
    float reward = 0f;
    PlayerHealth playerScript = player.GetComponent<PlayerHealth>();
    Enemy enemyScript = enemy.GetComponent<Enemy>();

    if (playerScript.WasHit)
    {
        reward += 0.04f; // Smaller reward for hitting the player
        playerScript.WasHit = false;
    }

    if (enemyScript.WasHit)
    {
        reward -= 0.15f; // Slightly larger penalty for getting hit
        enemyScript.WasHit = false;
    }

    if (playerScript.IsDead)
    {
        reward += 0.05f; // Smaller lump sum for player death
        playerScript.IsDead = false;
    }

    if (enemyScript.IsDead)
    {
        reward -= 0.05f; // Reduce the impact of death
        enemyScript.IsDead = false;
    }

    if (enemyScript.WasHealed && enemyScript.health < 100)
    {
        reward += 0.01f; // Incremental reward for healing
        enemyScript.WasHealed = false;
    } else if (enemyScript.WasHealed && enemyScript.health >= 100)
        {
            reward -= 0.01f;
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
