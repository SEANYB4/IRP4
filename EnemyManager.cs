using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    public GameObject enemyPrefab; // Assign this in the inspector with prefab#
    public Transform[] spawnPoints; // Assign spawn points in the inspector

    public List<GameObject> enemies = new List<GameObject>();

    public static EnemyManager instance;

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
        SpawnEnemy(); // Spawn the enemy
    }



    public void SpawnEnemy()
    {
        if (spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            enemies.Add(newEnemy);
        }
    }

    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
        Destroy(enemy);
    }

    // Update is called once per frame
    void Update()
    {

        if (enemies.Count == 0)
        {
            SpawnEnemy();
        }
        
    }
}
