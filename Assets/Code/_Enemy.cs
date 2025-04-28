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
    public static Enemy instance;
    [SerializeField] float movementSpeed = 7f;
    [SerializeField] float smoothTime = 0.2f;
    //private BehaviorGraph behaviorGraph;
    Vector3 currentMovementDirection = Vector3.zero;
    Vector3 velocity = Vector3.zero;
    Vector3 targetDirection;

    public int spatialGroup = 0;
    [SerializeField] private SpriteRenderer spriteRender;
    [SerializeField] private Color flashColor = Color.white;
    private Color originColor;
    [SerializeField] int health = 10;
    [SerializeField] int damage = 5;
    public int Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    void Start()
    {   
        instance = this;
        if (spriteRender != null)
        {
            originColor = spriteRender.color;
        }
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

        Vector3 targetPosition = GameController.instance.character.position;
        currentMovementDirection = (targetPosition - transform.position).normalized;

        transform.position = Vector3.SmoothDamp(transform.position, transform.position + currentMovementDirection, ref velocity, smoothTime);

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
                enemy.transform.position -= 1.5f * movementSpeed * Time.deltaTime * direction;
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
        //int maxHealth = health;
        health -= amount;
        //Debug.Log("dmg taken" + amount);

        if (amount > 0 && spriteRender != null)
        {
            if (spriteRender != null)
            {
                StartCoroutine(FlashWhenHit(spriteRender, originColor, flashColor, 0.1f));
            }
            //maxHealth = health;
        }
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


    public IEnumerator FlashWhenHit(SpriteRenderer renderer, Color originColor, Color flashColor, float flashTime)
    {
        renderer.color = flashColor;
        yield return new WaitForSeconds(flashTime);
        renderer.color = originColor;
    }



}
