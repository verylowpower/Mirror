using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    [SerializeField] private float angleOffset = -90f; // Tùy chỉnh theo sprite

    private Transform player;

    private void Start()
    {
        player = GameController.instance.character;
    }

    public void FaceTowardsPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + angleOffset);
    }
}
