using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerSaveData CurrentSaveData { get; set; } = new PlayerSaveData("local");
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
        OnLevelComplete?.Invoke(interactables);
    }
}