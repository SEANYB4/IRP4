using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    public Transform bulletSpawn; // Point from which bullets are fired
    public GameObject bulletPrefab; // Bullet prefab

    public float bulletSpeed = 1000f; // Speed of the bullet

    public float shootingInterval = 1f; // Time between shots

    private float lastShootTime; // Time when last shot was fired


    public float damage = 10f;
    public float range = 100f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1") && CanShoot())
        {
            Shoot();
        }
    }

    void Shoot()
    {
        lastShootTime = Time.time; // Update last shoot time
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation * Quaternion.Euler(90, 0, 0));
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.AddForce(bulletSpawn.forward * bulletSpeed);

       
    }

    bool CanShoot()
    {
        // Check if enough time has passed since the last shot
        if (Time.time > lastShootTime + shootingInterval)
        {
           return true;

        }
        Debug.Log("Can't shoot yet....");
        return false;
    }
}
