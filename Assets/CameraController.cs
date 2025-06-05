using UnityEngine;
using System.Collections;

public class CameraOrbitController : MonoBehaviour
{
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private bool isSwiping = false;
    private float minSwipeDistance = 50f; // Minimum swipe distance in pixels
    public bool isRotating = false; // Prevent new rotations during animation
    private float rotationSpeed = 120f; // Degrees per second
    private Vector3 cubeCenter = Vector3.zero; // Cube’s center (fixed at origin)

    public bool isInputEnabled = true;

    void Start()
    {
        // Ensure the camera looks at the cube’s center
        transform.LookAt(cubeCenter);
        Debug.Log("CameraOrbitController initialized on " + gameObject.name);
    }

    void Update()
    {

        if (isRotating || !isInputEnabled) return;

        // Handle touch input for Android
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    isSwiping = true;
                    Debug.Log("Touch started at: " + touchStartPos);
                    break;

                case TouchPhase.Ended:
                    if (isSwiping)
                    {
                        touchEndPos = touch.position;
                        Debug.Log("Touch ended at: " + touchEndPos);
                        DetectSwipe();
                    }
                    isSwiping = false;
                    break;
            }
        }
        // Mouse input for Editor testing
        else if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            isSwiping = true;
            Debug.Log("Mouse down at: " + touchStartPos);
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            touchEndPos = Input.mousePosition;
            Debug.Log("Mouse up at: " + touchEndPos);
            DetectSwipe();
            isSwiping = false;
        }
    }

    void DetectSwipe()
    {
        Vector2 swipeDelta = touchEndPos - touchStartPos;
        if (swipeDelta.magnitude > minSwipeDistance)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;
            Debug.Log("Swipe detected: Delta = " + swipeDelta);

            // Horizontal swipe
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                if (x > 0)
                {
                    Debug.Log("Swiping right");
                    StartCoroutine(RotateCamera(transform.up, -90f)); // Orbit right
                }
                else
                {
                    Debug.Log("Swiping left");
                    StartCoroutine(RotateCamera(transform.up, 90f));  // Orbit left
                }
            }

        }
        else
        {
            Debug.Log("Swipe too short: " + swipeDelta.magnitude);
        }
    }

    IEnumerator RotateCamera(Vector3 axis, float angle)
    {
        isRotating = true;
        Quaternion startRotation = transform.rotation;
        Vector3 startPosition = transform.position;

        // Calculate target rotation
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        Quaternion endRotation = rotation * startRotation;

        // Calculate target position by rotating the position vector around the cube’s center
        Vector3 positionRelativeToCenter = startPosition - cubeCenter;
        Vector3 endPosition = cubeCenter + rotation * positionRelativeToCenter;

        float duration = Mathf.Abs(angle) / rotationSpeed;
        float elapsed = 0f;

        Debug.Log("Starting camera rotation: Axis = " + axis + ", Angle = " + angle);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Interpolate rotation
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            // Interpolate position
            Vector3 currentRelativePos = Vector3.Slerp(positionRelativeToCenter, endPosition - cubeCenter, t);
            transform.position = cubeCenter + currentRelativePos;


            yield return null;
        }

        // Snap to final position and rotation
        transform.position = endPosition;
        transform.rotation = endRotation;
        transform.LookAt(cubeCenter);

        isRotating = false;
        Debug.Log("Camera rotation complete");
    }
}