using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform target; // Sleep hier je taxi in
    Vector3 offset;
    public float smoothTime = 0.3f;

    Vector3 velocity = Vector3.zero;
    float distance;

    private void Start()
    {
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = target.position + target.right * offset.x + target.up * offset.y + target.forward * offset.z;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.LookAt(target);
    }
}
