using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private GunRecoil recoil;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform gunTip;
    public Transform grappleTip;
    [SerializeField] private Transform player;

    [Header("Gun Settings")]
    [SerializeField] private float bulletsPerSecond;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletDamage;
    [SerializeField] private float shakeStrength;
    [SerializeField] private float shakeDuration;

    [Header("Grapple Settings")]
    
    [SerializeField] private LayerMask whatIsGrappleable;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float grapplePower;
    [SerializeField] private float grappleDrag;
    [SerializeField] private float grappleShakeStrength;
    [SerializeField] private float grappleShakeDuration;
    
    private bool grappling;
    private Vector3 grapplePoint;
    private bool validGrapple;
    private float defaultDrag;

    [Header("Object Pool")]
    [SerializeField] private Transform bulletContainer;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int defaultCapacity;
    [SerializeField] private int maxCapacity;

    private float attackTimer;
    private float MaxAttackTimer => 1.0f / bulletsPerSecond;

    private IObjectPool<Bullet> objectPool;
    

    private void Awake()
    {
        attackTimer = 0;
        defaultDrag = rb.drag;
        objectPool = new ObjectPool<Bullet>(CreateProjectile, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, true, defaultCapacity, maxCapacity);
    }

    private Bullet CreateProjectile()
    {
        var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity, bulletContainer).GetComponent<Bullet>();
        bullet.Pool = objectPool;
        bullet.gameObject.SetActive(false);
        return bullet;
    }

    private void OnReleaseToPool(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void OnGetFromPool(Bullet bullet)
    {
        bullet.transform.position = transform.position;
        bullet.gameObject.SetActive(true);
    }

    private void OnDestroyPooledObject(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)){
            Vector3 targetPos = new(hit.point.x, transform.position.y, hit.point.z);
            transform.LookAt(targetPos);
        }
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        else if (Input.GetMouseButton(0))
        {
            Attack();
        }
        if (Input.GetMouseButtonDown(1))
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            StopGrapple();
        }
        if(grappling && validGrapple && Vector2.Distance(new Vector2(grapplePoint.x, grapplePoint.z), new Vector2(transform.position.x, transform.position.z)) > maxDistance * 1.25f)
        {
            StopGrapple();
        }
    }

    private void FixedUpdate()
    {
        if(grappling && validGrapple)
        {
            Vector3 direction = grapplePoint - transform.position;
            direction.y = 0;
            rb.AddForce(direction.normalized * grapplePower);
            rb.drag = grappleDrag;
        }
        else
        {
            rb.drag = defaultDrag;
        }
    }

    private void StartGrapple()
    {
        if (grappling) return;
        grappling = true;
        AudioManager.instance.Play("Grapple");
        GameManager.instance.CameraShake(grappleShakeStrength, grappleShakeDuration, CinemachineImpulseDefinition.ImpulseShapes.Rumble);
        Vector3 direction = transform.forward;
        if (Physics.Raycast(grappleTip.position, direction, out RaycastHit hit, maxDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            validGrapple = true;
        }
        else
        {
            grapplePoint = grappleTip.position + maxDistance * 0.9f * transform.forward;
            validGrapple = false;
            StartCoroutine(CancelGrapple());
        }
        

    }

    private IEnumerator CancelGrapple()
    {
        yield return new WaitForSeconds(0.6f);
        grappling = false;
        validGrapple = true;
    }

    private void StopGrapple()
    {
        if (!validGrapple) return;
        grappling = false;
    }

    public bool IsGrappling()
    {
        return grappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

    private void Attack()
    {
        
        Bullet bullet = objectPool.Get();
        if (bullet != null)
        {
            recoil.Attack(MaxAttackTimer);
            attackTimer = MaxAttackTimer;
            AudioManager.instance.Play("Shoot");
            GameManager.instance.CameraShake(shakeStrength, shakeDuration);
            bullet.Init(gunTip.position, Quaternion.LookRotation(transform.forward), bulletSpeed, bulletDamage);
        }
    }


}
