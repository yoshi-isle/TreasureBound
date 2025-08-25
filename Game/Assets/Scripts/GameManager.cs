using System;
using UnityEngine;
using UnityEngine.Rendering;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum GameState
    {
        Normal,
        Paused
    }
    public GameState gameState = GameState.Normal;
    public Canvas mainGameCanvas;
    public Canvas pauseMenuCanvas;
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

    void Update()
    {
        if (Input.GetKeyDown(key: KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        switch (gameState)
        {
            case GameState.Normal:
                gameState = GameState.Paused;
                Time.timeScale = 0f;
                mainGameCanvas.gameObject.SetActive(false);
                pauseMenuCanvas.gameObject.SetActive(true);
                // Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameState.Paused:
                gameState = GameState.Normal;
                Time.timeScale = 1f;
                mainGameCanvas.gameObject.SetActive(true);
                pauseMenuCanvas.gameObject.SetActive(false);
                // Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                pauseMenuCanvas.gameObject.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public event System.Action<Collectable, Vector3> OnCollectableFocused;
    public event System.Action OnCollectableUnfocused;

    public void TriggerCollectableFocused(Collectable Collectable, Vector3 hitPoint)
    {
        OnCollectableFocused?.Invoke(Collectable, hitPoint);
    }
    public void TriggerCollectableUnfocused()
    {
        OnCollectableUnfocused?.Invoke();
    }
}