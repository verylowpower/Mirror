using UnityEngine;
using UnityEngine.InputSystem;

public class _Character_Move : MonoBehaviour
{
    [Header("Stat")]
    [SerializeField] private float _speed = 3.0f;

    private PlayerInputAct inputActions;
    private Rigidbody2D _rb;
    private Vector2 moveInput;

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputAct();
        inputActions.PlayerControl.Move.performed += OnMove;
        inputActions.PlayerControl.Move.canceled += OnMove;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }


    public void FixedUpdate()
    {
        Vector2 newPositon = _rb.position + moveInput * _speed * Time.fixedDeltaTime;

        _rb.MovePosition(newPositon);

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Đảm bảo không thay đổi trục Z

        // Tính toán góc quay từ nhân vật đến chuột
        Vector2 direction = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Gán góc quay cho nhân vật
        _rb.rotation = angle;
    }
}
