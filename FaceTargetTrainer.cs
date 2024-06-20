using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTargetTrainer : MonoBehaviour
{

    public Transform target;
    // Start is called before the first frame update
    void Start()
    {

        target = GameObject.FindWithTag("Enemy").transform;
        
    }

    // Update is called once per frame
    void Update()
    {

        if (target != null)
        {
            Vector3 directionToTarget = target.position - transform.position;
            directionToTarget.y = 0; // This ensures the enemy only rotates on the y-axis


            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
            // Smooth rotation
        } else 
        {
            target = GameObject.FindWithTag("Enemy").transform;
        }
        
    }
}
