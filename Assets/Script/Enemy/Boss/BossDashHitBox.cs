using UnityEngine;

public class BossDashHitbox : MonoBehaviour
{
    public int damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player")) 
        {
            Character.instance.ModifyHealth(damage);
            Debug.Log("[BossDashHitbox] Gây damage dash: " + damage);
        }
    }
}
