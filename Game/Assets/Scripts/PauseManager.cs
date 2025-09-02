using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using System.Collections.Generic;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    public enum GameState
    {
        Normal,
        Paused
    }
    public GameState gameState = GameState.Normal;
    public Canvas mainGameCanvasPrefab, pauseMenuCanvasPrefab, gameOverCanvasPrefab, loadingCanvasPrefab;
    private Canvas mainGameCanvas, pauseMenuCanvas, gameOverCanvas, loadingCanvas;
    public GameObject pendingItemGrid, pendingItemBox;
    public Button m_RestartGame;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainGameCanvas = Instantiate(mainGameCanvasPrefab);
        pauseMenuCanvas = Instantiate(pauseMenuCanvasPrefab);
        gameOverCanvas = Instantiate(gameOverCanvasPrefab);
        loadingCanvas = Instantiate(loadingCanvasPrefab);
        mainGameCanvas.transform.parent = this.transform;
        pauseMenuCanvas.transform.parent = this.transform;
        gameOverCanvas.transform.parent = this.transform;
        loadingCanvas.transform.parent = this.transform;

        GameManager.Instance.OnPlayerDead += HandlePlayerDead;
        GameManager.Instance.OnDungeonGenerated += HandleDungeonGenerated;
        GameManager.Instance.OnGameRestart += HandleGameRestart;
        GameManager.Instance.OnLevelComplete += HandleLevelComplete;
        m_RestartGame.onClick.AddListener(OnRestartGameButtonPressed);
        loadingCanvas.gameObject.SetActive(true);
        mainGameCanvas.gameObject.SetActive(false);
        pauseMenuCanvas.gameObject.SetActive(false);
        gameOverCanvas.gameObject.SetActive(false);
        print("1");
    }

    private void HandleLevelComplete(List<Interactable> list)
    {
        loadingCanvas.gameObject.SetActive(true);
        mainGameCanvas.gameObject.SetActive(false);
        pauseMenuCanvas.gameObject.SetActive(false);
        gameOverCanvas.gameObject.SetActive(false);
    }

    private void HandleGameRestart()
    {
        loadingCanvas.gameObject.SetActive(true);
        mainGameCanvas.gameObject.SetActive(false);
        pauseMenuCanvas.gameObject.SetActive(false);
        gameOverCanvas.gameObject.SetActive(false);
    }

    private void HandleDungeonGenerated()
    {
        print("2");
        loadingCanvas.gameObject.SetActive(false);
        mainGameCanvas.gameObject.SetActive(true);
        pauseMenuCanvas.gameObject.SetActive(false);
        gameOverCanvas.gameObject.SetActive(false);
    }

    private void OnRestartGameButtonPressed()
    {
        GameManager.Instance.TriggerOnGameRestart();
        loadingCanvas.gameObject.SetActive(true);
        mainGameCanvas.gameObject.SetActive(false);
        pauseMenuCanvas.gameObject.SetActive(false);
        gameOverCanvas.gameObject.SetActive(false);
    }

    private void HandlePlayerDead()
    {
        gameOverCanvas.gameObject.SetActive(true);
        mainGameCanvas.gameObject.SetActive(false);
        pauseMenuCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
                //TODO - Improve this connection
                foreach (var pendingItems in FindAnyObjectByType<Inventory>().Bag)
                {
                    var itemBox = Instantiate(pendingItemBox, pendingItemGrid.transform);
                    itemBox.GetComponentInChildren<TextMeshProUGUI>().text = pendingItems.Name;
                }
                break;
            case GameState.Paused:
                gameState = GameState.Normal;
                Time.timeScale = 1f;
                mainGameCanvas.gameObject.SetActive(true);
                pauseMenuCanvas.gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                pauseMenuCanvas.gameObject.SetActive(false);
                foreach (Transform child in pendingItemGrid.transform)
                {
                    Destroy(child.gameObject);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}
