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
        int widthRange = GameController.instance.SpatitalGroupWidth;
        int heightRange = GameController.instance.SpatitalGroupHeight;
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
        int widthRange = GameController.instance.SpatitalGroupWidth;
        int heightRange = GameController.instance.SpatitalGroupHeight;
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

    public static List<Enemy> GetAllEnemySpatialGroups(List<int> spatialGroups)
    {
        List<Enemy> enemies = new();

        foreach (int spatialGroup in spatialGroups)
        {
            enemies.AddRange(GameController.instance.enemySpatitalGroups[spatialGroup]);
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



}
