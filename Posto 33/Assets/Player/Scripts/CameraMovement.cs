using System.Collections;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed = 3f;
    public float lookSensitivity = 1f;
    [Tooltip("Referência ao PlayerMovement (arraste no Inspector)")]

    public PlayerMovement playerMovement;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    [Tooltip("Deslocamento vertical local em metros")]
    public float shakeAmount = 0.04f;

    [Tooltip("Duração total do bob em segundos")]
    public float shakeDuration = 0.12f;

    private Vector3 initialLocalPos;

    private PlayerControl controls;
    private void Start()
    {
        initialLocalPos = transform.localPosition;
    }
    void Awake()
    {
        controls = new PlayerControl();

        controls.Player.Look.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Lock.performed += ctx => {

            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;

        };
    }

    void OnEnable()
    {
        controls?.Enable();
        if (playerMovement != null) playerMovement.OnStep += HandleStep;

    }

    void OnDisable()
    {
        controls?.Disable();
        if (playerMovement != null) playerMovement.OnStep -= HandleStep;

    }
    private void HandleStep()
    {
        // código executado quando OnStep é invocado
        Debug.Log("Passo detectado");
        StartCoroutine(ShakeCam());

    }


    void Update()
    {
        // Rotação da câmera
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -75f, 75f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.parent.Rotate(Vector3.up * mouseX);

      
    }
    private IEnumerator ShakeCam()
    {
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / shakeDuration);
            float offset = Mathf.Sin(norm * Mathf.PI) * shakeAmount; 
            transform.localPosition = initialLocalPos + Vector3.up * offset;
            yield return null;
        }
        transform.localPosition = initialLocalPos;
    }


}
