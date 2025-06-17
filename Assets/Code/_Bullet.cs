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

    [SerializeField] public float bulletDmg;
    [SerializeField] public float bulletSpeed;
    [SerializeField] public float bulletHitBoxRadius;

    [SerializeField] public Transform modelTransfrom;

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
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg - 90f;
        //set rotation to transfrom
        if (!spinBullet) modelTransfrom.rotation = Quaternion.Euler(0, 0, angle);
        else modelTransfrom.Rotate(0, 0, 10f);

        //update spatial group 
        int newSpatialGroup = GameController.instance.GetSpatialGroupStatic(transform.position.x, transform.position.y);
        if (newSpatialGroup != spatialGroup)
        {
            // Safely remove from the old group
            if (GameController.instance.bulletSpatialGroups.ContainsKey(spatialGroup))
            {
                GameController.instance.bulletSpatialGroups[spatialGroup].Remove(this);
            }

            spatialGroup = newSpatialGroup;

            // Safely add to the new group
            if (GameController.instance.bulletSpatialGroups.ContainsKey(spatialGroup))
            {
                GameController.instance.bulletSpatialGroups[spatialGroup].Add(this);
                surroundingSpatialGroup = Helper.GetExpandedSpatialGroups(spatialGroup, movementDirection);
            }
            else
            {
                Debug.LogWarning($"[Bullet] Invalid spatial group: {spatialGroup} at position {transform.position}");
            }
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

    void OnTriggerEnter2D(Collider2D collision)
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
        if
        (
        transform.position.x < GameController.instance.MapHeightMin ||
        transform.position.x > GameController.instance.MapHeightMax ||
        transform.position.y < GameController.instance.MapHeightMin ||
        transform.position.y > GameController.instance.MapHeightMax ||
        Vector2.Distance(transform.position, GameController.instance.character.position) > 20f
        )
        {
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        if (isDestroy) return;

        //trigger when bullet destroy
        OnDestroy?.Invoke(transform);

        //Weaponary. 

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
