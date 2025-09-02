using System;
using System.Collections.Generic;
using UnityEngine;
using static PlayerSaveData;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerSaveData CurrentSaveData { get; set; } = new PlayerSaveData("local");
    public int currentFloor = 1;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public event Action<Interactable, Vector3> OnInteractableFocused;
    public event System.Action OnInteractableUnfocused;
    public event System.Action<Interactable> OnCollectablePickedUp;
    public event System.Action OnPlayerDead;
    public event System.Action OnGameRestart;
    public event Action OnDungeonGenerated;
    public event Action<List<Interactable>> OnLevelComplete;
    public event Action<List<Interactable>> OnGameQuit;

    public void TriggerInteractableFocused(Interactable interactable, Vector3 hitPoint)
    {
        print("Focused");
        OnInteractableFocused?.Invoke(interactable, hitPoint);
    }

    public void TriggerInteractableUnfocused()
    {
        print("Unfocused");
        OnInteractableUnfocused?.Invoke();
    }

    public void TriggerCollectablePickedUp(Interactable collectable)
    {
        OnCollectablePickedUp?.Invoke(collectable);
    }

    public void TriggerOnPlayerDead()
    {
        currentFloor = 1;
        OnPlayerDead?.Invoke();
    }

    public void TriggerOnGameRestart()
    {
        OnGameRestart?.Invoke();
    }

    public void TriggerOnDungeonGenerated()
    {
        OnDungeonGenerated?.Invoke();
    }

    public void TriggerOnLevelComplete(List<Interactable> interactables)
    {
        currentFloor++;
        OnLevelComplete?.Invoke(interactables);
    }

    public void TriggerOnGameQuit(List<Interactable> collectedItems)
    {
        foreach (var item in collectedItems)
        {
            ItemEntry entry = new()
            {
                itemId = Items.ItemIndex[item.Name],
                count = 1
            };
            int index = CurrentSaveData.CollectedItems.FindIndex(e => e.itemId == entry.itemId);
            if (index != -1)
            {
                var temp = CurrentSaveData.CollectedItems[index];
                temp.count++;
                CurrentSaveData.CollectedItems[index] = temp;
            }
            else
            {
                CurrentSaveData.CollectedItems.Add(entry);
            }
        }

        var file = CurrentSaveData.FileName + ".json";
        string json = JsonUtility.ToJson(CurrentSaveData);

        string filePath = Application.dataPath + "/" + file;
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log($"Save file path: {filePath}");

        OnGameQuit?.Invoke(collectedItems);
    }

}