using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


public class Character : MonoBehaviour
{
    //public bool shootRandom = false;
    public static Character instance;
    Camera _Camera;
    //Shoot shoot;
    //    public float bulletDistance = GameController.instance.bulletMaxDistance;

    [Header("Stat")]
    public float _baseSpeed = 3.0f;
    public float _speedMultiplier = 1f;
    public float Speed => _baseSpeed * _speedMultiplier;
    public int _health;
    public int _curHealth;
    public float baseCollectRadious;
    public float collectRadiousMultiplier = 1f;
    public float CollectRadious => baseCollectRadious * collectRadiousMultiplier;

    [Header("GUI")]
    [SerializeField] Slider healthBar;
    [SerializeField] Slider expBar;
    [SerializeField] GameObject deathScreen;
    //public int _maxHealth;

    [Header("Experience")]
    public long exp = 0;
    long expFromLastLevel = 0;
    public long expToNextLevel;
    public int Level = 1;
    public bool buffUIActive = false;


    public delegate void LevelChanged();
    public event LevelChanged OnLevelChanged;

    [Header("Spatial Group")]
    int spatialGroup = -1;
    public int SpatialGroup { get { return spatialGroup; } }

    [Header("Take DMG")] //take from every enemy
    // [SerializeField] int takeDmgEveryXFrame = 0;
    // [SerializeField] int takeDmgEveryXFrameCD = 1;
    [SerializeField] float hurtBoxRadius = 0.1f;
    [SerializeField] float iFrame;
    [SerializeField] float iFrameCD;
    public bool isIFrame;
    public bool isKilled = false;

    [Header("Shoot")]

    public GameObject bulletHolder;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public float nextFireTime = 0f;
    public float baseBulletSpeed = 12f;       // default
    public float bulletSpeedMultiplier = 1f;  // modified by buffs
    public float CurrentBulletSpeed => baseBulletSpeed * bulletSpeedMultiplier;

    public float baseBulletDmg = 5f;
    public float bulletDmgMultiplier = 1f;  // modified by buffs
    public float CurrentBulletDmg => baseBulletDmg * bulletDmgMultiplier;

    [Header("Bullet Buff")]
    public int bulletPerShoot = 1;
    public int bulletPerShootSpread = 1;
    public float multiShootDelayTime = 0.1f;
    public float spreadAngle = 15f;

    public bool isFireBulletOn = false;
    public float burnDmg;
    public float burnTime;

    [Header("Ice Spell")]
    public bool isIceSpellOn = false;
    public float iceSlowNumber;
    public float iceSlowTime;

    [Header("Auto shoot")]
    public bool isAutoShootOn = false;
    public float autoShootInterval;
    public float autoShootTime;
    public string autoBulletType;



    [Header("Detect Enemy")]
    //check nearest enemy for gun
    float enemySearchTimer = 0f;
    float searchInterval = 0.2f;

    [SerializeField] private float enemyDetectRadius = 1f;
    [SerializeField] private float maxClosestDistance;
    Vector2 nearestEnemy = Vector2.zero;


    public Vector2 NearestEnemy
    {
        get { return nearestEnemy; }
        set { nearestEnemy = value; }
    }

    public bool noEnemyNearby = false;
    public bool foundTarget = false;
    public bool NoEnemyNearby
    {
        get { return noEnemyNearby; }
        set { noEnemyNearby = value; }
    }


    private PlayerInputAct inputActions;
    private Rigidbody2D _rb;
    private Vector2 moveInput;
    public HashSet<string> unlockedBuffs = new();

    [Header("Point")]
    //public int enemyKilled;

    public SpriteRenderer spriteRender;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashTime = 0f;
    private Color originColor;


    void Start()
    {
        spatialGroup = GameController.instance.GetSpatialGroup(transform.position.x, transform.position.y);
        _Camera = Camera.main;
        _curHealth = _health;
        UpdateGUIforHealthBar(_curHealth, _health);
        expToNextLevel = Helper.GetExpRequired(Level);
        //Debug.Log("Sprite is:" + spriteRender);
        if (spriteRender != null)
        {
            originColor = spriteRender.color;
        }
        //LevelUp();
    }

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        instance = this;
        inputActions = new PlayerInputAct();
        inputActions.PlayerControl.Move.performed += OnMove;
        inputActions.PlayerControl.Move.canceled += OnMove;
        _Camera = Camera.main;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void FixedUpdate()
    {

        spatialGroup = GameController.instance.GetSpatialGroup(transform.position.x, transform.position.y);

        enemySearchTimer += Time.fixedDeltaTime;
        if (enemySearchTimer >= searchInterval)
        {
            CheckNearestEnemyDirection();
            enemySearchTimer = 0f;
        }

        if (!noEnemyNearby && nearestEnemy != Vector2.zero && Time.time >= nextFireTime)
        {
            Vector2 directionToEnemy = nearestEnemy - (Vector2)firePoint.position;
            if (directionToEnemy.sqrMagnitude > 0.001f)
            {
                directionToEnemy.Normalize();
                ShootBullet(directionToEnemy);
                nextFireTime = Time.time + fireRate;
            }
        }

        if (isAutoShootOn)
        {
            autoShootTime += Time.fixedDeltaTime;
            if (autoShootTime >= autoShootInterval)
            {
                autoShootTime = 0f;

                Vector2 shootDir;

                if (!noEnemyNearby && nearestEnemy != Vector2.zero)
                {
                    shootDir = (nearestEnemy - (Vector2)firePoint.position).normalized;
                }
                else
                {
                    shootDir = Vector2.right; // hoặc Vector2.down, Vector2.up...
                }

                ShootAutoBullet(shootDir);
            }
        }


        if (isIFrame)
        {
            iFrameCD -= Time.deltaTime;
            if (iFrameCD <= 0f)
            {
                isIFrame = false;
            }
        }
        else
        {
            CheckCollisionWithEnemy();
        }

        // CheckCollisionWithEnemy();
        // takeDmgEveryXFrame++;
        // if (takeDmgEveryXFrame > takeDmgEveryXFrameCD)
        // {
        //     CheckCollisionWithEnemy();
        //     takeDmgEveryXFrame = 0;
        // }
        // _rb.MovePosition(_rb.position + moveInput * _speed * Time.fixedDeltaTime);
        _rb.linearVelocity = moveInput * Speed;

    }

