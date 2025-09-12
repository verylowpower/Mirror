using UnityEngine;
using System.Collections.Generic;

public class LightningEffect : IBulletEffect
{
    private readonly int chainCount;
    private readonly float chainRange;

    public LightningEffect(int count, float range)
    {
        chainCount = count;
        chainRange = range;
    }

    public void Apply(Enemy enemy, Bullet bullet)
    {
        if (enemy == null) return;

        // Damage enemy đầu tiên
        enemy.ChangeHealth(bullet.bulletDmg);

        // Chain tới các enemy khác trong phạm vi
        List<Enemy> nearbyEnemies = GameController.instance.GetEnemiesInRange(enemy.transform.position, chainRange);

        int chained = 0;
        foreach (var e in nearbyEnemies)
        {
            if (e == enemy) continue;
            e.ChangeHealth(bullet.bulletDmg * 0.5f); // vd: chain damage yếu hơn
            chained++;

            // // Spawn lightning VFX
            // if (bullet.LightningVFXPrefab != null)
            // {
            //     Object.Instantiate(bullet.LightningVFXPrefab, e.transform.position, Quaternion.identity);
            // }

            if (chained >= chainCount) break;
        }
    }
}
