using UnityEngine;

public class RoomComplete : Interactable
{
    public override void Interact()
    {
        base.Interact();
        GameManager.Instance.TriggerOnGameRestart();
        gameObject.SetActive(false);
    }
}