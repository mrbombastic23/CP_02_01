using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;
    public int enemyCount = 4;
    public float spawnRadius = 5f;

    private bool canSpawn = true;

    public List<Enemy> SpawnEnemies()
    {
        List<Enemy> enemies = new List<Enemy>();

        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle.normalized * spawnRadius;
            GameObject obj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            Enemy enemy = obj.GetComponent<Enemy>();
            enemy.Init(RandomLetter(), RandomWord(), player);

            enemies.Add(enemy);
        }

        return enemies;
    }

    private string RandomLetter()
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return alphabet[Random.Range(0, alphabet.Length)].ToString();
    }

    private string RandomWord()
    {
        string[] words = { "CAT", "DOG", "APPLE", "HOUSE", "FISH", "TREE" };
        return words[Random.Range(0, words.Length)];
    }

    public void StopSpawning()
    {
        canSpawn = false;
    }
}


