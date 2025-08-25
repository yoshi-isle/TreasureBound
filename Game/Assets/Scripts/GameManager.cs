using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    public event System.Action<Collectible, Vector3> OnCollectibleFocused;
    public event System.Action OnCollectibleUnfocused;

    public void TriggerCollectibleFocused(Collectible collectible, Vector3 hitPoint)
    {
        OnCollectibleFocused?.Invoke(collectible, hitPoint);
    }
    public void TriggerCollectibleUnfocused()
    {
        OnCollectibleUnfocused?.Invoke();
    }
}