using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject impact;
    [SerializeField] private Transform tail;

    private IObjectPool<Bullet> pool;
    public IObjectPool<Bullet> Pool { set => pool = value; }

    private Coroutine disableCoroutine;
    private float damage;

    public void Init(Vector3 position, Quaternion rotation, float speed, float damage)
    {
        rb.position = position;
        transform.rotation = rotation;
        rb.velocity = transform.forward * speed;
        this.damage = damage;
        disableCoroutine = StartCoroutine(Deactivate());
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }
        //print(other.gameObject);
        if (other.CompareTag("Enemy"))
        {
            if (disableCoroutine != null)
            {
                StopCoroutine(disableCoroutine);
            }
            var enemy = other.GetComponent<Enemy>();
            enemy.TakeDamage(damage, transform.forward, 0.1f);

        }
        
        Instantiate(impact, tail.position, Quaternion.identity);
        
        AudioManager.instance.Play("Hit");
        pool.Release(this);
    }

    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(2f);

        pool.Release(this);
    }
}
