using UnityEngine;

public class HandScript : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Distância à frente da câmera")]
    public float forwardDistance = 0.8f;

    [Tooltip("Deslocamento vertical relativo à câmera")]
    public float heightOffset = 0f;

    [Tooltip("Tempo de suavização (0 = sem suavização)")]
    public float smoothTime = 0.06f;

    private Vector3 velocity = Vector3.zero;
    void LateUpdate()
    {
        if (cameraTransform == null) return;

        


    }

}
