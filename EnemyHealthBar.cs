using UnityEngine;
using UnityEngine.UI; // Ensure this is included to use the Image type

public class EnemyHealthBar : MonoBehaviour
{
    public Image enemyHealthBarFill; // Reference to the UI Image for the health fill
   
    private GameObject enemy;

    private Enemy enemyComponent; // Reference to the enemy script attached to the enemy Game object

    private float originalWidth;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError("Enemy script not found on the enemy object!");
        }
        
        
        originalWidth = enemyHealthBarFill.rectTransform.sizeDelta.x;
    }

    // Update is called once per frame
    void Update()
    {
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError("Enemy script not found on the enemy object!");
        }
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        // Ensure that playerHealthComponent and healthBarFill are not null
        if (enemyComponent != null && enemyHealthBarFill != null)
        {
            float healthPercentage = enemyComponent.health / enemyComponent.maxHealth;
            
            // Calculate the new width based on the fill area's original width
            float newWidth = healthPercentage * originalWidth;

            // Update the RectTransform of the healthBarFill
            enemyHealthBarFill.rectTransform.sizeDelta = new Vector2(newWidth, enemyHealthBarFill.rectTransform.sizeDelta.y);
        }
         else {
            Debug.Log("Enemy health bar error...");
         }
    }
}