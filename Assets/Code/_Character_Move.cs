using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character_Move : MonoBehaviour
{
    public bool shootRandom = false;
    Camera _Camera;
    //Shoot shoot;



    [Header("Stat")]
    [SerializeField] private float _speed = 3.0f;
    public int _health;
    public int _maxHealth;

    [Header("Experience")]
    long exp = 0;
    long expFromLastLevel = 0;
    long expToNextLevel = 0;
    int Level = 0;

    [Header("Spatial Group")]
    int spatialGroup = -1;
    public int SpatialGroup { get { return spatialGroup; } }

    [Header("Take DMG")] //take from every enemy
    [SerializeField] int takeDmgEveryXFrame = 0;
    [SerializeField] int takeDmgEveryXFrameCD = 10;
    [SerializeField] float hitBoxRadius = 0.1f;

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

    public static Character_Move instance;

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


    void Start()
    {
        spatialGroup = GameController.instance.GetSpatialGroup(transform.position.x, transform.position.y);
        _Camera = Camera.main;
        //LevelUp();
    }

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        instance = this;
        inputActions = new PlayerInputAct();
        inputActions.PlayerControl.Move.performed += OnMove;
        inputActions.PlayerControl.Move.canceled += OnMove;
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

        takeDmgEveryXFrame++;
        if (takeDmgEveryXFrame > takeDmgEveryXFrameCD)
        {
            CheckCollisionWithEnemy();
            takeDmgEveryXFrame = 0;
        }
    }

    void CameraRotate()
    {
        Vector2 newPositon = _rb.position + moveInput * _speed * Time.fixedDeltaTime;

        _rb.MovePosition(newPositon);

        //Debug.Log("Get camera: " + _Camera);
        Vector3 mousePosition = _Camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Đảm bảo không thay đổi trục Z

        // Tính toán góc quay từ nhân vật đến chuột
        Vector2 direction = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Gán góc quay cho nhân vật
        _rb.rotation = angle;
    }

    void CheckCollisionWithEnemy()
    {
        List<int> surroundingSpatialGroup = EnemyHelper.GetExpandedSpatialGroups(spatialGroup);
        List<Enemy> surroudingEnemy = EnemyHelper.GetAllEnemySpatialGroups(surroundingSpatialGroup);

        foreach (Enemy enemy in surroudingEnemy)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < hitBoxRadius)
            {
                //ModifyHealth(enemy.Dmg);

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
        spatialGroupToSearch = EnemyHelper.GetExpandedSpatialGroupsV2(spatialGroup, Mathf.CeilToInt(enemyDetectRadius));
        //get all enemy 
        List<Enemy> nearbyEnemy = EnemyHelper.GetAllEnemySpatialGroups(spatialGroupToSearch);
        Debug.Log("Enemy nearby: " + nearbyEnemy.Count);
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
            // if (enemy == null) continue;
            // float distance = Vector2.Distance(transform.position, enemy.transform.position);
            // if (distance < enemyDetectRadius)
            // {
            //     //maxClosestDistance = distance;
            //     closestPosition = enemy.transform.position;
            //     foundTarget = true;
            // }
        }
        if (foundTarget)
        {
            noEnemyNearby = false;
            nearestEnemy = closestPosition;
        }
        else
        {
            noEnemyNearby = false;
        }

        // else
        // {
        //     noEnemyNearby = false;

        //     foreach (Enemy enemy in nearbyEnemy)
        //     {
        //         if (enemy == null) continue;
        //         float distance = Vector2.Distance(transform.position, enemy.transform.position);
        //         // if (distance < minDistance)
        //         // {
        //         //     minDistance = distance;
        //         //     closestPosition = enemy.transform.position;
        //         //     foundTarget = true;
        //         // }
        //     }
        //     if (!foundTarget)
        //     {
        //         noEnemyNearby = true;
        //     }
        //     else
        //     {
        //         nearestEnemy = closestPosition;
        //     }
        // }
    }

    public void ModifyExp(int amount)
    {
        exp += amount;
        //EXP bar UI
        if (exp >= expToNextLevel) LevelUp();
    }

    public void LevelUp()
    {
        expFromLastLevel = expToNextLevel;
        expToNextLevel = EnemyHelper.GetExpRequired(Level) - expToNextLevel;
        Level++;
        //EXP bar UI
    }

    public void ModifyHealth(int amount)
    {
        _health += amount;
        //UI for health bar
        if (_health <= 0) KillPlayer();
    }

    public void KillPlayer()
    {
        Destroy(gameObject);
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
    }

}


