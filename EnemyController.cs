using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    private StateMachine stateMachine;
    private Enemy enemy;

  
    
    // Start is called before the first frame update
    void Start()
    {

        enemy = GetComponent<Enemy>();


        stateMachine = new StateMachine();
        var chase = new ChaseState(gameObject, GameObject.FindGameObjectWithTag("Player").transform, stateMachine);

        stateMachine.ChangeState(chase); // Start in the chase state

        
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();



    }


}