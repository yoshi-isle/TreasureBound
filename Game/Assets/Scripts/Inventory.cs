using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.OnCollectablePickedUp += HandleCollectablePickedUp;
    }

    [SerializeField]
    public List<Collectable> Bag = new List<Collectable>();

    private void HandleCollectablePickedUp(Collectable collectable)
    {
        if (collectable == null)
        {
            Debug.LogWarning("Tried to add null collectable to bag!");
            return;
        }
        print($"Item added to bag {collectable.Name}");
        Bag.Add(collectable);
    }
}