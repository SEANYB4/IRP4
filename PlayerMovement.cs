using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float speed = 5.0f;
    public float jumpForce = 7.0f;

    private bool isGrounded;


    public float mouseSensitivity = 300.0f;
   
    private float xRotation = 0f; // To store the vertical rotation of the camera
    

    // Reference to the camera component
    public Camera playerCamera;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {


        Vector3 movement = Vector3.zero;

        // Check each key individually

        if (Input.GetKey(KeyCode.W))
        {

            movement += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += Vector3.right;
        }

        // Normalize the movement vector to ensure consistent speed in all directions
        movement = movement.normalized;


        // Apply the movement
        transform.Translate(movement * speed * Time.deltaTime);



        // Jumping logic

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {

            // Apply a vertical force
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;


        }




         // Handle mouse movement
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Vertical rotation for looking up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamping rotation to prevent flipping



        // Apply rotations
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
        
    }



    void OnCollisionEnter(Collision collision)
    {

        // Check if the collision is with the ground
        if (collision.contacts[0].normal == Vector3.up)
        {

            isGrounded = true;
        }
    }
}
