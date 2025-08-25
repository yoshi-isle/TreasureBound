using UnityEngine;

public class Collectable : MonoBehaviour
{
    public string Name;
    public float Weight;

    public void Collect()
    {
        Debug.Log($"Collected {Name}");
        Destroy(gameObject);
    }

}

