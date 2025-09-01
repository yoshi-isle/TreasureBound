using System;
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
        text.text = "Floor " + currentLevel;
        GameManager.Instance.OnLevelComplete += HandleLevelComplete;
        GameManager.Instance.OnPlayerDead += SyncCurrentFloor;
        GameManager.Instance.OnGameRestart += () => currentLevel = 1;
    }

    private void OnEnable()
    {
        SyncCurrentFloor();
    }

    private void HandleLevelComplete(List<Interactable> list)
    {
        currentLevel++;
        SyncCurrentFloor();
    }

    private void SyncCurrentFloor()
    {
        text.text = "Floor " + currentLevel;
        if (currentLevel != GameManager.Instance.currentFloor)
        {
            currentLevel = GameManager.Instance.currentFloor;
        }
    }
}
