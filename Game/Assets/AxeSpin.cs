using UnityEngine;

public class AxeSpin : MonoBehaviour
{
    public float speed = 2f;
    public float maxAngle = 90f;
    public float offsetAngle = 0f;

    void Update()
    {
        float angle = Mathf.Sin(Time.time * speed) * maxAngle + offsetAngle;
        
        transform.localEulerAngles = new Vector3(angle, 0f, 0f);
    }
}
