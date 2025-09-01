using UnityEngine;

public class AxeSpin : MonoBehaviour
{
    public int speed = 100;
    void Update()
    {
        transform.Rotate(Vector3.right, speed * Time.deltaTime);
    }
}
