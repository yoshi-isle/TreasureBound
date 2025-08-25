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
    private Vector3 worldPosition;
    private Collectible currentCollectible;
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
        currentCollectible = collectible;
        textMeshProUGUI.text = $"Pick up: <color=yellow>{collectible.Name}</color>";
        weightDisplayUGUI.text = $"Weight: {collectible.Weight:F1} kg";
    }
}