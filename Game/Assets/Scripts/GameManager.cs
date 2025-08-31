using System;
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

    public event System.Action<Collectable, Vector3> OnCollectableFocused;
    public event System.Action OnCollectableUnfocused;
    public event System.Action<Collectable> OnCollectablePickedUp;
    public event System.Action OnPlayerDead;
    public event System.Action OnGameRestart;

    public void TriggerCollectableFocused(Collectable collectable, Vector3 hitPoint)
    {
        print("Focused");
        OnCollectableFocused?.Invoke(collectable, hitPoint);
    }
    public void TriggerCollectableUnfocused()
    {
        print("Unfocused");
        OnCollectableUnfocused?.Invoke();
    }

    public void TriggerCollectablePickedUp(Collectable collectable)
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
}