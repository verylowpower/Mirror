using UnityEngine;

public class FireBullet : Bullet
{
    //public GameObject fireBullet;
    //FireBullet instance;
    //public float fireBulletDmg;
    public float burnDmg;
    public float burnTime;

    void Awake()
    {
        instance = this;
    }
    

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            //enemy.ChangeHealth(fireBulletDmg);
            enemy.ApplyBurn(burnDmg, burnTime);
            //OnContactEnemy?.Invoke(transform);
            //DestroyBullet();
        }
    }
}
