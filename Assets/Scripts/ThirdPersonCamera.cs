using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;  
    public Vector3 offset = new Vector3(0, 2, -4);
    public float touchSensitivity = 0.5f;  
    public float verticalRotationLimit = 80f;

    private float yaw = 0f;
    private float pitch = 0f;

    private Vector2 previousTouchPosition;

    private void Start()
    {
    }

    private void LateUpdate()
    {
        HandleCameraRotation();
    }

    private void HandleCameraRotation()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.position.x > Screen.width / 2)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    previousTouchPosition = touch.position;
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 delta = touch.position - previousTouchPosition;
                    yaw += delta.x * touchSensitivity;
                    pitch -= delta.y * touchSensitivity;
                    pitch = Mathf.Clamp(pitch, -verticalRotationLimit, verticalRotationLimit);

                    previousTouchPosition = touch.position;
                }
            }
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;
        transform.position = desiredPosition;

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
