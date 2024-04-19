using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AfterImage afterImage;
    [SerializeField] private GameObject deathParticle;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float afterImageThreshold;

    [SerializeField] private float dashShakeAmount;
    [SerializeField] private float dashShakeDuration;

    [SerializeField] private float invincibilityTime;
    [SerializeField] private int playerLayer;
    [SerializeField] private int enemyLayer;

    private float MaxWalkSpeed => (walkSpeed / rb.drag - (Time.fixedDeltaTime * walkSpeed));

    private Vector2 moveInput;
    private float dashCooldownTime;

    private float speed;
    private bool invincible;

    private void Awake()
    {

        dashCooldownTime = 0;
        afterImage.showAfterImage = false;
        invincible = false;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        moveInput = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        speed = walkSpeed * 10f;
        if(dashCooldownTime > 0)
        {
            dashCooldownTime -= Time.deltaTime;
        }
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }
        if(rb.velocity.sqrMagnitude - MaxWalkSpeed * MaxWalkSpeed * 100f > afterImageThreshold)
        {
            afterImage.showAfterImage = true;
        }
        else
        {
            afterImage.showAfterImage = false;
        }
        
    }

    private void FixedUpdate()
    {
        moveInput.Normalize();
        rb.AddForce(new(moveInput.x * speed, 0, moveInput.y * speed), ForceMode.Force);
        //rb.velocity = new(moveInput.x * speed, rb.velocity.y, moveInput.y * speed);
    }

    private void Dash()
    {
        if (dashCooldownTime > 0) return;
        moveInput.Normalize();
        Vector2 dashDirection = moveInput.sqrMagnitude == 0 ? new(transform.forward.x, transform.forward.z) : moveInput;
        dashDirection.Normalize();
        rb.AddForce(new(dashDirection.x * dashSpeed * 10f, 0, dashDirection.y * dashSpeed * 10f), ForceMode.Impulse);
        dashCooldownTime = dashCooldown;
        AudioManager.instance.Play("Dash");
        GameManager.instance.CameraShake(dashShakeAmount, Vector3.back, dashShakeDuration, Cinemachine.CinemachineImpulseDefinition.ImpulseShapes.Bump);
        StartCoroutine(Invincibility());
    }

    private IEnumerator Invincibility()
    {
        invincible = true;
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        yield return new WaitForSeconds(invincibilityTime);
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        invincible = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (invincible) return;
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Death();
        }
    }

    private void Death()
    {
        AudioManager.instance.Play("Hit");
        afterImage.ClearClones();
        Instantiate(deathParticle, transform.position + Vector3.up * 1.1f + transform.forward * 0.05f, Quaternion.identity);
        AudioManager.instance.Play("PlayerDeath");
        Destroy(gameObject);
    }
}
