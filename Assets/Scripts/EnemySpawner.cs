using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private AnimationCurve difficultyCurve;
    [SerializeField] private Transform player;
    [SerializeField] private Vector2 healthRange;
    [SerializeField] private float spawnInterval;
    [SerializeField] private float spawnDistance;

    private float nextSpawn;
    private float timeElapsed;

    private void Awake()
    {
        nextSpawn = spawnInterval;
        timeElapsed = 0f;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (nextSpawn > 0)
        {
            nextSpawn -= Time.deltaTime;
        }
        else
        {
            Vector2 spawnLocation = spawnDistance * Random.insideUnitCircle.normalized;
            var enemy = Instantiate(enemyPrefab, new Vector3(spawnLocation.x, 1.5f, spawnLocation.y), Quaternion.identity, transform).GetComponent<Enemy>();
            enemy.Init(player, Random.Range(healthRange.x, healthRange.y));
            nextSpawn = spawnInterval / difficultyCurve.Evaluate(timeElapsed / 60f);
        }
        timeElapsed += Time.deltaTime;
        
    }
}
