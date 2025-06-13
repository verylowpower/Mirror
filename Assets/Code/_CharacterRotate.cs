using UnityEngine;

public class _CharacterRotate : MonoBehaviour
{
    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 12f; // Đẩy chuột ra mặt phẳng Z = 0

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        Vector2 direction = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle); // hoặc angle tùy sprite
        //Debug.Log("Angle: " + angle);
    }


}
