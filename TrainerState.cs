using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public abstract class TrainerState
{
    protected GameObject trainer;
    protected Transform enemyTransform;
    protected TrainerStateMachine trainerStateMachine;

    protected TrainerGun trainerGun;
    
    
    


    public TrainerState(GameObject trainer, Transform enemyTransform, TrainerStateMachine trainerStateMachine)
    {
        this.trainer = trainer;
        this.enemyTransform = enemyTransform;
        this.trainerStateMachine = trainerStateMachine;
         trainerGun = trainer.GetComponent<TrainerGun>();
        if (trainerGun == null)
        {
            //Debug.LogError("EnemyGun component not found on the enemy object");

        }
    }


    public abstract void Tick();
    public virtual void OnStateEnter() 
    {
        
    }
    public virtual void OnStateExit() { } 

    


    protected void TryShoot()
{
    if (trainerGun != null && Time.time > trainerStateMachine.lastShootTime + trainerStateMachine.shootingInterval)
    {
        trainerGun.Shoot();
        trainerStateMachine.lastShootTime = Time.time;
        trainerStateMachine.lastActionTime = Time.time;
    }
}


    protected bool IsPathClear(Vector3 targetPosition)
    {
        RaycastHit hit;
        Vector3 direction = targetPosition - trainer.transform.position;
        float distance = Vector3.Distance(trainer.transform.position, targetPosition);


        if (Physics.Raycast(trainer.transform.position, direction.normalized, out hit, distance))
        {
            // Check if the ray has hit a non-passable collider
            if (!hit.collider.isTrigger)
            {
                return false;
            }
        }
        return true; // Path is clear
    }

    protected void Heal()
        {

            var trainerComponent = trainer.GetComponent<Trainer>();
            
            //Debug.Log("Enemy healing!");
            //Debug.Log("Enemy Health: " + enemyComponent.health);
            if (trainerComponent.health <= trainerStateMachine.maxHealth)
            {
                trainerComponent.health += trainerStateMachine.healingRate * Time.deltaTime;
                trainerComponent.WasHealed = true;
            }
            //trainerStateMachine.lastActionTime = Time.time;

        }
    
}



public class TrainerChaseState : TrainerState
{


    public float chaseSpeed = 20.0f;



    public TrainerChaseState(GameObject enemy, Transform enemyTransform, TrainerStateMachine trainerStateMachine)
    : base(enemy, enemyTransform, trainerStateMachine) {}


    public override void Tick()
    {
        Vector3 targetPosition = Vector3.MoveTowards(trainer.transform.position, enemyTransform.position, Time.deltaTime * chaseSpeed);
        targetPosition.y = 1;
        
        //if (IsPathClear(targetPosition))
        //{
            trainer.transform.position = targetPosition;
        //}

        TryShoot();
    }


    

    public override void OnStateEnter()
    {
        trainerStateMachine.lastShootTime = Time.time;
        //Debug.Log("Chase");
        trainerStateMachine.shootingInterval = 1.5f;
    }
}


public class TrainerAttackState : TrainerState
{
    

    public TrainerAttackState(GameObject trainer, Transform enemyTransform, TrainerStateMachine trainerStateMachine)
    : base(trainer, enemyTransform, trainerStateMachine) 
    {
       

    }

    public override void Tick()
    {
        TryShoot();
    }

    


    public override void OnStateEnter()
    {
        // stateMachine.lastShootTime = Time.time - shootingInterval; // Allows shooting straight away
        
        //Debug.Log("Attack");
        trainerStateMachine.shootingInterval = 1.0f;
    }

}


public class TrainerRetreatState : TrainerState
{
    
    private float retreatSpeed = 10.0f;
    
    



    public TrainerRetreatState(GameObject trainer, Transform enemyTransform, TrainerStateMachine trainerStateMachine) : base(trainer, enemyTransform, trainerStateMachine)
    {

        

    }

    public override void OnStateEnter()
    {
        //Debug.Log("Retreat");
        trainerStateMachine.shootingInterval = 2.0f;
    }

    public override void OnStateExit()
    {
        //Debug.Log("Exiting Retreat State");
    }


    public override void Tick()
    {
        // Move away from enemy
        Vector3 directionFromPlayer = trainer.transform.position - enemyTransform.position;
        Vector3 retreatDirection = directionFromPlayer.normalized;


        Vector3 newPosition = trainer.transform.position;
        newPosition += retreatDirection * retreatSpeed * Time.deltaTime;
        newPosition.y = 1;

        //if (IsPathClear(newPosition))
        //{
            trainer.transform.position = newPosition;
        //}

        TryShoot();
    }

    
}


public class TrainerTakeCoverState : TrainerState
{

    
    private Transform coverSpot;
    public bool inCover;

    public float moveToCoverSpeed = 20.0f;
   
    public TrainerTakeCoverState(GameObject trainer, Transform enemyTransform, TrainerStateMachine trainerStateMachine) : base(trainer, enemyTransform, trainerStateMachine)
    {
       
        
    }


    public override void OnStateEnter()
    {
        //Debug.Log("Take Cover");
        FindNearestCover();
        trainerStateMachine.shootingInterval = 1.0f;
       
    }


    public override void OnStateExit()
    {
        // Actions to perform when exiting the cover state
        coverSpot = null;
    }


    public override void Tick()
    {
       if (coverSpot != null) {
        MoveToCover(); // Continuously move to cover
       }  

       

       
    }


    



    private void FindNearestCover()
    {
        float closestDistanceSqr = Mathf.Infinity;
        GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
        Vector3 currentPosition = trainer.transform.position;



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


    private void MoveToCover()

    {
         if (Vector3.Distance(trainer.transform.position, coverSpot.position) > 8f)
        {
            Vector3 targetPosition = Vector3.MoveTowards(trainer.transform.position, coverSpot.position, Time.deltaTime * moveToCoverSpeed);
            targetPosition.y = 1;

            //if (IsPathClear(targetPosition))
            //{
                trainer.transform.position = targetPosition;
            //}

        } 
    }

}



public class TrainerHideInCoverState : TrainerState
{

    public TrainerHideInCoverState(GameObject trainer, Transform enemyTransform, TrainerStateMachine trainerStateMachine) : base(trainer, enemyTransform, trainerStateMachine)
        {

        }

    public override void OnStateEnter()
    {
        trainerStateMachine.shootingInterval = 3.0f;
    }

    public override void Tick() {
       Heal();
       TryShoot();
    }


   
    
}


public class TrainerStateMachine {

    public float lastShootTime;
    public float shootingInterval = 1.0f; // Time between shots
    public float lastActionTime;
    public float healingRate = 10.0f; // Health per second
    public float maxHealth = 100.0f;


    public TrainerState currentState;


    public void ChangeState(TrainerState newState)
    {

        if (currentState != null && currentState.GetType() == newState.GetType())
        {
            return;
        }

        if (currentState != null) 
        {
            currentState.OnStateExit();
        }

        currentState = newState;
        currentState.OnStateEnter();
    }


    public void Start()
    {
        lastShootTime = Time.time - shootingInterval;
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.Tick();
        }
    }
}
