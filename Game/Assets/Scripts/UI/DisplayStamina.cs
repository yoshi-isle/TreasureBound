using UnityEngine;

public class DisplayStamina : MonoBehaviour
{
    float initialWidth;
    FirstPersonController firstPersonController;
    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialWidth = rectTransform.sizeDelta.x;
        firstPersonController = FindFirstObjectByType<FirstPersonController>();
        if (firstPersonController == null)
        {
            Debug.LogError("FirstPersonController not found in the scene.");
        }
    }
    void Update()
    {
        float stamina = firstPersonController.stamina;
        float newWidth = (stamina / 100f) * initialWidth;
        rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
    }
}
