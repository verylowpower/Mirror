//cần gọi Character.instance.ModifyHealth(-damage); trong hàm OnTriggerEnter2D của dash hitbox

using System;
using System.Collections;
using UnityEngine;

public class Boss : Enemy
{
    private enum BossState { Idle, Chasing, Shooting, Dashing }
    private BossState currentState = BossState.Idle;

    [Header("Shoot Settings")]
    public float detectRange = 10f;
    public float shootingRange = 7f;
    public float shootingFireRate = 1f;
    public int shootingDamage = 10;
    public int bulletsPerBurst = 3;
    public float shootTimer;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public int collisionDamage = 10;
    public float dashDuration = 0.5f;
    public float dashCooldown = 3f;
    public GameObject dashHitboxPrefab;

    private bool canDash = true;
    private bool isShootingBurst = false;

    protected override void Start()
    {
        base.Start(); // Gọi Enemy.Start()
        shootTimer = 0f;
    }

    void Update()
    {
        HandleBossBehavior();
    }

    private void HandleBossBehavior()
    {
        if (GameController.instance == null || GameController.instance.character == null)
            return;

        float distance = Vector2.Distance(transform.position, GameController.instance.character.position);

        switch (currentState)
        {
            case BossState.Idle:
                if (distance <= detectRange)
                    currentState = BossState.Chasing;
                break;

            case BossState.Chasing:
                if (distance <= shootingRange)
                {
                    currentState = BossState.Shooting;
                    rb.linearVelocity = Vector2.zero;
                }
                else
                {
                    ChasePlayer();
                }
                break;

            case BossState.Shooting:
                if (!isShootingBurst)
                {
                    StartCoroutine(ShootBurst());
                }
                break;

            case BossState.Dashing:
                // handled in coroutine
                break;
        }
    }

    private void ChasePlayer()
    {
        Vector2 dir = (GameController.instance.character.position - transform.position).normalized;
        rb.linearVelocity = dir * 3f; // tốc độ boss khi đuổi
    }

    private IEnumerator ShootBurst()
    {
        isShootingBurst = true;
        rb.linearVelocity = Vector2.zero;

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            Vector2 dir = (GameController.instance.character.position - firePoint.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();
            enemyBullet.MovementDirection = dir;
            enemyBullet.bulletSpeed = 8f;
            enemyBullet.bulletDmg = shootingDamage;

            yield return new WaitForSeconds(1f / shootingFireRate);
        }

        if (canDash)
            StartCoroutine(DashTowardsPlayer());
        else
            currentState = BossState.Chasing;

        isShootingBurst = false;
    }

    private IEnumerator DashTowardsPlayer()
    {
        canDash = false;
        currentState = BossState.Dashing;

        Vector2 dashDir = (GameController.instance.character.position - transform.position).normalized;

        // Tạo hitbox và gán damage qua script
        GameObject hitbox = Instantiate(dashHitboxPrefab, transform.position, Quaternion.identity, transform);
        var dashHitbox = hitbox.GetComponent<BossDashHitbox>();
        dashHitbox.damage = collisionDamage;

        float t = 0f;
        while (t < dashDuration)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(hitbox);
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(dashCooldown);

        currentState = BossState.Shooting;
        canDash = true;
    }



    public override void RunLightLogic() { }
    public override void RunHeavyLogic() { }

    public override void ChangeHealth(float amount)
    {
        base.ChangeHealth(amount);
        StartCoroutine(FlashWhenHit(spriteRender, originColor, flashColor, 0.1f));
        Debug.Log("[Boss] Boss HP changed: " + amount);
    }

    public override void ApplyBurn(float dmgPerSec, float duration)
    {
        base.ApplyBurn(dmgPerSec, duration);
    }

    public override void ApplySlow(float slowDownNumber, float duration)
    {
        base.ApplySlow(slowDownNumber, duration);
    }

    public override void KillEnemy()
    {
        Debug.Log("[Boss] Boss defeated!");

        OnBossDefeated(); // gọi lưu tiến trình
        base.KillEnemy();
    }

    private void OnBossDefeated()
    {
        GameProgress data = SaveLoadManager.Load();
        if (data == null) data = new GameProgress();

        data.currentLevel = Character.instance.Level;
        data.playerHealth = Character.instance._curHealth;
        data.bossDefeated = true;
        data.playerPosition = Character.instance.transform.position;

        SaveLoadManager.Save(data);
    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}
