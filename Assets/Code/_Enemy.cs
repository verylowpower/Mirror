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


    public float movementSpeed = 7f;
    //private BehaviorGraph behaviorGraph;
    //Vector3 currentMovementDirection = Vector3.zero;
    public int spatialGroup = 0;

    int health = 10;
    int damage = 5;
    public int Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    // void Start()
    // {
    //     // Lấy biến Blackboard của GameObject
    //     var blackboard = behaviorGraph.BlackboardReference;

    //     // Khai báo biến BlackboardVariable để lưu trữ giá trị lấy từ Blackboard
    //     BlackboardVariable enemySpeed;

    //     if (blackboard.GetVariable("enemySpeed", out enemySpeed))
    //     {
    //         // Kiểm tra xem biến này có phải là BlackboardVariable<float> không
    //         if (enemySpeed is BlackboardVariable<float> speedVariable)
    //         {
    //             movementSpeed = speedVariable.value;  // Truy cập giá trị trong BlackboardVariable<float>
    //             Debug.Log("Enemy Speed from Behavior Graph: " + movementSpeed);
    //         }
    //         else
    //         {
    //             Debug.LogWarning("The 'enemySpeed' variable is not a float.");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("Variable 'enemySpeed' not found in Behavior Graph Blackboard.");
    //     }
    // }



    void FixedUpdate()
    {
        RunLogic();
    }

    void RunLogic()
    {
        PushEnemyNearby();
        int newSpatialGroup = GameController.instance.GetSpatialGroup(transform.position.x, transform.position.y); // GET spatial group
        if (newSpatialGroup != spatialGroup)
        {
            GameController.instance.enemySpatialGroups[spatialGroup].Remove(this); // REMOVE from old spatial group

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

    // public void ChangeHealth()
    // {

    // }

    // public void KillEnemy()
    // {

    // }


}
