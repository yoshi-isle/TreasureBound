using UnityEngine;

public class AggroTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("Player detected by aggro trigger");
            transform.parent.SendMessage("OnPlayerDetected", other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("Player left aggro trigger");
            transform.parent.SendMessage("OnPlayerLost", other.gameObject);
        }
    }
}