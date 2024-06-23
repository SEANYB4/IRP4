

using UnityEngine;


public abstract class State
{
    protected GameObject enemy;
    protected Transform playerTransform;
    protected StateMachine stateMachine;

    protected EnemyGun enemyGun;
    
    
    


    public State(GameObject enemy, Transform playerTransform, StateMachine stateMachine)
    {
        this.enemy = enemy;
        this.playerTransform = playerTransform;
        this.stateMachine = stateMachine;
         enemyGun = enemy.GetComponent<EnemyGun>();
        if (enemyGun == null)
        {
            Debug.LogError("EnemyGun component not found on the enemy object");

        }
    }


    public abstract void Tick();
    public virtual void OnStateEnter() 
    {
        
    }
    public virtual void OnStateExit() { } 

    public abstract bool CheckTransition(); 


    protected void TryShoot()
{
    if (enemyGun != null && (Time.time > stateMachine.lastShootTime + stateMachine.shootingInterval))
    {

        enemyGun.Shoot();
        stateMachine.lastShootTime = Time.time;
        stateMachine.lastActionTime = Time.time;
    }
}

    protected bool IsPathClear(Vector3 targetPosition)
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


    protected void Heal()
        {

            var enemyComponent = enemy.GetComponent<Enemy>();
            
            //Debug.Log("Enemy healing!");
            //Debug.Log("Enemy Health: " + enemyComponent.health);
            if (enemyComponent.health <= stateMachine.maxHealth)
            {
                enemyComponent.health += stateMachine.healingRate * Time.deltaTime;
                enemyComponent.WasHealed = true;
            }
            //stateMachine.lastActionTime = Time.time;

        }
    
}



public class ChaseState : State
{

    private float attackRange = 10.0f;
    private float retreatHealthThreshold = 40.0f;

    public float chaseSpeed = 20.0f;



    public ChaseState(GameObject enemy, Transform playerTransform, StateMachine stateMachine)
    : base(enemy, playerTransform, stateMachine) {}


    public override void Tick()
    {
        TryShoot();
        Vector3 targetPosition = Vector3.MoveTowards(enemy.transform.position, playerTransform.position, Time.deltaTime * chaseSpeed);
        targetPosition.y = 1;


        //if (IsPathClear(targetPosition))
        //{
            enemy.transform.position = targetPosition;
        //}

        
    }


    public override bool CheckTransition()
    {


        if (Vector3.Distance(enemy.transform.position, playerTransform.position) < attackRange)
        {
            stateMachine.ChangeState(new AttackState(enemy, playerTransform, stateMachine));
            return true;
        }

        if (enemy.GetComponent<Enemy>().health <= retreatHealthThreshold)
        {
            stateMachine.ChangeState(new RetreatState(enemy, playerTransform, stateMachine));
            return true;
        }
        return false;
    }

    public override void OnStateEnter()
    {
        stateMachine.lastShootTime = Time.time;
        stateMachine.shootingInterval = 1.5f;
    }
}


public class AttackState : State
{
    
    private float maxAttackDistance = 30.0f;
    private float criticalHealthThreshold = 40.0f;

    public AttackState(GameObject enemy, Transform playerTransform, StateMachine stateMachine)
    : base(enemy, playerTransform, stateMachine) 
    {
       

    }

    public override void Tick()
    {
        TryShoot();
    }

    public override bool CheckTransition()
    {
        if (Vector3.Distance(enemy.transform.position, playerTransform.position) > maxAttackDistance)
        {
            stateMachine.ChangeState(new ChaseState(enemy, playerTransform, stateMachine));
            return true;
        }
        if (enemy.GetComponent<Enemy>().health <= criticalHealthThreshold)
        {
            stateMachine.ChangeState(new RetreatState(enemy, playerTransform, stateMachine));
            return true;
        }
        return false;
    }



    public override void OnStateEnter()
    {
        // stateMachine.lastShootTime = Time.time - shootingInterval; // Allows shooting straight away
        
        
        stateMachine.shootingInterval = 1.0f;
    }

}


public class RetreatState : State
{
    private float safeDistance = 30.0f;
    private float retreatSpeed = 20.0f;
    
    



    public RetreatState(GameObject enemy, Transform trainerTransform, StateMachine stateMachine) : base(enemy, trainerTransform, stateMachine)
    {

        

    }

    public override void OnStateEnter()
    {
        
        stateMachine.shootingInterval = 2.0f;
    }

    public override void OnStateExit()
    {
        
    }


    public override void Tick()
    {
        TryShoot();
        // Move away from trainer
        Vector3 directionFromPlayer = enemy.transform.position - playerTransform.position;
        Vector3 retreatDirection = directionFromPlayer.normalized;


        Vector3 newPosition = enemy.transform.position;
        newPosition += retreatDirection * retreatSpeed * Time.deltaTime;
        newPosition.y = 1;

        //if (IsPathClear(newPosition))
        //{
            enemy.transform.position = newPosition;
        //}

        
    }

    public override bool CheckTransition()
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, playerTransform.position);


        if (distanceToPlayer >= safeDistance)
        {
            stateMachine.ChangeState(new TakeCoverState(enemy, playerTransform, stateMachine));
            return true;
        }
        return false;
    }
}


public class TakeCoverState : State
{

    
    private Transform coverSpot;
    public bool inCover;

    public float moveToCoverSpeed = 20.0f;
   
    public TakeCoverState(GameObject enemy, Transform playerTransform, StateMachine stateMachine) : base(enemy, playerTransform, stateMachine)
    {
       
        
    }


    public override void OnStateEnter()
    {
        //Debug.Log("Take Cover");
        FindNearestCover();
        stateMachine.shootingInterval = 1.0f;
       
    }


    public override void OnStateExit()
    {
        // Actions to perform when exiting the cover state
        coverSpot = null;
    }


    public override void Tick()
    {
        TryShoot();
       if (coverSpot != null) {
        MoveToCover(); // Continuously move to cover
       }  

       
        
       
    }


    public override bool CheckTransition()
    {

         float distanceToPlayer = Vector3.Distance(enemy.transform.position, playerTransform.position);

        
        if (Vector3.Distance(enemy.transform.position, coverSpot.position) <=  8.0f)
        {
            stateMachine.ChangeState(new HideInCoverState(enemy, playerTransform, stateMachine));
            return true;
        }

        if (distanceToPlayer <= 10.0f)
        {
            stateMachine.ChangeState(new AttackState(enemy, playerTransform, stateMachine));
        }

        return false;
    }



    private void FindNearestCover()
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


    private void MoveToCover()

    {
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

}



public class HideInCoverState : State
{

    


    public HideInCoverState(GameObject enemy, Transform playerTransform, StateMachine stateMachine) : base(enemy, playerTransform, stateMachine)
        {

        }

    public override void OnStateEnter()
    {
        
        stateMachine.shootingInterval = 3.0f;
    }

    public override void Tick() {
       TryShoot();
       Heal();
       
       
    }


    public override bool CheckTransition()
    {
        if (enemy.GetComponent<Enemy>().health >= 100)
        {
            stateMachine.ChangeState(new ChaseState(enemy, playerTransform, stateMachine));
            return true;
        }

        return false;
    }

    
}


public class StateMachine {


    public float lastShootTime;

    public float shootingInterval = 0.3f; // Time between shots

    public float lastActionTime;


    public float healingRate = 10.0f; // Health per second
    public float maxHealth = 100.0f;

   

    public State currentState;


    public void ChangeState(State newState)
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

            // Comment out CheckTransition for DQL Agent implementation
            currentState.CheckTransition(); // Check for transitions
        }
    }
}
