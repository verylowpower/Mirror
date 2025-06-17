using System.Collections.Generic;
using UnityEngine;

public class BuffLibrary : MonoBehaviour
{
    public static Dictionary<string, Buff> AllBuffs = new();

    static BuffLibrary()
    {
        Sprite dummyIcon = null;

        // Stat buff
        //health buff
        AllBuffs["H1"] = new Buff("H1", "Increase Health 1", "Health +1", dummyIcon, () =>
        {
            Character.instance._health += 1;
        });
        AllBuffs["H2"] = new Buff("H2", "Increase Health 2", "Health +2", dummyIcon, () =>
        {
            Character.instance._health += 2;
        });
        AllBuffs["H3"] = new Buff("H3", "Increase Health 3", "Health +3", dummyIcon, () =>
        {
            Character.instance._health += 3;
        });
        // AllBuffs["H4"] = new Buff("H4", "Recover Health", "Recover", dummyIcon, () =>
        // {
        //     Character.instance._curHealth = Character.instance._health;
        // });

        //Bullet speed buff
        AllBuffs["B1"] = new Buff("B1", "Increase BulletSpeed 1", "Bullet Speed +1", dummyIcon, () =>
        {
            Character.instance.bulletSpeedMultiplier += 0.5f;
            Character.instance.fireRate -= 0.1f;
        });
        AllBuffs["B2"] = new Buff("B2", "Increase BulletSpeed 2", "Bullet Speed +2", dummyIcon, () =>
        {
            Character.instance.bulletSpeedMultiplier += 0.75f;
            Character.instance.fireRate -= 0.2f;
        });
        AllBuffs["B3"] = new Buff("B3", "Increase BulletSpeed 3", "Bullet Speed +3", dummyIcon, () =>
        {
            Character.instance.bulletSpeedMultiplier += 2.0f;
            Character.instance.fireRate -= 0.3f;
        });

        //Bullet dmg buff
        AllBuffs["BD1"] = new Buff("BD1", "Increase BulletDamge 1", "Bullet Dmg +1", dummyIcon, () =>
        {
            Character.instance.bulletDmgMultiplier += 0.25f;
        });
        AllBuffs["BD2"] = new Buff("BD2", "Increase BulletDamge 2", "Bullet Dmg +2", dummyIcon, () =>
        {
            Character.instance.bulletDmgMultiplier += 0.5f;
        });
        AllBuffs["BD3"] = new Buff("BD3", "Increase BulletDamge 3", "Bullet Dmg +3", dummyIcon, () =>
        {
            Character.instance.bulletDmgMultiplier += 0.75f;
        });


        //Character speed buff
        AllBuffs["S1"] = new Buff("S1", "Increase Speed 1", "Speed +1", dummyIcon, () =>
        {
            Character.instance._speedMultiplier += 0.2f;
        });
        AllBuffs["S2"] = new Buff("S2", "Increase Speed 2", "Speed +2", dummyIcon, () =>
        {
            Character.instance._speedMultiplier += 0.5f;
        });
        AllBuffs["S3"] = new Buff("S3", "Increase Speed 3", "Speed +3", dummyIcon, () =>
        {
            Character.instance._speedMultiplier += 2f;
        });

        //Collect radious buff
        AllBuffs["CR1"] = new Buff("CR1", "Increase Collect Radious 1", "Collect Radious +1", dummyIcon, () =>
        {
            Character.instance.collectRadiousMultiplier += 0.2f;
        });
        AllBuffs["CR2"] = new Buff("CR2", "Increase Collect Radious 2", "Collect Radious +2", dummyIcon, () =>
        {
            Character.instance.collectRadiousMultiplier += 0.5f;
        });
        AllBuffs["CR3"] = new Buff("CR3", "Increase Collect Radious 3", "Collect Radious +3", dummyIcon, () =>
        {
            Character.instance.collectRadiousMultiplier += 2f;
        });

    }

}
