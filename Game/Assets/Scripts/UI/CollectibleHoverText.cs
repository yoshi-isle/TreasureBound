using System;
using TMPro;
using UnityEngine;

public class CollectibleHoverText : MonoBehaviour
{
    private bool isVisible
    {
        get
        {
            return textMeshProUGUI.enabled && weightDisplayUGUI.enabled;
        }
        set
        {
            textMeshProUGUI.gameObject.SetActive(value);
            weightDisplayUGUI.gameObject.SetActive(value);
        }
    }
    public TextMeshProUGUI textMeshProUGUI;
    public TextMeshProUGUI weightDisplayUGUI;

    void Start()
    {
        isVisible = false;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCollectibleFocused += HandleCollectibleFocused;
            GameManager.Instance.OnCollectibleUnfocused += HandleCollectibleUnfocused;
        }
    }

    void Update()
    {
        print(isVisible);
    }

    private void HandleCollectibleUnfocused()
    {
        isVisible = false;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCollectibleFocused -= HandleCollectibleFocused;
        }
    }

    private void HandleCollectibleFocused(Collectible collectible, Vector3 hitPoint)
    {
        isVisible = true;
        textMeshProUGUI.text = $"<sprite name=Key_E> Pick up: <color=yellow>{collectible.Name}</color>";
        weightDisplayUGUI.text = $"{collectible.Weight:F1} kg";
    }
}