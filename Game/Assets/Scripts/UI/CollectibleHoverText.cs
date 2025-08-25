using System;
using TMPro;
using UnityEngine;

public class CollectableHoverText : MonoBehaviour
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
            GameManager.Instance.OnCollectableFocused += HandleCollectableFocused;
            GameManager.Instance.OnCollectableUnfocused += HandleCollectableUnfocused;
        }
    }

    void Update()
    {
        print(isVisible);
    }

    private void HandleCollectableUnfocused()
    {
        isVisible = false;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCollectableFocused -= HandleCollectableFocused;
        }
    }

    private void HandleCollectableFocused(Collectable Collectable, Vector3 hitPoint)
    {
        isVisible = true;
        textMeshProUGUI.text = $"<sprite name=Key_E> Pick up: <color=yellow>{Collectable.Name}</color>";
        weightDisplayUGUI.text = $"{Collectable.Weight:F1} kg";
    }
}