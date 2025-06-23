using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffLibrary : MonoBehaviour
{
    public static Dictionary<string, Buff> AllBuffs = new();

    static BuffLibrary()
    {
        Sprite dummyIcon = null;

        //  Health Buffs 
        AllBuffs["H1"] = new Buff("H1", "Increase Health 1", "Health +1", dummyIcon, () =>
        {
            Character.instance._health += 2;
        });
        AllBuffs["H2"] = new Buff("H2", "Increase Health 2", "Health +2", dummyIcon, () =>
        {
            Character.instance._health += 5;
        }, "H1");
        AllBuffs["H3"] = new Buff("H3", "Increase Health 3", "Health +3", dummyIcon, () =>
        {
            Character.instance._health += 6;
        }, "H2");

        //  Fire Rate Buffs 
        AllBuffs["B1"] = new Buff("B1", "Faster Fire Rate 1", "Fire Rate +1", dummyIcon, () =>
        {
            Character.instance.fireRate -= 0.1f;
        });
        AllBuffs["B2"] = new Buff("B2", "Faster Fire Rate 2", "Fire Rate +2", dummyIcon, () =>
        {
            Character.instance.fireRate -= 0.2f;
        }, "B1");
        AllBuffs["B3"] = new Buff("B3", "Faster Fire Rate 3", "Fire Rate +3", dummyIcon, () =>
        {
            Character.instance.fireRate -= 0.2f;
        }, "B2");

        //  Bullet Damage Buffs 
        AllBuffs["BD1"] = new Buff("BD1", "Bullet Damage 1", "Bullet Dmg +25%", dummyIcon, () =>
        {
            Character.instance.bulletDmgMultiplier += 0.25f;
        });
        AllBuffs["BD2"] = new Buff("BD2", "Bullet Damage 2", "Bullet Dmg +50%", dummyIcon, () =>
        {
            Character.instance.bulletDmgMultiplier += 0.5f;
        }, "BD1");
        AllBuffs["BD3"] = new Buff("BD3", "Bullet Damage 3", "Bullet Dmg +75%", dummyIcon, () =>
        {
            Character.instance.bulletDmgMultiplier += 0.75f;
        }, "BD2");

        //  Player Speed Buffs 
        AllBuffs["S1"] = new Buff("S1", "Move Speed 1", "Speed +20%", dummyIcon, () =>
        {
            Character.instance._speedMultiplier += 0.2f;
        });
        AllBuffs["S2"] = new Buff("S2", "Move Speed 2", "Speed +50%", dummyIcon, () =>
        {
            Character.instance._speedMultiplier += 0.5f;
        }, "S1");
        AllBuffs["S3"] = new Buff("S3", "Move Speed 3", "Speed +100%", dummyIcon, () =>
        {
            Character.instance._speedMultiplier += 1f;
        }, "S2");

        //  Collect Radius Buffs 
        AllBuffs["CR1"] = new Buff("CR1", "Collect Radius 1", "Pickup Radius +20%", dummyIcon, () =>
        {
            Character.instance.collectRadiousMultiplier += 0.2f;
        });
        AllBuffs["CR2"] = new Buff("CR2", "Collect Radius 2", "Pickup Radius +50%", dummyIcon, () =>
        {
            Character.instance.collectRadiousMultiplier += 0.5f;
        }, "CR1");
        AllBuffs["CR3"] = new Buff("CR3", "Collect Radius 3", "Pickup Radius +100%", dummyIcon, () =>
        {
            Character.instance.collectRadiousMultiplier += 1f;
        }, "CR2");

        //  Multi Shoot Buffs 
        AllBuffs["MS1"] = new Buff("MS1", "Multi Shoot 1", "Shoot +1 bullet", dummyIcon, () =>
        {
            Character.instance.bulletPerShoot += 1;
        });
        AllBuffs["MS2"] = new Buff("MS2", "Multi Shoot 2", "Shoot +2 bullets", dummyIcon, () =>
        {
            Character.instance.bulletPerShoot += 1;
        }, "MS1");
        AllBuffs["MS3"] = new Buff("MS3", "Multi Shoot 3", "Shoot +3 bullets", dummyIcon, () =>
        {
            Character.instance.bulletPerShoot += 1;
        }, "MS2");

        //  Spread Shoot Buffs 
        AllBuffs["SB1"] = new Buff("SB1", "Spread Shot 1", "Spread +1 bullet", dummyIcon, () =>
        {
            Character.instance.bulletPerShootSpread += 1;
            Character.instance.spreadAngle += 10f;
        });
        AllBuffs["SB2"] = new Buff("SB2", "Spread Shot 2", "Spread +2 bullets", dummyIcon, () =>
        {
            Character.instance.bulletPerShootSpread += 1;
            Character.instance.spreadAngle += 10f;
        }, "SB1");
        AllBuffs["SB3"] = new Buff("SB3", "Spread Shot 3", "Spread +3 bullets", dummyIcon, () =>
        {
            Character.instance.bulletPerShootSpread += 1;
            Character.instance.spreadAngle += 10f;
        }, "SB2");


        //  Fire Bullet Buff 
        AllBuffs["FB1"] = new Buff("FB1", "Fire Bullet 1", "Add Fire bullet +1", dummyIcon, () =>
        {
            Character.instance.isFireBulletOn = true;
        });
        AllBuffs["FB2"] = new Buff("FB2", "Fire Bullet 2", "Add Fire bullet +1", dummyIcon, () =>
        {
            Character.instance.burnDmg += 1;
            Character.instance.burnTime += 3;
        }, "FB1");
        AllBuffs["FB3"] = new Buff("FB3", "Fire Bullet 3", "Add Fire bullet +1", dummyIcon, () =>
        {
            Character.instance.burnDmg += 2;
            Character.instance.burnTime += 3;
        }, "FB2");

        //  Lightning Spell Buff 
        AllBuffs["IS1"] = new Buff("IS1", "Ice Spell 1", "Using Ice Spell +1", dummyIcon, () =>
        {

        });
        AllBuffs["IS2"] = new Buff("IS2", "Ice Spell 2", "Using Ice Spell +1", dummyIcon, () =>
        {

        }, "IS1");
        AllBuffs["IS3"] = new Buff("IS3", "Ice Spell 3", "Using Ice Spell +1", dummyIcon, () =>
        {

        }, "IS2");

        //  Ice Spell Buff 
        AllBuffs["IS1"] = new Buff("IS1", "Ice Spell 1", "Using Ice Spell +1", dummyIcon, () =>
        {
            Character.instance.isIceSpellOn = true;
            Character.instance.isAutoShootOn = true;
            Character.instance.autoBulletType = "ice";
            Character.instance.autoShootInterval = 3f;
        });
        AllBuffs["IS2"] = new Buff("IS2", "Ice Spell 2", "Using Ice Spell +1", dummyIcon, () =>
        {
            Character.instance.iceSlowNumber += 0.3f;
            Character.instance.iceSlowTime += 2f;
        }, "IS1");
        AllBuffs["IS3"] = new Buff("IS3", "Ice Spell 3", "Using Ice Spell +1", dummyIcon, () =>
        {
            Character.instance.iceSlowNumber += 0.5f;
            Character.instance.iceSlowTime += 2f;
        }, "IS2");

        //  Wind Spell Buff 
        AllBuffs["WS1"] = new Buff("WS1", "Wind Spell 1", "Using Wind Spell +1", dummyIcon, () =>
        {

        });
        AllBuffs["WS2"] = new Buff("WS2", "Wind Spell 2", "Using Wind Spell +1", dummyIcon, () =>
        {

        }, "WS1");
        AllBuffs["WS3"] = new Buff("WS3", "Wind Spell 3", "Using Wind Spell +1", dummyIcon, () =>
        {

        }, "WS2");

        //  Dark Spell Buff 
        AllBuffs["DSR"] = new Buff("DS1", "Dark Spell 1", "Using Dark Spell +1", dummyIcon, () =>
        {

        });
        AllBuffs["DS2"] = new Buff("DS2", "Dark Spell 2", "Using Dark Spell +1", dummyIcon, () =>
        {

        }, "DS1");
        AllBuffs["DS3"] = new Buff("DS3", "Dark Spell 3", "Using Dark Spell +1", dummyIcon, () =>
        {

        }, "DS2");

    }

}
