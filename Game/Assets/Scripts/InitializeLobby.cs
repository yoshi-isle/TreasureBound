using UnityEngine;

public class InitializeLobby : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.TriggerOnDungeonGenerated();
    }
}