using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;

public class ScoreManager : MonoBehaviour
{

    public static ScoreManager instance;
    public TextMeshProUGUI scoreText; // Use public Text scoreText


    public TextMeshProUGUI enemyText;
    private int playerScore = 0;

    private int enemyScore = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        
    }

   
    public void AddScore(int points)
    {
        playerScore += points;
        UpdateScoreUI();
    }


    public void AddEnemyScore(int points)
    {
        enemyScore += points;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Player: " + playerScore.ToString();
        enemyText.text = "Enemy: " + enemyScore.ToString();
    }
}
