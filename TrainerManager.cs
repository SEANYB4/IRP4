using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrainerManager : MonoBehaviour
{

    public GameObject trainerPrefab; // Assign this in the inspector with prefab#
    public Transform[] spawnPoints; // Assign spawn points in the inspector

    public List<GameObject> trainers = new List<GameObject>();

    public static TrainerManager instance;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        SpawnTrainer(); // Spawn the enemy
    }



    public void SpawnTrainer()
    {
        if (spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject newTrainer = Instantiate(trainerPrefab, spawnPoint.position, spawnPoint.rotation);
            trainers.Add(newTrainer);
        }
    }

    public void RemoveTrainer(GameObject trainer)
    {
        trainers.Remove(trainer);
        Destroy(trainer);
    }

    // Update is called once per frame
    void Update()
    {

        if (trainers.Count == 0)
        {
            SpawnTrainer();
        }
        
    }
}
