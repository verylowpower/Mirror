using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    int batchId;
    public int BatchID
    {
        get { return batchId; }
        set { batchId = value; }
    }

    [SerializeField] float movementSpeed = 7f;
    [SerializeField] float smoothTime = 0.2f;
    //private BehaviorGraph behaviorGraph;
    Vector3 currentMovementDirection = Vector3.zero;
    Vector3 velocity = Vector3.zero;
    Vector3 targetDirection;

    public int spatialGroup = 0;

    [SerializeField] int health = 10;
    [SerializeField] int damage = 5;
    public int Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    void Update()
    {
        //RunLogic();
    }

    public void RunLogic()
    {
        // currentMovementDirection = GameController.instance.character.position - transform.position;
        // currentMovementDirection.Normalize();

        // transform.position += currentMovementDirection * Time.deltaTime * movementSpeed;

        // currentMovementDirection = GameController.instance.character.position - transform.position;
        // currentMovementDirection.Normalize();

        // targetDirection = transform.position + currentMovementDirection;
        // transform.position = Vector3.Lerp(transform.position, transform.position + currentMovementDirection, Time.deltaTime * movementSpeed);

        Vector3 currentMovementDirection = GameController.instance.character.position;
        transform.position = Vector3.MoveTowards(transform.position, currentMovementDirection, movementSpeed * smoothTime * Time.deltaTime);

        PushEnemyNearby();
        int newSpatialGroup = GameController.instance.GetSpatialGroup(transform.position.x, transform.position.y); // GET spatial group
        if (newSpatialGroup != spatialGroup)
        {
            GameController.instance.enemySpatialGroups[spatialGroup].Remove(this); // REMOVE from old spatial group
            //GameController.instance.RemoveFromSpatialGroup(spatialGroup, this);

            spatialGroup = newSpatialGroup; // UPDATE current spatial group
            GameController.instance.enemySpatialGroups[spatialGroup].Add(this); // ADD to new spatial group
        }
    }

    private void PushEnemyNearby()
    {
        List<Enemy> currAreaEnemy = GameController.instance.enemySpatialGroups[spatialGroup].ToList();

        foreach (Enemy enemy in currAreaEnemy)
        {
            if (enemy == null) continue;
            if (enemy == this) continue;

            float distance = Mathf.Abs(transform.position.x - enemy.transform.position.x) +
            Mathf.Abs(transform.position.y - enemy.transform.position.y);
            if (distance < 0.2f)
            {
                Vector3 direction = transform.position - enemy.transform.position;
                direction.Normalize();
                enemy.transform.position -= 5 * movementSpeed * Time.deltaTime * direction;
            }

        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "enemy")
        {
            Vector3 direction = transform.position - collision.transform.position;
            direction.Normalize();
            collision.transform.position += movementSpeed * Time.deltaTime * direction;
        }
    }

    public void ChangeHealth(int amount)
    {
        health += amount;
        if (health <= 0)
        {
            KillEnemy();
        }
    }

    public void KillEnemy()
    {
        GameController.instance.UpdateEnemyOnUnitDeath("enemy", batchId);
        GameController.instance.enemySpatialGroups[spatialGroup].Remove(this);
        //GameController.instance.RemoveFromSpatialGroup(spatialGroup, this);

        //drop exp
        // if(Random.Range(1,50)<25)
        // {
        //     GameController.instance.Drop
        // }

        Destroy(gameObject);
    }


}
