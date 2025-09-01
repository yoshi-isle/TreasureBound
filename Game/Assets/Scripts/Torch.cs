using UnityEngine;

public class Torch : MonoBehaviour
{
    [Header("Screen Position Settings")]
    [Range(0f, 1f)]
    public float screenX = 0.1f;
    [Range(0f, 1f)]
    public float screenY = 0.1f;
    public float distanceFromCamera = 2f;
    
    private Camera parentCamera;
    
    void Start()
    {
        parentCamera = GetComponentInParent<Camera>();
        if (parentCamera == null)
            parentCamera = Camera.main;
            
        UpdatePosition();
    }

    void Update()
    {
        UpdatePosition();
    }
    
    void UpdatePosition()
    {
        if (parentCamera == null) return;
        
        Vector3 screenPosition = new Vector3(
            screenX * Screen.width,
            screenY * Screen.height,
            distanceFromCamera
        );
        
        Vector3 worldPosition = parentCamera.ScreenToWorldPoint(screenPosition);
        
        transform.position = worldPosition;
    }
}
