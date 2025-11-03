using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{


    public float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private PlayerControl controls;

    void Awake()
    {
        controls = new PlayerControl();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 velocity = move * moveSpeed;
        velocity.y = rb.linearVelocity.y; // mant�m a gravidade
        rb.linearVelocity = velocity;
    }

}
