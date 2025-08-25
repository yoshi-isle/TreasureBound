using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public Button m_NewGame, m_ContinueButton, m_BackButton;
    public TextMeshProUGUI continueText;
    bool saveFileExists = false;
    List<PlayerSaveData> playerSaveDatas = new List<PlayerSaveData>();
    public GameObject mainCanvas, fileSelectCanvas, fileSelectGrid;
    public GameObject LoadFilePrefab;

    void Start()
    {
        m_NewGame.onClick.AddListener(OnNewGameButtonPressed);
        m_ContinueButton.onClick.AddListener(OnContinueButtonPressed);
        m_BackButton.onClick.AddListener(OnBackButtonPressed);
        if (TryToFindSaveFiles())
        {
            continueText.color = Color.white;
            saveFileExists = true;
        }
    }

    private void OnBackButtonPressed()
    {
        mainCanvas.SetActive(true);
        fileSelectCanvas.SetActive(false);
        foreach (Transform child in fileSelectGrid.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnContinueButtonPressed()
    {
        if (!saveFileExists) return;
        mainCanvas.SetActive(false);
        fileSelectCanvas.SetActive(true);
        foreach (var item in playerSaveDatas)
        {
            var obj = Instantiate(LoadFilePrefab, fileSelectGrid.transform);
            obj.GetComponent<ContinueButton>().playerSaveData = item;
            obj.GetComponentInChildren<TextMeshProUGUI>().text = $"Play\n{item.FileName}";
        }
    }

    public void OnNewGameButtonPressed()
    {
        var fileName = "savefile-" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string json = JsonUtility.ToJson(new PlayerSaveData(fileName));
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + fileName + ".json", json);
        GameManager.Instance.CurrentSaveData = new PlayerSaveData(fileName);
        SceneManager.LoadScene("Main");
    }

    bool TryToFindSaveFiles()
    {
        string[] files = System.IO.Directory.GetFiles(Application.persistentDataPath, "savefile-*.json");
        foreach (string file in files)
        {
            Debug.Log("Found save file: " + file);
            string json = System.IO.File.ReadAllText(file);
            PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(json);
            playerSaveDatas.Add(saveData);
        }
        return files.Length > 0;
    }

}
