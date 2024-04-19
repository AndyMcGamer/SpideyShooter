using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject enemyDeathParticle;

    private Transform player;
    private float health;
    private float startingHealth;

    public void Init(Transform target, float hp)
    {
        player = target;
        health = hp;
        startingHealth = hp;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        //var targetPosition = player.position;
        //targetPosition.y = 0;
        //transform.LookAt(targetPosition);
        if (player != null) agent.SetDestination(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    public void TakeDamage(float damage, Vector3 damageDirection, float knockback)
    {
        //anim.Play(idleAnim);
        health -= damage;
        rb.AddForce(damageDirection.normalized * knockback, ForceMode.Impulse);
        if(health <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        Instantiate(enemyDeathParticle, transform.position, Quaternion.identity);
        AudioManager.instance.Play("EnemyDeath");
        GameManager.instance.CameraShake(0.7f, -Vector3.one.normalized, 0.4f, Cinemachine.CinemachineImpulseDefinition.ImpulseShapes.Rumble);
        GameManager.instance.IncreaseScore(startingHealth * GameManager.instance.ScoreMultiplier);
        Destroy(gameObject);
    }

}
