using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private float waitTime;
    [SerializeField] private float radius;
    private void Awake()
    {
        StartCoroutine(Death());
    }

    private IEnumerator Death()
    {
        GameManager.instance.CameraShake(0.3f, -Vector3.one.normalized, waitTime, Cinemachine.CinemachineImpulseDefinition.ImpulseShapes.Rumble);
        yield return new WaitForSeconds(waitTime);
        GameManager.instance.CameraShake(1.25f, -Vector3.one.normalized, 0.4f, Cinemachine.CinemachineImpulseDefinition.ImpulseShapes.Explosion);
        var colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var item in colliders)
        {
            if (item.gameObject.CompareTag("Enemy"))
            {
                var enemy = item.GetComponent<Enemy>();
                Vector3 dir = item.transform.position - transform.position;
                dir.y = 0f;
                enemy.TakeDamage(0.0f, dir, 45f);
            }
        }
        yield return new WaitForSeconds(0.3f);
        foreach (var item in colliders)
        {
            
            if (item.gameObject.CompareTag("Enemy"))
            {
                var enemy = item.GetComponent<Enemy>();
                enemy.TakeDamage(1000f, Vector3.zero, 0f);
            }
        }
        yield return new WaitForSeconds(0.5f);
        GameManager.instance.EndGame();
        Destroy(gameObject);
    }
}
