using UnityEngine;

public class AudioListenerCleaner : MonoBehaviour
{
    void Awake()
    {
        var allListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (allListeners.Length > 1)
        {
            foreach (var listener in allListeners)
            {
                if (listener != Camera.main.GetComponent<AudioListener>())
                {
                    listener.enabled = false;
                }
            }
        }
    }
}
