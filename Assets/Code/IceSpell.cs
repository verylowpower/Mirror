using UnityEngine;

public class IceSpell : Bullet
{
    //public GameObject fireBullet;
    //FireBullet instance;
    //public float fireBulletDmg;
    public float slowDownNumber;
    public float slowDuration;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            //enemy.ChangeHealth(fireBulletDmg);
            enemy.ApplySlow(slowDownNumber, slowDuration);
            //OnContactEnemy?.Invoke(transform);
            //DestroyBullet();
        }
    }
}
