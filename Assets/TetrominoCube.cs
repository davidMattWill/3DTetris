using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TetrominoCube : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isColliding = false;
    private Camera mainCamera;

    private Material cube_material;
    void Start()
    {
        //get the camera, which we'll need for shading based on distance to camera
        mainCamera = Camera.main;

        //get the material property of the prefab
        cube_material = GetComponent<Renderer>().material;

    }

    // Update is called once per frame
    void Update()
    {
        //every update check if current tetromino cube is colliding
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("No main camera found for depth shading!");
                return;
            }
        }
        isColliding = IsColliding();
        shadeDepth();
    }

    public void SetInteriorColor(Color newColor)
    {
        if (cube_material == null)
        {
            Debug.LogWarning($"No Renderer or material found in tetromino_interior of {transform.name}!");
            return;
        }
        // Set the material color
        cube_material.color = newColor;
    }

    //going to replace this with something else
    private bool IsColliding()
    {
        Collider collider = GetComponent<Collider>();
        if (!collider.enabled || collider.isTrigger) return false;
        // Use OverlapBox to check for collisions at the collider's position
        Vector3 center = collider.bounds.center;
        Vector3 halfExtents = collider.bounds.extents; //correcting collision for slight overlap
        Quaternion orientation = Quaternion.identity; // Assume axis-aligned boxes
        int tetrominoLayerMask = LayerMask.GetMask("Tetromino");

        Collider[] hits = Physics.OverlapBox(center, halfExtents, orientation, tetrominoLayerMask);

        // Check if any hit is a different tetromino
        foreach (Collider hit in hits)
        {
            // Ignore the tetromino itself, its children, or other tetromino cubes belonging to the same tetromino
            if (hit.transform == transform || hit.transform.IsChildOf(transform) || hit.transform.IsChildOf(collider.gameObject.transform.parent))
                continue;

            // Confirm the hit is on the Tetromino layer and not the current tetromino
            if (hit.gameObject.layer == LayerMask.NameToLayer("Tetromino"))
            {

                Debug.Log($"Overlap detected with {hit.gameObject.name} at position {center}");
                return true;

            }
        }
        return false;

    }

    private void shadeDepth()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("No main camera found for depth shading!");
                return;
            }
        }
        if (cube_material == null)
        {
            Debug.LogWarning($"No Renderer or material found in tetromino_interior of {transform.name}!");
            return;
        }

        float minDistance = 4.5f; // Distance where color is brightest
        float maxDistance = 5.5f; // Distance where color is darkest


        Vector3 distVector = mainCamera.transform.position - transform.position;
        float distToCamera = new Vector2(distVector.x, distVector.y).magnitude;
 
        
        // Calculate brightness factor (1 = brightest, 0 = darkest)
        float brightness = Mathf.InverseLerp(maxDistance, minDistance, distToCamera);
        brightness = Mathf.Clamp01(brightness); // Ensure brightness is between 0 and 1

        // Get the current color
        Color currentColor = cube_material.color;
        // Convert to HSV to adjust brightness (value)
        Color.RGBToHSV(currentColor, out float h, out float s, out float v);
        // Apply brightness to the value component
        v = brightness;
        // Convert back to RGB
        Color newColor = Color.HSVToRGB(h, s, v);
        // Preserve alpha (in case the material uses transparency)
        newColor.a = currentColor.a;
        // Set the new color using the existing method
        SetInteriorColor(newColor);
    }

}

