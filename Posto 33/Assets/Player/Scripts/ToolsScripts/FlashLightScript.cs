using UnityEngine;

public class FlashLightScript : MonoBehaviour
{


    [SerializeField] Transform cameraTransform;
    [SerializeField] float distance = 0.6f;
    [SerializeField] float heightOffset = 0f;

    void LateUpdate()
    {
        Vector3 targetPos = cameraTransform.position + cameraTransform.forward * distance + Vector3.up * heightOffset;
        transform.position = targetPos;
    }




}
