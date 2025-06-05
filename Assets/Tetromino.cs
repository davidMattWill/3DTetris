using UnityEngine;

public class Tetromino : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isColliding = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        isColliding = IsColliding();
    }

    public void SetColor(Color newColor)
    {

        // Get all child tetromino cubes
        foreach (Transform cube in transform)
        {
            // Find the tetromino_interior child within each cube
            Material cube_material = cube.GetComponent<Renderer>().material;
            if (cube_material == null)
            {
                Debug.LogWarning($"No Renderer or material found in tetromino_interior of {cube.name}!");
                continue;
            }
            // Set the material color
            cube_material.color = newColor;
        }
    }
    //checks if any of its child cubes are colliding
    private bool IsColliding()
    {
        foreach(Transform tetrominoCubeTransform in transform)
        {
            GameObject tetronimoCube = tetrominoCubeTransform.gameObject;
            if (tetronimoCube.GetComponent<TetrominoCube>().isColliding)
            {
                return true;
            }

        }
        return false;
    }

    public void disableTetronimoCubes()
    {
        foreach (Transform tetrominoCubeTransform in transform)
        {
            GameObject tetronimoCube = tetrominoCubeTransform.gameObject;
            tetronimoCube.GetComponent<TetrominoCube>().enabled = false;

        }
    }
}
