using UnityEngine;

public class CamTest : MonoBehaviour
{
    public float speed = 45f; // graus por segundo
    private float angleX = 0f;

    void Update()
    {
        angleX += speed * Time.deltaTime;          // incrementa ao longo do tempo
        angleX = angleX % 360f;                    // mantém entre 0-360
        transform.localRotation = Quaternion.Euler(angleX, 0f, 0f);
    }

}
