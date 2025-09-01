using UnityEngine;

public class RoomComplete : Interactable
{
    public override void Interact()
    {
        base.Interact();
        var inventory = FindAnyObjectByType<Inventory>();
        GameManager.Instance.TriggerOnLevelComplete(inventory.Bag);
        gameObject.SetActive(false);
    }
}