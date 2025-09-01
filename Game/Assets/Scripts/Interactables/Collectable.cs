using UnityEngine;

public class Collectable : Interactable
{
    public float Weight;

    public override void Interact()
    {
        base.Interact();
        Collect();
    }

    public void Collect()
    {
        Debug.Log($"Collected {gameObject.name}");
        GameManager.Instance.TriggerCollectablePickedUp(this);
        Destroy(gameObject);
    }
}

