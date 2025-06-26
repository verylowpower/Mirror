using UnityEngine;
using UnityEngine.EventSystems;

public class MenuSceneCleaner : MonoBehaviour
{
    void Awake()
    {
        // Nếu đã có 1 EventSystem rồi (ví dụ từ GameScene)
        if (EventSystem.current != null)
        {
            var allES = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            foreach (var es in allES)
            {
                if (es != EventSystem.current)
                {
                    Destroy(es.gameObject); // Xoá cái EventSystem thừa
                }
            }
        }
    }
}
