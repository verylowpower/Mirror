using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    int batchId;
    public int BatchID
    {
        get { return batchId; }
        set { batchId = value; }
    }

    public float movementSpeed = 3f;
    Vector3 currentMovementDirection = Vector3.zero;
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
        // Removed RunLogic since behavior and NavMesh control movement now
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
        GameController.instance.UpdateBatchOnUnitDeath("enemy", batchId);
        GameController.instance.enemySpatialGroups[spatialGroup].Remove(this);

        // Drop experience points with a chance
        if (Random.Range(0, 100) < 25)
            GameController.instance.DropExperiencePoint(transform.position, 1);

        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            // Push this enemy away (if collided with another enemy)
            Vector3 direction = transform.position - collision.transform.position;
            direction.Normalize();
            collision.transform.position += direction * Time.deltaTime * movementSpeed;
        }
    }

    void PushNearbyEnemies()
    {
        List<Enemy> currAreaEnemies = GameController.instance.enemySpatialGroups[spatialGroup].ToList(); // Enemies in the same spatial group

        // Check each enemy and push if too close
        foreach (Enemy enemy in currAreaEnemies)
        {
            if (enemy == null || enemy == this) continue;

            float distance = Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y);
            if (distance < 0.2f)
            {
                // Push this enemy away
                Vector3 direction = transform.position - enemy.transform.position;
                direction.Normalize();
                enemy.transform.position -= direction * Time.deltaTime * movementSpeed * 5;
            }
        }
    }
}
