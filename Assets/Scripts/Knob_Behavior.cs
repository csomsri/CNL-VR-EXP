using Unity.VisualScripting;
using UnityEngine;

public class KnobController : MonoBehaviour
{
    public GameObject ball;

    public float minSpeed = 0f;
    public float maxSpeed = 10f;

    public float minAngle = 0f;
    public float maxAngle = 270f;
    void UpdateMag(){
        
    }
    void Update()
    {
        float angle = transform.localEulerAngles.z;

        float t = Mathf.InverseLerp(minAngle, maxAngle, angle);

        float speed = Mathf.Lerp(minSpeed, maxSpeed, t);
        /*
        if (ball != null)
        {
            ball.linearVelocity = ball.linearVelocity.normalized * speed;
        }
        */
    }
}