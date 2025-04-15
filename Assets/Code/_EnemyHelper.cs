using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//use to render spatial group
public class EnemyHelper : MonoBehaviour
{
    public static List<int> GetExpandedSpatialGroupsV2(int spatialGroup, int radious = 1)
    {
        List<int> expandedSpatialGroups = new(); //list to save parition
        int widthRange = GameController.instance.SpatialGroupWidth;
        int heightRange = GameController.instance.SpatialGroupHeight;
        int numberOfPartition = GameController.instance.NumberOfPartitions;

        //check each partition
        for (int dx = -radious; dx <= radious; dx++)
        {
            for (int dy = -radious; dy <= radious; dy++)
            {
                int newGroup = spatialGroup + dx + dy * widthRange; //postion of new parition

                //check if square inside map
                bool isWithinWidth = newGroup % widthRange >= 0 && newGroup % widthRange < widthRange;
                bool isWithinHeight = newGroup / heightRange >= 0 && newGroup / heightRange < heightRange;
                bool isWithinBonds = isWithinHeight && isWithinWidth; //if width and height in map 

                bool isWithinPartition = newGroup >= 0 && newGroup < numberOfPartition; //if partition !> map size

                if (isWithinBonds && isWithinPartition)
                {
                    expandedSpatialGroups.Add(newGroup); //add to list
                }
            }
        }
        return expandedSpatialGroups.Distinct().ToList(); //remove duplicates
    }

    public static List<int> GetExpandedSpatialGroups(int spatialGroup, int numberOfPartitions = -1)
    {
        List<int> expandedSpatialGroups = new(); //list to add
        int widthRange = GameController.instance.SpatialGroupWidth;
        int heightRange = GameController.instance.SpatialGroupHeight;
        if (numberOfPartitions == -1) //check partition, if not add from GameController
            numberOfPartitions = GameController.instance.NumberOfPartitions;
        if (numberOfPartitions <= 0)
        {
            Debug.LogError($"[EnemyHelper] Invalid numberOfPartitions: {numberOfPartitions}. Cannot proceed.");
            return expandedSpatialGroups;
        }
        int sqrtOfPartitions = (int)Mathf.Sqrt(numberOfPartitions); // calculate grid size of map
        int numberOfRows = sqrtOfPartitions; //row and col are same -> map square
        int partitionPerRows = sqrtOfPartitions;

        //check corner
        bool isLeft = spatialGroup % partitionPerRows == 0;
        bool isRight = spatialGroup % partitionPerRows == partitionPerRows - 1;
        bool isTop = spatialGroup / partitionPerRows >= numberOfRows - 1;
        bool isBottom = spatialGroup / partitionPerRows == 0;

        //check side
        if (!isTop) expandedSpatialGroups.Add(spatialGroup + partitionPerRows);
        if (!isBottom) expandedSpatialGroups.Add(spatialGroup - partitionPerRows);
        if (!isLeft) expandedSpatialGroups.Add(spatialGroup - 1);
        if (!isRight) expandedSpatialGroups.Add(spatialGroup + 1);

        //diagonals
        if (!isTop && !isRight) expandedSpatialGroups.Add(spatialGroup + partitionPerRows + 1);
        if (!isTop && !isLeft) expandedSpatialGroups.Add(spatialGroup + partitionPerRows - 1);
        if (!isBottom && !isRight) expandedSpatialGroups.Add(spatialGroup - partitionPerRows + 1);
        if (!isBottom && !isLeft) expandedSpatialGroups.Add(spatialGroup - partitionPerRows - 1);

        return expandedSpatialGroups;
    }

