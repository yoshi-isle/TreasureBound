using UnityEngine;

public class ExitDungeon : Interactable
{
    public override void Interact()
    {
        base.Interact();
        var inventory = FindAnyObjectByType<Inventory>();
        GameManager.Instance.TriggerOnGameQuit(inventory.Bag);
        gameObject.SetActive(false);
    }
}