using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField] LayerMask groundMask;
    public bool onGround = false;
    public float moveSpeed = 5f;
    private bool walking = false;
    private bool canStep = true;


    private Rigidbody rb;
    private Vector2 moveInput;
    private PlayerControl controls;

    public event Action OnStep;

    void Awake()
    {
        controls = new PlayerControl();

        controls.Player.Move.performed += ctx =>
        {
            walking = true;
            moveInput = ctx.ReadValue<Vector2>();
        };
        controls.Player.Move.canceled += ctx => {
            walking = false;
            moveInput = Vector2.zero;
        };
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
    void OnCollisionStay(Collision collision)
    {
        // verifica se o layer do objeto colidido está presente no groundMask
        if ((((1 << collision.gameObject.layer) & groundMask) != 0) && onGround != true)
        {
            onGround = true;
        }
        else if (onGround != false) {
            onGround = false;
        }
    }

    void FixedUpdate()
    {
       
        Vector3 localMove = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 horizontalVelocity = Mathf.Min(1f, localMove.magnitude) * moveSpeed * localMove.normalized;

        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector3(horizontalVelocity.x, 0, horizontalVelocity.z);

        }
        else if (onGround == false) {
            rb.linearVelocity = new Vector3(horizontalVelocity.x, -5, horizontalVelocity.z);

        }
        else { 
        
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);

        }
        if (canStep && walking) {
            canStep = false;
            StartCoroutine(StepTime());

        }
    }
    private IEnumerator StepTime() {
        OnStep?.Invoke();
        yield return new WaitForSeconds(0.8f);
        canStep = true;
    }

}
