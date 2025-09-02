using UnityEngine.SceneManagement;

public class StartDungeon : Interactable
{
    public override void Interact()
    {
        base.Interact();
        SceneManager.LoadScene("Main");
    }
}