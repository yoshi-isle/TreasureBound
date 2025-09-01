using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.OnCollectablePickedUp += HandleCollectablePickedUp;
    }

    [SerializeField]
    public List<Interactable> Bag = new List<Interactable>();

    private void HandleCollectablePickedUp(Interactable collectable)
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