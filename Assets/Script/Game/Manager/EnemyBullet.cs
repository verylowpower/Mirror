using UnityEngine;

public class EnemyBullet : Bullet
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi đạn enemy chạm player
        if (collision.CompareTag("player"))
        {
            Debug.Log("[EnemyBullet] Hit player!");

            Character.instance.ModifyHealth((int)bulletDmg);

            InvokeOnContactEnemy(transform);

            // Hủy đạn sau khi chạm
            DestroyBullet();
        }
    }
}
