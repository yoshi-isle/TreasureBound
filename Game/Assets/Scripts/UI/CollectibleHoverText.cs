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
            GameManager.Instance.OnInteractableFocused += HandleInteractableFocused;
            GameManager.Instance.OnInteractableUnfocused += HandleInteractableUnfocused;
        }
    }

    private void HandleInteractableUnfocused()
    {
        print("hide it");
        isVisible = false;
        print(isVisible);
    }

    void Update()
    {
        print(isVisible);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInteractableFocused -= HandleInteractableFocused;
        }
    }

    private void HandleInteractableFocused(Interactable interactable, Vector3 hitPoint)
    {
        isVisible = true;
        textMeshProUGUI.text = $"{interactable.PromptText}";
        weightDisplayUGUI.text = $"2 kg";
    }
}