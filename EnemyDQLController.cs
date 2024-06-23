using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDQLController : MonoBehaviour
{

    public GameObject enemy;

    public GameObject player;
    

    

    public EnemyGun enemyGun;




    private Transform coverSpot;
    public bool inCover;

    public float moveToCoverSpeed;
    public float lastShootTime;

    public float shootingInterval; // Time between shots

    public float chaseSpeed;

    private float retreatSpeed;

    public float healingRate; // Health per second
    public float maxHealth;



     public EnemyDQLController()
    {
        moveToCoverSpeed = 20.0f;
        shootingInterval = 0.3f;
        chaseSpeed = 20.0f;
        retreatSpeed = 20.0f;
        healingRate = 10.0f;
        maxHealth = 100.0f;

        
    }


    // Start is called before the first frame update
    void Start()
    {
        
        lastShootTime = Time.time;

        enemy = GameObject.FindGameObjectWithTag("Enemy");
        player = GameObject.FindGameObjectWithTag("Player");

        if (enemy == null)
        {
            Debug.LogError("Enemy does not exist");
        }
        
        enemyGun = enemy.GetComponent<EnemyGun>();
        if (enemyGun == null)
        {
            Debug.LogError("EnemyGun component not found on the enemy object");

        }
        
    }

    // Update is called once per frame
    void Update()
    {

        
    }


    // ACTIONS


    public void TryShoot()
    {
       
        if (enemyGun != null && (Time.time > (lastShootTime + shootingInterval)))
        {
            enemyGun.Shoot();
            lastShootTime = Time.time;
        }
        
    }




    public void Heal()
    {
        
        var enemyComponent = enemy.GetComponent<Enemy>();
        
        
        if (enemyComponent.health <= maxHealth)
        {
            
            enemyComponent.health += healingRate * Time.deltaTime;
            enemyComponent.WasHealed = true;   
        }
        

    }


    public void Chase()
    {
        
        Vector3 targetPosition = Vector3.MoveTowards(enemy.transform.position, player.transform.position, Time.deltaTime * chaseSpeed);
        targetPosition.y = 1;
        enemy.transform.position = targetPosition;

    }


    public void Retreat()
    {
        
        Vector3 directionFromPlayer = enemy.transform.position - player.transform.position;
        Vector3 retreatDirection = directionFromPlayer.normalized;


        Vector3 newPosition = enemy.transform.position;
        newPosition += retreatDirection * retreatSpeed * Time.deltaTime;
        newPosition.y = 1;

        //if (IsPathClear(newPosition))
        //{
            enemy.transform.position = newPosition;
        //}
    }


    public void MoveToCover()

    {
        
        FindNearestCover();


         if (Vector3.Distance(enemy.transform.position, coverSpot.position) > 8f)
        {
            Vector3 targetPosition = Vector3.MoveTowards(enemy.transform.position, coverSpot.position, Time.deltaTime * moveToCoverSpeed); // Assuming 5.0f is the movement speed
            targetPosition.y = 1;

            //if (IsPathClear(targetPosition))
            //{
                enemy.transform.position = targetPosition;
            //}
            
        } 
    }








    // HELPER METHODS

    public void FindNearestCover()
    {
        float closestDistanceSqr = Mathf.Infinity;
        GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
        Vector3 currentPosition = enemy.transform.position;



        foreach (GameObject cover in coverObjects)
        {
            Vector3 directionToCover = cover.transform.position - currentPosition;
            float distanceSqr = directionToCover.sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                coverSpot = cover.transform;
            }
        }
    }


    public bool IsPathClear(Vector3 targetPosition)
    {
        RaycastHit hit;
        Vector3 direction = targetPosition - enemy.transform.position;
        float distance = Vector3.Distance(enemy.transform.position, targetPosition);


        if (Physics.Raycast(enemy.transform.position, direction.normalized, out hit, distance))
        {
            // Check if the ray has hit a non-passable collider
            if (!hit.collider.isTrigger)
            {
                return false;
            }
        }
        return true; // Path is clear
    }


    
}
