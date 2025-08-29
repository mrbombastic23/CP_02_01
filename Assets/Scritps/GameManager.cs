using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI targetObjectText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;

    public EnemySpawner spawner;

    private int score = 0;
    private int lives = 3;

    private Enemy trueEnemy;
    private string currentLetter;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartRound();
    }

    public void StartRound()
    {
        List<Enemy> enemies = spawner.SpawnEnemies();

        // Seleccionar enemigo verdadero
        trueEnemy = enemies[Random.Range(0, enemies.Count)];
        targetObjectText.text = "Target: " + trueEnemy.word;

        currentLetter = trueEnemy.letter;
    }

    public void OnPlayerGesture(string recognizedLetter)
    {
        if (recognizedLetter == currentLetter)
        {
            DestroyAllEnemies();
            AddScore(100);
            StartRound();
        }
        else
        {
            Debug.Log("Letra incorrecta");
        }
    }

    public void EnemyReachedPlayer()
    {
        lives--;
        livesText.text = "Lives: " + lives;

        if (lives <= 0)
        {
            GameOver();
        }
    }

    private void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
    }

    private void DestroyAllEnemies()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
    }

    private void GameOver()
    {
        targetObjectText.text = "GAME OVER";
        spawner.StopSpawning();
    }
}
