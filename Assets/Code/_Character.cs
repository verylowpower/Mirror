using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;


public class Character : MonoBehaviour
{
    //public bool shootRandom = false;
    Camera _Camera;
    //Shoot shoot;

    [Header("Stat")]
    [SerializeField] private float _speed = 3.0f;
    public int _health;
    public int _curHealth;
    public float collectRadious;

    [Header("GUI")]
    [SerializeField] Slider healthBar;
    [SerializeField] Slider expBar;
    //public int _maxHealth;

    [Header("Experience")]
    public long exp = 0;
    long expFromLastLevel = 0;
    public long expToNextLevel;
    public int Level = 0;

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


    [Header("Shoot")]
    [SerializeField] public GameObject bulletPF;
    [SerializeField] public GameObject bulletHolder;
    [SerializeField] public Transform firePoint;
    [SerializeField] public float fireRate = 0.2f;
    [SerializeField] public float nextFireTime = 0f;

    [Header("Detect Enemy")]
    //check nearest enemy for gun
    [SerializeField] private float enemyDetectRadius = 1f;
    [SerializeField] private float maxClosestDistance;
    Vector2 nearestEnemy = Vector2.zero;

    public static Character instance;

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


    [SerializeField] private SpriteRenderer spriteRender;
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
        CameraRotate();
        spatialGroup = GameController.instance.GetSpatialGroup(transform.position.x, transform.position.y);
        CheckNearestEnemyDirection();
        if (!noEnemyNearby && Time.time >= nextFireTime)
        {
            Vector2 directionToEnemy = nearestEnemy - (Vector2)firePoint.position;
            directionToEnemy.Normalize();

            ShootBullet(directionToEnemy);
            nextFireTime = Time.time + fireRate;
        }

        if (isIFrame == true)
        {
            iFrameCD -= Time.deltaTime;
            if (iFrameCD <= 0)
                isIFrame = false;
        }
        else
        {
            CheckCollisionWithEnemy();
        }

        // takeDmgEveryXFrame++;
        // if (takeDmgEveryXFrame > takeDmgEveryXFrameCD)
        // {
        //     CheckCollisionWithEnemy();
        //     takeDmgEveryXFrame = 0;
        // }
        _rb.MovePosition(_rb.position + moveInput * _speed * Time.fixedDeltaTime);
    }

    void CameraRotate()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 12f; // Đẩy chuột ra mặt phẳng Z = 0

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        Vector2 direction = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle); // hoặc angle tùy sprite
        //Debug.Log("Angle: " + angle);
    }



    public void CheckCollisionWithEnemy()
    {
        List<int> surroundingSpatialGroup = Helper.GetExpandedSpatialGroups(spatialGroup);
        List<Enemy> surroudingEnemy = Helper.GetAllEnemySpatialGroups(surroundingSpatialGroup);

        foreach (Enemy enemy in surroudingEnemy)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < hurtBoxRadius && spriteRender != null)
            {
                ModifyHealth(enemy.Damage);

                //PushPlayer();
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

        Debug.Log($"Gained {amount} EXP. Total EXP: {exp}");
        UpdateGUIforExpBar(exp, expToNextLevel);
        // Nếu đủ exp thì lên cấp
        if (exp >= expToNextLevel)
        {
            LevelUp();
            OnLevelChanged?.Invoke();
        }
    }

    public void LevelUp()
    {
        Level++;
        expFromLastLevel = expToNextLevel;
        expToNextLevel = Helper.GetExpRequired(Level);

        Debug.Log($"Level Up! New Level: {Level}");
        //EXP bar UI
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
            expBar.value = curValue / maxValue;
        }
    }

    public void KillPlayer()
    {
        Destroy(gameObject);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator FlashWhenHit(SpriteRenderer renderer, Color original, Color flash, float duration)
    {
        renderer.color = flash;
        yield return new WaitForSeconds(duration);
        renderer.color = original;
    }

    void PlayerFlash()
    {
        StartCoroutine(FlashWhenHit(spriteRender, originColor, flashColor, flashTime));
    }

    void ShootBullet(Vector2 direction)
    {
        GameObject bulletGO = Instantiate(bulletPF, firePoint.position, Quaternion.identity, bulletHolder.transform);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.MovementDirection = direction;

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
        Gizmos.DrawWireSphere(transform.position, collectRadious);
    }

}


