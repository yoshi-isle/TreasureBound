using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayFloorNumber : MonoBehaviour
{
    TextMeshProUGUI text;
    int currentLevel = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        GameManager.Instance.OnLevelComplete += HandleLevelComplete;
    }

    private void HandleLevelComplete(List<Interactable> list)
    {
        currentLevel++;
        text.text = "Floor " + currentLevel;
    }
}
