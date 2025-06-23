using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class Bullet : MonoBehaviour
{
    public bool spinBullet;
    int spatialGroup = 0;
    public static Bullet instance;
    Vector2 movementDirection = Vector2.zero;
    public Vector2 MovementDirection
    {
        get { return movementDirection; }
        set { movementDirection = value; }
    }

    public float bulletDmg;
    public float bulletSpeed;
    public float bulletHitBoxRadius;

    public Transform modelTransfrom;

    List<int> surroundingSpatialGroup = new();

    //on spawn
    public delegate void BulletSpawnAction();
    public event BulletSpawnAction OnBulletSpawn;
    //on travel
    public delegate void BulletTravelAction(Transform parrentBullet);
    public event BulletTravelAction OnBulletTravel;
    //on destroy
    public delegate void BulletDestructionAction(Transform parrentBullet);
    public event BulletDestructionAction OnDestroy;
    //on contact with enemy
    public delegate void BulletContactAction(Transform parrentBullet);
    public event BulletContactAction OnContactEnemy;
    Enemy enemy;
    bool isDestroy = false;
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //get spatial group
        spatialGroup = GameController.instance.GetSpatialGroupStatic(transform.position.x, transform.position.y);
        if (GameController.instance.bulletSpatialGroups.ContainsKey(spatialGroup))
        {
            GameController.instance.bulletSpatialGroups[spatialGroup].Add(this);
        }
        else
        {
            Debug.LogWarning($"[Bullet] Spawned with invalid spatial group: {spatialGroup}, pos: {transform.position}");
        }

        // Debug.Log($"[Bullet] Spawned at {transform.position}, SpatialGroup: {spatialGroup}");
        // Debug.Log($"BulletMaxDistance: {GameController.instance.bulletMaxDistance}, Distance from player: {Vector2.Distance(transform.position, GameController.instance.character.position)}");
        // if (modelTransfrom == null)
        // {
        //     Debug.LogWarning("Bullet modelTransform is NULL! Auto-assigning transform.");
        //     modelTransfrom = transform;
        // }

        OnBulletSpawn?.Invoke();
    }

    void FixedUpdate()
    {
        RunLogic();
    }

    void RunLogic()
    {
        transform.position += Helper.V2toV3(movementDirection) * Time.deltaTime * bulletSpeed;
        // caculate the angle to shoot bullet

        if (!spinBullet && modelTransfrom != null)
        {
            float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
            modelTransfrom.rotation = Quaternion.Euler(0, 0, angle);
        }
        else if (modelTransfrom != null)
        {
            modelTransfrom.Rotate(0, 0, 10f); // chỉ dùng khi muốn xoay liên tục như viên đạn lốc xoáy
        }

        if (modelTransfrom == transform)
        {
            Debug.LogWarning("modelTransfrom is root transform, consider using a child!");
        }

        //update spatial group 
        int newSpatialGroup = GameController.instance.GetSpatialGroupStatic(transform.position.x, transform.position.y);

        if (newSpatialGroup < 0)
        {
            Debug.LogWarning($"[Bullet] Invalid spatial group: {newSpatialGroup} at position {transform.position}");
            DestroyBullet(); // hoặc xử lý hợp lý, tránh gây lỗi
            return;
        }

        if (newSpatialGroup != spatialGroup)
        {
            if (GameController.instance.bulletSpatialGroups.ContainsKey(spatialGroup))
            {
                GameController.instance.bulletSpatialGroups[spatialGroup].Remove(this);
            }

            spatialGroup = newSpatialGroup;

            if (!GameController.instance.bulletSpatialGroups.ContainsKey(spatialGroup))
            {
                GameController.instance.bulletSpatialGroups[spatialGroup] = new HashSet<Bullet>();
            }

            GameController.instance.bulletSpatialGroups[spatialGroup].Add(this);
            surroundingSpatialGroup = Helper.GetExpandedSpatialGroups(spatialGroup, movementDirection);
        }

        //CheckCollisionWithEnemy();

    }

    public void OnceASecondLogic()
    {
        //if destroy -> skip
        if (this == null) return;

        //if out of bound -> destroy
        DestroyIfOutOfBound();
        OnBulletTravel?.Invoke(transform);
    }

    // void CheckCollisionWithEnemy()
    // {
    //     List<Enemy> surroundingEnemies = EnemyHelper.GetAllEnemySpatialGroups(surroundingSpatialGroup);
    //     float closestDistance = float.MaxValue;
    //     Enemy closestEnemy = null;

    //     foreach (Enemy enemy in surroundingEnemies)
    //     {
    //         if (enemy == null) continue;
    //         float distance = Vector2.Distance(transform.position, enemy.transform.position);
    //         if (distance < bulletHitBoxRadius && distance < closestDistance)
    //         {
    //             closestDistance = distance;
    //             closestEnemy = enemy;
    //         }
    //     }
    //     if (closestEnemy != null)
    //     {
    //         OnContactEnemy?.Invoke(transform);
    //         closestEnemy.ChangeHealth(bulletDmg);
    //         DestroyBullet();
    //     }
    // }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("enemy")) // đảm bảo enemy có tag là "enemy"
        {
            if (collision.TryGetComponent<Enemy>(out var enemy))
            {
                OnContactEnemy?.Invoke(transform);
                enemy.ChangeHealth(bulletDmg);
                DestroyBullet(); // Gây sát thương và huỷ đạn
            }
        }
    }

    // void OnCollisonEnter2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("enemy")) // đảm bảo enemy có tag là "enemy"
    //     {
    //         if (collision.TryGetComponent<Enemy>(out var enemy))
    //         {
    //             OnContactEnemy?.Invoke(transform);
    //             enemy.ChangeHealth(bulletDmg);
    //             DestroyBullet(); // Gây sát thương và huỷ đạn
    //         }
    //     }
    // }



    void DestroyIfOutOfBound()
    {
        Vector2 pos = transform.position;
        Vector2 playerPos = GameController.instance.character.position;

        bool outOfMap =
        pos.x < GameController.instance.MapWidthMin ||
        pos.x > GameController.instance.MapWidthMax ||
        pos.y < GameController.instance.MapHeightMin ||
        pos.y > GameController.instance.MapHeightMax;
        bool tooFarFromPlayer = Vector2.Distance(pos, playerPos) > GameController.instance.bulletMaxDistance;

        if (outOfMap || tooFarFromPlayer)
        {
            DestroyBullet();
        }
    }

    public void DestroyBullet()
    {
        if (isDestroy) return;

        //trigger when bullet destroy
        OnDestroy?.Invoke(transform);

        GameController.instance.bulletSpatialGroups[spatialGroup].Remove(this);
        Destroy(gameObject);
        isDestroy = true;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, bulletHitBoxRadius);
    }

}
