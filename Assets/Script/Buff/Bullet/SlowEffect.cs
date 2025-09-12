using UnityEngine;

public class SlowEffect : IBulletEffect
{
    private readonly float slowPercent;
    private readonly float slowTime;

    public SlowEffect(float percent, float time)
    {
        slowPercent = percent;
        slowTime = time;
    }

    public void Apply(Enemy enemy, Bullet bullet)
    {
        if (enemy == null) return;

        // Gọi hàm slow trong Enemy
        enemy.ApplySlow(slowPercent, slowTime);

        // // Spawn slow VFX
        // if (bullet.SlowVFXPrefab != null)
        // {
        //     Object.Instantiate(bullet.SlowVFXPrefab, enemy.transform.position, Quaternion.identity);
        // }
    }
}
