using UnityEngine;
using UnityEngine.UI; // Ensure this is included to use the Image type

public class HealthBar : MonoBehaviour
{
    public Image healthBarFill; // Reference to the UI Image for the health fill
    private GameObject player; // Reference to the trainer's health component
    private PlayerHealth playerHealth;

    private float originalWidth;
    // Start is called before the first frame update
    void Start()
    {
        
       
        originalWidth = healthBarFill.rectTransform.sizeDelta.x;
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
       
    }

    // Update is called once per frame
    void Update()
    {
        
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        // Ensure that playerHealthComponent and healthBarFill are not null
        if (playerHealth != null && healthBarFill != null)
        {
            
           
            float healthPercentage = playerHealth.health / playerHealth.maxHealth;
            
            // Calculate the new width based on the fill area's original width
            
            float newWidth = healthPercentage * originalWidth;
            

            // Update the RectTransform of the healthBarFill
            healthBarFill.rectTransform.sizeDelta = new Vector2(newWidth, healthBarFill.rectTransform.sizeDelta.y);
        }
         else {
            Debug.Log("Health bar error...");
         }
    }
}