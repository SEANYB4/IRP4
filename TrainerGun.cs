using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerGun : MonoBehaviour
{

    public Transform bulletSpawn; // Point from which bullets are fired
    public GameObject bulletPrefab; // Bullet prefab


    public float bulletSpeed = 1000f;
    public float damage = 10f;
    public float range = 100f;


    
    public Transform target; // Target to shoot at



    // Start is called before the first frame update
    void Start()
    {
        // Find and assign the target by tag
        GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObject != null)
        {
            target = enemyObject.transform;
        }
        else
        {
            Debug.LogError("Enemy object not found. Please ensure your Enemy is tagged correctly.");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawRay(bulletSpawn.position, bulletSpawn.forward * 5, Color.red);   
    }


    public void Shoot()
    {
        
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation * Quaternion.Euler(90, 0, 0));
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.AddForce(bulletSpawn.forward * bulletSpeed); // Propel the bullet forward
    }
}
