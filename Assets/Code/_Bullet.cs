using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class Bullet : MonoBehaviour
{
    public bool spinBullet;
    int spatialGroup = 0;
    
    Vector2 movementDirection = Vector2.zero;
    Vector2 MovementDirection
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

    bool isDestroy = false;
    void Start()
    {
        //get spatial group
        spatialGroup = GameController.instance.GetSpatialGroupStatic(transform.position.x, transform.position.y);
        OnBulletSpawn?.Invoke();
    }

    void FixedUpdate()
    {
        RunBulletLogic();
    }

    void RunBulletLogic()
    {
        transform.position += EnemyHelper.V2toV3(movementDirection) * Time.deltaTime * bulletSpeed;
        // caculate the angle to shoot bullet
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg - 90f;
        //set rotation to transfrom
        if (!spinBullet) modelTransfrom.rotation = Quaternion.Euler(0, 0, angle);
        else modelTransfrom.Rotate(0, 0, 10f);

        //update spatial group 
        int newSpatialGroup = GameController.instance.GetSpatialGroupStatic(transform.position.x, transform.position.y);
        if (newSpatialGroup != spatialGroup)
        {
            GameController.instance.bulletSpatialGroups[spatialGroup].Remove(this); //remove from old spatial group
            spatialGroup = newSpatialGroup;
            GameController.instance.bulletSpatialGroups[spatialGroup].Add(this); //add new to spatial group
            surroundingSpatialGroup = EnemyHelper.GetExpandedSpatialGroups(spatialGroup, movementDirection);
        }
        CheckCollisionWithEnemy();
    }

    public void OnceASecondLogic()
    {
        //if destroy -> skip
        if (this == null) return;

        //if out of bound -> destroy
        DestroyIfOutOfBound();
        OnBulletTravel?.Invoke(transform);
    }

    void CheckCollisionWithEnemy()
    {
        List<Enemy> surroundingEnemies = EnemyHelper.GetAllEnemySpatialGroups(surroundingSpatialGroup);

        foreach (Enemy enemy in surroundingEnemies)
        {
            if (enemy == null) continue;
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < bulletHitBoxRadius)
            {
                OnContactEnemy?.Invoke(transform);
                //enemy.ChangeHealth(-bulletDmg)
                DestroyBullet();

                break;
            }
        }
    }

    void DestroyIfOutOfBound()
    {
        if
        (
        transform.position.x < GameController.instance.MapHeightMin ||
        transform.position.x < GameController.instance.MapHeightMax ||
        transform.position.y < GameController.instance.MapHeightMin ||
        transform.position.y < GameController.instance.MapHeightMax ||
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
}
