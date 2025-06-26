using UnityEngine;

public class FactoryPattern
{
    public class BulletFactory
    {
        public static GameObject CreateBullet(string bulletType, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject prefab = null;

            switch (bulletType)
            {
                case "normal":
                    prefab = Resources.Load<GameObject>("Prefab/GameObj/NormalBullet");
                    break;
                case "fire":
                    prefab = Resources.Load<GameObject>("Prefab/GameObj/FireBullet");
                    break;
                case "ice":
                    prefab = Resources.Load<GameObject>("Prefab/GameObj/IceBullet");
                    break;
            }

            if (prefab == null)
            {
                Debug.LogError($"[BulletFactory] Invalid bullet type or path. Type: {bulletType}");
                return null;
            }

            return Object.Instantiate(prefab, position, rotation, parent);
        }
    }
}