    //take spatial around enemy movement direction
    public static List<int> GetExpandedSpatialGroups(int spatialGroup, Vector2 direction)
    {
        List<int> expandedSpatialGroups = new List<int>() { spatialGroup };

        bool goingRight = direction.x > 0;
        bool goingTop = direction.y > 0;

        int widthRange = GameController.instance.SpatialGroupWidth; // ex. 100
        int heightRange = GameController.instance.SpatialGroupHeight; // ex. 100

        bool isLeft = spatialGroup % widthRange == 0;
        bool isRight = spatialGroup % widthRange == widthRange - 1;
        bool isTop = spatialGroup / widthRange == heightRange - 1;
        bool isBottom = spatialGroup / widthRange == 0;

        // Sides
        if (!isTop && goingTop) expandedSpatialGroups.Add(spatialGroup + widthRange);
        if (!isBottom && !goingTop) expandedSpatialGroups.Add(spatialGroup - widthRange);
        if (!isLeft && !goingRight) expandedSpatialGroups.Add(spatialGroup - 1);
        if (!isRight && goingRight) expandedSpatialGroups.Add(spatialGroup + 1);

        // Diagonals
        if (!isTop && !isRight && (goingTop || goingRight)) expandedSpatialGroups.Add(spatialGroup + widthRange + 1); // top right
        if (!isTop && !isLeft && (goingTop || !goingRight)) expandedSpatialGroups.Add(spatialGroup + widthRange - 1); // top left
        if (!isBottom && !isRight && (!goingTop || goingRight)) expandedSpatialGroups.Add(spatialGroup - widthRange + 1);
        // bottom right
        if (!isBottom && !isLeft && (!goingTop || !goingRight)) expandedSpatialGroups.Add(spatialGroup - widthRange - 1);
        // bottom left

        return expandedSpatialGroups;
    }

    public static List<Enemy> GetAllEnemySpatialGroups(List<int> spatialGroups)
    {
        List<Enemy> enemies = new();

        foreach (int spatialGroup in spatialGroups)
        {
            enemies.AddRange(GameController.instance.enemySpatialGroups[spatialGroup]);
        }
        return enemies;
    }

    public static Vector3 V2toV3(Vector2 v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    public static Vector2 V3toV2(Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static string ListToString<T>(List<T> list)
    {
        return string.Join(", ", list);
    }

    public static long GetExpRequired(int currentLevel)
    {
        List<long> expChart = new()
        {   //exp neeed                               // lvl  
            0, 10, 22, 34, 48, 56, 73, 105, 145, 192, // 1 - 10
            252, 326, 410, 505, 610, 750, 910, 1150, 1420, 1710, // 11-20
            2020, 2350, 2700, 3100, 3530, 3985, 4480, 5030, 5610, 6216, // 21-30
            6870, 7550, 8290, 9100, 10000, 11000, 12400, 14000, 15800, 17800, // 31-40
            20000, 22500, 25300, 28500, 32000, 35700, 39600, 44000, 48600, 53500, // 41-50
            // 58600, 63900, 69500, 75300, 81300, 87651, 94200, 101500, 108940, 116550, // 51-60
            // 124500, 132780, 141380, 151710, 162510, 173770, 185530, 197780, 210550, 223850, // 61-70
            // 237690, 252090, 267070, 282640, 298820, 315630, 333070, 351180, 369970, 389450, // 71-80
            // 409650, 430590, 452290, 474760, 498030, 522110, 547040, 572840, 599520, 627120, // 81-90
            // 655650, 685140, 715610, 747100, 779630, 813230, 847920, 883740, 920710, 958860, // 91-100
            // 998230, 1038850, 1080750, 1123960, 1168530, 1214470, 1261840, 1310660, 1360980, 1412830, // 101-110
            // 1466260, 1521300, 1578000, 1636400, 1696550, 1758480, 1822250, 1887910, 1955500, 2025070, // 111-120
            // 2096670, 2170370, 2246200, 2324240, 2404520, 2487120, 2572090, 2659490, 2749390, 2841860, // 121-130
            // 2936940, 3034730, 3135270, 3238660, 3344950, 3454230, 3566570, 3682060, 3800760, 3922780, // 131-140
            // 4048180, 4177060, 4309520, 4445640, 4585510, 4729240, 4876930, 5028670, 5184580, 5344760, // 141-150
            // 5509330, 5678390, 5852060, 6030470, 6213740, 6401990, 6595360, 6793980, 6997980, 7207510, // 151-160
            // 7422720, 7643740, 7870740, 8103870, 8343290, 8589170, 8841670, 9100980, 9367270, 9640730, // 161-170
            // 9921540, 10209910, 10506030, 10810100, 11223500, 11442980, 11772220, 12110300, 12457450, 12813930, // 171-180
            // 13179970, 13555840, 14041790, 14638110, 15345060, 16162940, 17092050, 18132670, 19285130, 20549750, // 181-190
            // 21926860, 23416790, 25019900, 26736550, 28567100, 30511940, 32571460, 34746060, 37036160, 55554240 // 191-200
        };

        return expChart[currentLevel];
    }

}
