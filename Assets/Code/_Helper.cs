using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//use to render spatial group
public class Helper : MonoBehaviour
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
            // Debug.LogError($"[EnemyHelper] Invalid numberOfPartitions: {numberOfPartitions}. Cannot proceed.");
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
            if (GameController.instance.enemySpatialGroups.TryGetValue(spatialGroup, out var groupEnemies))
            {
                enemies.AddRange(groupEnemies);
            }

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
            1, 10, 22, 34, 48, 56, 73, 105, 145, 192, // 1 - 10
            252, 326, 410, 505, 610, 750, 910, 1150, 1420, 1710, // 11-20
            2020, 2350, 2700, 3100, 3530, 3985, 4480, 5030, 5610, 6216, // 21-30
            6870, 7550, 8290, 9100, 10000, 11000, 12400, 14000, 15800, 17800, // 31-40
            20000, 22500, 25300, 28500, 32000, 35700, 39600, 44000, 48600, 53500, // 41-50
        };
        
        if (currentLevel < 0) return 0;
        if (currentLevel >= expChart.Count)
            return expChart.Last(); // hoặc throw error / scale tiếp

        return expChart[currentLevel];

    }

}
