using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class _Character_Move : MonoBehaviour
{
    public bool shootRandom = false;

    [Header("Stat")]
    [SerializeField] private float _speed = 3.0f;
    public int _health;
    public int _maxHealth;

    [Header("Spatital Group")]
    int spatialGroup = -1;
    public int SpatitalGroup { get { return spatialGroup; } }

    [Header("Take DMG")] //take from every enemy
    int takeDmgEveryXFrame = 0;
    int takeDmgEveryXFrameCD = 10;
    float hitBoxRadius = 0.1f;

    //check nearest enemy for gun
    Vector2 nearestEnemy = Vector2.zero;
    public Vector2 NearestEnemy
    {
        get { return nearestEnemy; }
        set { nearestEnemy = value; }
    }

    bool noEnemyNearby = false;
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
        spatialGroup = GameController.instance.GetSpatitalGroup(transform.position.x, transform.position.y);
        //LevelUp();
    }

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
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
        Vector2 newPositon = _rb.position + moveInput * _speed * Time.fixedDeltaTime;

        _rb.MovePosition(newPositon);

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Đảm bảo không thay đổi trục Z

        // Tính toán góc quay từ nhân vật đến chuột
        Vector2 direction = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Gán góc quay cho nhân vật
        _rb.rotation = angle;


        //spatialGroup = GameController.instance.GetSpatitalGroup(transform.position.x, transform.position.y);


    }


    void CheckCollisionWithEnemy()
    {
        List<int> surroundingSpatitalGroup = EnemyHelper.GetExpandedSpatialGroups(spatialGroup);
        List<Enemy> surroudingEnemy = EnemyHelper.GetAllEnemySpatialGroups(surroundingSpatitalGroup);

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
        float minDistance = 100f;
        Vector2 closestPosition = Vector2.zero;

        List<int> spatitalGroupToSearch = new List<int>() { spatialGroup };


    }

    public void ModifyExp()
    {

    }

    public void LevelUp()
    {

    }

    public void ModifyHealth()
    {

    }

    public void KillPlayer()
    {

    }
}
