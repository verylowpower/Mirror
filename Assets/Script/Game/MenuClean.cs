using UnityEngine;
using UnityEngine.EventSystems;

public class MenuSceneCleaner : MonoBehaviour
{
    void Awake()
    {
        if (EventSystem.current != null)
        {
            var allES = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            foreach (var es in allES)
            {
                if (es != EventSystem.current)
                {
                    Destroy(es.gameObject); 
                }
            }
        }
    }
}
