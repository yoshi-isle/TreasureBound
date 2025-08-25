
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinueButton : MonoBehaviour
{
    public Button button;
    public PlayerSaveData playerSaveData;

    void Awake()
    {
        button = GetComponent<Button>();
    }
    void Start()
    {
        button.onClick.AddListener(OnContinueButtonPressed);
    }

    private void OnContinueButtonPressed()
    {
        if (playerSaveData != null)
        {
            GameManager.Instance.CurrentSaveData = playerSaveData;
            SceneManager.LoadScene("Main");
        }
    }
}
