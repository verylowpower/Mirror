using UnityEngine;

public class BurnEffect : IBulletEffect
{
    [SerializeField] private float burnDmg;
    [SerializeField] private float burnTime;

    public BurnEffect(float dmg, float time)
    {
        burnDmg = dmg;
        burnTime = time;
    }

    public void Apply(Enemy enemy, Bullet bullet)
    {
        if (enemy == null) return;

        enemy.ApplyBurn(burnDmg, burnTime);

        // if (bullet.BurnVFXPrefab != null)
        // {
        //     Object.Instantiate(bullet.BurnVFXPrefab, enemy.transform.position, Quaternion.identity);
        // }
    }
}
