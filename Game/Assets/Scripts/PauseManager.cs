using UnityEngine.UI;
using UnityEngine;
using System;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

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
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameState.Paused:
                gameState = GameState.Normal;
                Time.timeScale = 1f;
                mainGameCanvas.gameObject.SetActive(true);
                pauseMenuCanvas.gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                pauseMenuCanvas.gameObject.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
