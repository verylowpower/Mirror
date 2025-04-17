using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] GameObject bulletPF;
    [SerializeField] GameObject bulletHolder;
    [SerializeField] Transform firePoint;
    [SerializeField] float fireRate = 0.2f;

    float nextFireTime = 0f;
    Bullet bullet;

    public void FixedUpdate()
    {
        // Kiểm tra có quái gần không, và cooldown đã xong chưa
        if (!Character_Move.instance.noEnemyNearby && Time.time >= nextFireTime)
        {
            Vector2 directionToEnemy = Character_Move.instance.NearestEnemy - (Vector2)firePoint.position;
            directionToEnemy.Normalize();

            PiuPiu(directionToEnemy);
            nextFireTime = Time.time + fireRate;
        }
    }

    void PiuPiu(Vector2 direction)
    {
        GameObject bulletGO = Instantiate(bulletPF, firePoint.position, Quaternion.identity, bulletHolder.transform);
        bullet = bulletGO.GetComponent<Bullet>();
        bullet.MovementDirection = direction;

        int group = GameController.instance.GetSpatialGroupStatic(bullet.transform.position.x, bullet.transform.position.y);
        GameController.instance.bulletSpatialGroups[group].Add(bullet);
    }
}
