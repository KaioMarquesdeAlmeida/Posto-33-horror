using System.Linq;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    private PlayerControl controls;

    void Awake()
    {
        controls = new PlayerControl();

        controls.Player.Look.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        // Rotação da câmera
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.parent.Rotate(Vector3.up * mouseX);

        // Movimento com teclado ou controle
        Vector3 move = transform.parent.forward * moveInput.y + transform.parent.right * moveInput.x;
        transform.parent.position += move * moveSpeed * Time.deltaTime;
    }

}
