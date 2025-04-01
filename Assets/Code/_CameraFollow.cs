using UnityEngine;

public class _CameraFollow : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _target;

    void Update()
    {
        if (_target == null) return;
        _camera.transform.position = _target.transform.position + new Vector3(0, 0, -5);
    }
}
