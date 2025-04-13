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
    //private BehaviorGraph behaviorGraph;
    Vector3 currentMovementDirection = Vector3.zero;
    Vector3 velocity = Vector3.zero;
    Vector3 targetDirection;
    [SerializeField] float smoothTime = 0.2f;
    public int spatialGroup = 0;

    int health = 10;
    int damage = 5;
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
        transform.position = Vector3.MoveTowards(transform.position, currentMovementDirection, movementSpeed * Time.deltaTime);

        PushEnemyNearby();
        int newSpatialGroup = GameController.instance.GetSpatitalGroup(transform.position.x, transform.position.y); // GET spatial group
        if (newSpatialGroup != spatialGroup)
        {
            GameController.instance.enemySpatitalGroups[spatialGroup].Remove(this); // REMOVE from old spatial group

            spatialGroup = newSpatialGroup; // UPDATE current spatial group
            GameController.instance.enemySpatitalGroups[spatialGroup].Add(this); // ADD to new spatial group
        }
    }

    private void PushEnemyNearby()
    {
        List<Enemy> currAreaEnemy = GameController.instance.enemySpatitalGroups[spatialGroup].ToList();

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

    // public void ChangeHealth()
    // {

    // }

    // public void KillEnemy()
    // {

    // }


}
