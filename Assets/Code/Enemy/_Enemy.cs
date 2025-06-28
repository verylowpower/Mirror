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
    //[SerializeField] float smoothTime = 0.2f;
    //[SerializeField] int enemyExp;
    //[SerializeField] private GameObject expPrefab;
    public Rigidbody2D rb;

    //private BehaviorGraph behaviorGraph;
    Vector3 currentMovementDirection = Vector3.zero;
    Vector3 velocity = Vector3.zero;
    Vector3 targetDirection;



    public int spatialGroup = 0;
    [SerializeField] private SpriteRenderer spriteRender;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private Color slowColor = Color.blue;
    private Color originColor;
    [SerializeField] float health = 10;
    [SerializeField] int damage = 5;
    private bool isSlowed = false;
    public int Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    void Start()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
        if (spriteRender != null)
        {
            originColor = spriteRender.color;
        }

    }

    void FixedUpdate()
    {
        RunLightLogic();
    }

    public void RunHeavyLogic()
    {
        Vector3 targetPosition = GameController.instance.character.position;
        targetDirection = (targetPosition - transform.position).normalized;
    }

    public void RunLightLogic()
    {
        if (rb == null) return;

        int newSpatialGroup = GameController.instance.GetSpatialGroup(transform.position.x, transform.position.y);
        if (newSpatialGroup != spatialGroup)
        {
            // Add trước khi remove
            if (!GameController.instance.enemySpatialGroups.ContainsKey(newSpatialGroup))
            {
                GameController.instance.enemySpatialGroups[newSpatialGroup] = new();
            }

            GameController.instance.enemySpatialGroups[newSpatialGroup].Add(this);
            GameController.instance.enemySpatialGroups[spatialGroup].Remove(this);

            //Debug.Log($"Enemy {BatchID} moved to group {newSpatialGroup} at position {transform.position}");

            spatialGroup = newSpatialGroup;
        }

        Vector2 newPosition = rb.position + (Vector2)targetDirection * movementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
        PushEnemyNearby();
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
            if (distance < 2f)
            {
                Vector3 direction = transform.position - enemy.transform.position;
                direction.Normalize();
                enemy.transform.position -= 2f * movementSpeed * Time.deltaTime * direction;
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

    public void ChangeHealth(float amount)
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
        GameController.instance.RemoveFromSpatialGroup(spatialGroup, this);

        //drop exp
        // if (Random.Range(0f, 1f) < 0.5f)
        // {
        //Debug.Log("EXP DROP WHEN KILL WORKING");
        GameController.instance.DropExpPoint(transform.position, 1);

        //Instantiate(expPrefab, transform.position, Quaternion.identity);
        // }
        Destroy(gameObject);
    }

    public void ApplyBurn(float dmgPerSec, float duration)
    {
        StartCoroutine(DmgOverTime(dmgPerSec, duration));
    }

    public void ApplySlow(float slowDownNumber, float duration)
    {
        StartCoroutine(SlowSpeed(slowDownNumber, duration));
    }

    private IEnumerator DmgOverTime(float dmgPerSec, float duration)
    {
        float interval = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            ChangeHealth(dmgPerSec);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private IEnumerator SlowSpeed(float slowDownNumber, float duration)
    {
        if (isSlowed) yield break;

        isSlowed = true;
        movementSpeed -= slowDownNumber;
        StartCoroutine(FlashWhenHit(spriteRender, originColor, slowColor, duration));
        yield return new WaitForSeconds(duration);
        movementSpeed += slowDownNumber;
        isSlowed = false;
    }


    public IEnumerator FlashWhenHit(SpriteRenderer renderer, Color originColor, Color flashColor, float flashTime)
    {
        renderer.color = flashColor;
        yield return new WaitForSeconds(flashTime);
        renderer.color = originColor;
    }


}