    // void CameraRotate()
    // {
    //     Vector3 mouseScreenPos = Input.mousePosition;
    //     mouseScreenPos.z = 12f; // Đẩy chuột ra mặt phẳng Z = 0

    //     Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

    //     Vector2 direction = mouseWorldPos - transform.position;
    //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    //     transform.rotation = Quaternion.Euler(0, 0, angle); // hoặc angle tùy sprite
    //     //Debug.Log("Angle: " + angle);
    // }



    public void CheckCollisionWithEnemy()
    {
        //Debug.Log("Calling CheckCollisionWithEnemy"); 
        List<int> surroundingSpatialGroup = Helper.GetExpandedSpatialGroups(spatialGroup);
        List<Enemy> surroudingEnemy = Helper.GetAllEnemySpatialGroups(surroundingSpatialGroup);
        //Debug.Log("Total enemies nearby: " + surroudingEnemy.Count);
        foreach (Enemy enemy in surroudingEnemy)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < hurtBoxRadius)
            {
                //Debug.Log("Hallo");
                // Take damage
                ModifyHealth(enemy.Damage);

                break;
            }
        }
    }

    void CheckNearestEnemyDirection()
    {
        Vector2 closestPosition = Vector2.zero;
        maxClosestDistance = 10f;
        bool foundTarget = false;

        List<int> spatialGroupToSearch = new List<int>() { spatialGroup };
        spatialGroupToSearch = Helper.GetExpandedSpatialGroupsV2(spatialGroup, Mathf.CeilToInt(enemyDetectRadius));
        //get all enemy 
        List<Enemy> nearbyEnemy = Helper.GetAllEnemySpatialGroups(spatialGroupToSearch);
        //Debug.Log("Enemy nearby: " + nearbyEnemy.Count);

        if (nearbyEnemy.Count == 0)
        {
            noEnemyNearby = true;
            return;
        }

        foreach (Enemy enemy in nearbyEnemy)
        {
            if (enemy == null) continue;
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < maxClosestDistance)
            {
                maxClosestDistance = distance;
                closestPosition = enemy.transform.position;
                foundTarget = true;
            }
        }

        noEnemyNearby = !foundTarget;
        if (foundTarget)
            nearestEnemy = closestPosition;

    }

    public void ModifyExp(int amount)
    {
        exp += amount;

        //Debug.Log($"Gained {amount} EXP. Total EXP: {exp}");

        //Debug.Log("exp from last level" + expFromLastLevel);
        // Nếu đủ exp thì lên cấp
        if (exp >= expToNextLevel)
        {
            LevelUp();

            OnLevelChanged?.Invoke();
        }
        UpdateGUIforExpBar(exp, expToNextLevel);
    }

    public void LevelUp()
    {
        Level++;

        expFromLastLevel = expToNextLevel;
        expToNextLevel = Helper.GetExpRequired(Level);
        OnLevelChanged?.Invoke();
        // Debug.Log($"Level Up! New Level: {Level}");
        if (!buffUIActive)
        {
            RandomSystem.instance.RandomBuff();
            buffUIActive = true;
        }
        //Debug.Log("Health is: " + _health);
        //Debug.Log("Bullet Speed is: " + CurrentBulletSpeed);
    }

    public void ModifyHealth(int amount)
    {
        if (isIFrame) return;

        _curHealth = Mathf.Clamp(_curHealth - amount, 0, _health);
        //Debug.Log("Player get dmg: " + amount);
        UpdateGUIforHealthBar(_curHealth, _health);
        if (_curHealth <= 0) KillPlayer();

        isIFrame = true;
        iFrameCD = iFrame;

        PlayerFlash();
    }

    void UpdateGUIforHealthBar(float curValue, float maxValue)
    {
        if (healthBar != null)
        {
            healthBar.value = curValue / maxValue;
        }
    }

    void UpdateGUIforExpBar(float curValue, float maxValue)
    {
        if (expBar != null)
        {
            expBar.value = (curValue - expFromLastLevel) / (maxValue - expFromLastLevel);

        }
    }

    //store unlocked buff for UI (not done)
    public static bool IsUnlocked(string buffID)
    {
        return instance.unlockedBuffs.Contains(buffID);
    }

    public void KillPlayer()
    {
        isKilled = true;
        DeathHandler.instance.HandlePlayerDeath();
        Destroy(gameObject);
    }


    private IEnumerator FlashWhenHit(SpriteRenderer renderer, Color original, Color flash, float duration)
    {
        renderer.color = flash;
        yield return new WaitForSeconds(duration);
        renderer.color = original;
    }

    void PlayerFlash()
    {
        StartCoroutine(FlashWhenHit(spriteRender, originColor, flashColor, flashTime));
    }

    //shoot logic
    void ShootBullet(Vector2 direction)
    {

        StartCoroutine(ShootCombined(direction));
    }

    IEnumerator ShootCombined(Vector2 direction)
    {
        for (int seq = 0; seq < bulletPerShoot; seq++)
        {
            // Tính toán và bắn các viên xoè (spread)
            int spreadCount = bulletPerShootSpread;
            float totalSpreadAngle = spreadAngle;
            float angleStep = (spreadCount > 1) ? totalSpreadAngle / (spreadCount - 1) : 0f;
            float startAngle = -totalSpreadAngle / 2f;

            for (int i = 0; i < spreadCount; i++)
            {
                float currentAngle = startAngle + i * angleStep;
                Vector2 spreadDirection = Quaternion.Euler(0, 0, currentAngle) * direction;
                ShootSingleBullet(spreadDirection.normalized);
            }

            if (seq < bulletPerShoot - 1)
            {
                yield return new WaitForSeconds(multiShootDelayTime);
            }
        }
    }

    void ShootSingleBullet(Vector2 direction)
    {
        if (isFireBulletOn)
        {
            ShootFireBullet(direction);
        }
        else
        {
            ShootNormalBullet(direction);
        }
    }

    void ShootNormalBullet(Vector2 direction)
    {

        GameObject bulletGO = FactoryPattern.BulletFactory.CreateBullet
        (
        "normal",
        firePoint.position,
        Quaternion.identity,
        bulletHolder.transform
        );


        Bullet bullet = bulletGO.GetComponent<Bullet>();

        bullet.MovementDirection = direction;

        bullet.bulletSpeed = CurrentBulletSpeed;
        bullet.bulletDmg = CurrentBulletDmg;

        // Debug.Log("Current bullet speed: " + CurrentBulletSpeed);
        // Debug.Log("Current bullet dmg: " + CurrentBulletDmg);

        RegisterBulletToSpatialGroup(bullet);
    }

    void ShootFireBullet(Vector2 direction)
    {
        GameObject bulletGO = FactoryPattern.BulletFactory.CreateBullet
        (
        "fire",
        firePoint.position,
        Quaternion.identity,
        bulletHolder.transform
        );

        FireBullet fireBullet = bulletGO.GetComponent<FireBullet>();

        fireBullet.MovementDirection = direction;

        fireBullet.burnDmg = burnDmg;

        fireBullet.burnTime = burnTime;

        RegisterBulletToSpatialGroup(fireBullet);
    }


    void ShootAutoBullet(Vector2 direction)
    {
        switch (autoBulletType)
        {
            case "ice":
                ShootIceSpell(direction);
                break;
            case "fire":
                ShootFireBullet(direction);
                break;
            default:
                ShootNormalBullet(direction);
                break;
        }
    }


    void ShootIceSpell(Vector2 direction)
    {
        GameObject bulletGO = FactoryPattern.BulletFactory.CreateBullet
        (
        "ice",
        firePoint.position,
        Quaternion.identity,
        bulletHolder.transform
        );

        IceSpell iceSpell = bulletGO.GetComponent<IceSpell>();

        iceSpell.MovementDirection = direction;

        iceSpell.slowDownNumber = iceSlowNumber;

        iceSpell.slowDuration = iceSlowTime;

        RegisterBulletToSpatialGroup(iceSpell);
    }


    void RegisterBulletToSpatialGroup(Bullet bullet)
    {
        int group = GameController.instance.GetSpatialGroupStatic(bullet.transform.position.x, bullet.transform.position.y);
        if (!GameController.instance.bulletSpatialGroups.ContainsKey(group))
        {
            GameController.instance.bulletSpatialGroups.Add(group, new HashSet<Bullet>()); // Initialize with HashSet
        }
        GameController.instance.bulletSpatialGroups[group].Add(bullet); // Add to HashSet
    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyDetectRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxClosestDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hurtBoxRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, CollectRadious);

        if (GameController.instance != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, GameController.instance.bulletMaxDistance);
        }
        // else
        // {
        //    // Debug.LogWarning("Don't found gamecontroller");
        // }
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireSphere(transform.position, bulletDistance);


    }

}


