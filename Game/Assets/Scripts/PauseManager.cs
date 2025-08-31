using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public enum GameState
    {
        Normal,
        Paused
    }
    public GameState gameState = GameState.Normal;
    public Canvas mainGameCanvas, pauseMenuCanvas, gameOverCanvas;
    public GameObject pendingItemGrid, pendingItemBox;
    public Button m_RestartGame;

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

    void Start()
    {
        GameManager.Instance.OnPlayerDead += HandlePlayerDead;
        m_RestartGame.onClick.AddListener(OnRestartGameButtonPressed);

    }

    private void OnRestartGameButtonPressed()
    {
        GameManager.Instance.TriggerOnGameRestart();
        mainGameCanvas.gameObject.SetActive(true);
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
