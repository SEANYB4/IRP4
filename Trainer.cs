using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Trainer : MonoBehaviour
{

    public float health = 100; // Health of the enemy
    public float maxHealth = 100; // Max health of the enemy

    public bool IsDead = false;

    public bool WasHit = false;

    public bool WasHealed = false;


    private float resetHeight = -10f; // Height at which the enemy will reset

    private Vector3 resetPosition;


    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy hit! Remaining health: " + health);

        WasHit = true;
        
        // Check if health has fallen below zero
        if (health <= 0) {

            Die(); // Call the Die method if health is zero or less
        }
    }


    void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        // Here you can add animations, effects, or other game logic

        TrainerManager.instance.RemoveTrainer(GameObject.FindGameObjectWithTag("Trainer"));
      
        IsDead = true;

        ScoreManager.instance.AddEnemyScore(1);
                
    }

    

    // Start is called before the first frame update
    void Start()
    {
        resetPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
                    // Check if the enemy has fallen below the reset height
        if (transform.position.y < resetHeight)
        {
            // Reset position to (0, 0, 0) or any other specified respawn point
            transform.position = resetPosition;
            
        }
    }
}
