using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string Name;
    [SerializeField]
    public string PromptText;

    public virtual void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }
}