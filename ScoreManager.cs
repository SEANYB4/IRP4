using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.VisualScripting;


public class ScoreManager : MonoBehaviour
{


    [DllImport("__Internal")]
    private static extern void SaveScoresToFirebase(int playerScore, int enemyScore, string agentType);
    void SaveScoresToDatabase()
    {
        // Call JavaScript method to save scores


        #if UNITY_WEBGL && !UNITY_EDITOR
        SaveScoresToFirebase(playerScore, enemyScore, agentType);
        #endif

    }

    public static ScoreManager instance;
    public TextMeshProUGUI scoreText; // Use public Text scoreText
    public string agentType = "FST";
    public TextMeshProUGUI enemyText;
    private int playerScore = 0;
    private int enemyScore = 0;

    private int winningScore = 1;



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
        CheckScores();
    }


    public void AddEnemyScore(int points)
    {
        enemyScore += points;
        UpdateScoreUI();
        CheckScores();
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Player: " + playerScore.ToString();
        enemyText.text = "Enemy: " + enemyScore.ToString();
    }


    void CheckScores()
    {
        if (playerScore >= winningScore || enemyScore >= winningScore)
        {
            playerScore = 0;
            enemyScore = 0;
            SaveScoresToDatabase();
        }
    }


   
    
}


[Serializable]
public class ScoreData
{
    public int playerScore;
    public int enemyScore;
    public string agentType;

    public ScoreData(int playerScore, int enemyScore, string agentType)
    {
        this.playerScore = playerScore;
        this.enemyScore = enemyScore;
        this.agentType = agentType;
    }
}
