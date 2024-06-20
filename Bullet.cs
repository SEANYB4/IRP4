using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{


    public float lifetime = 5f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifetime); // Destroy bullet after a certain time
    }


    void OnCollisionEnter(Collision collision)
    {

        // Debug.Log("Collided with: " + collision.gameObject.name); // Check what the bullet collides with

        // Check if object hit is an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Debug.Log("Hit an enemy!");

            // Apply damage to the enemy

            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            
            if (enemy != null)
            {
                enemy.TakeDamage(10);
            }
        } else if (collision.gameObject.CompareTag("Player"))
        {

            // Debug.Log("Player Hit!");
            GameObject player = GameObject.FindWithTag("Player");
            PlayerHealth pH = player.GetComponent<PlayerHealth>();
            
            if (player != null)
            {
                pH.TakeDamage(10);
            }

            
        } else if (collision.gameObject.CompareTag("Player"))
        {

            
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
            
            if (player != null)
            {
                player.TakeDamage(10);
            }

            
        }


        Destroy(gameObject); // Destroy bullet on collision
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
