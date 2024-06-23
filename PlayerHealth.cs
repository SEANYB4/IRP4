using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    public float health = 100;

    public float maxHealth = 100;
    public Transform respawnPoint; // Assign this in the inspector

    public bool IsDead = false;

    public bool WasHit = false;

    public void TakeDamage(int damage)
    {

        health -= damage;

        WasHit = true;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {

        ScoreManager.instance.AddEnemyScore(1);
        Debug.Log("Player died.");
        IsDead = true;
        Respawn();
    }


    private void Respawn()

    {

        health = 100; // Reset health
        transform.position = respawnPoint.position; // Move player to respawn point
        Debug.Log("Player respawned.");
    }





    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
