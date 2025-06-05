using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class GameController : MonoBehaviour
{
    public GameObject[] tetrominoPrefabs; // Assign multiple tetromino prefabs in the Inspector
    private GameObject currentTetromino; // Reference to the instantiated tetromino

    public GameObject gameCube; //assign the GameCube in the inspector
    private Collider gameCubeCollider;

    //public float snapIncrement = 1.5f; // Grid size for snapping (adjust in Inspector)
    private bool isMovingTetromino = false; // Tracks if player is holding touch on tetromino

    private CameraOrbitController cameraOrbitController; // Reference to camera rotation script
    private Camera mainCamera; // Reference to the main camera

    private Vector2 moveStartPos;
    private Vector2 moveDelta;

    private float lastClickTime = 0f;
    private float maxClickTime = 0.5f;

    private AudioSource audioSource;
    public AudioClip soundClip;



    void Start()
    {
        // Get the main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Ensure a camera is tagged as 'MainCamera'.");
            return;
        }

        // Find the CameraOrbitController (assumes it’s on the main camera)
        cameraOrbitController = mainCamera.GetComponent<CameraOrbitController>();
        if (cameraOrbitController == null)
        {
            Debug.LogError("CameraOrbitController not found on Main Camera!");
        }

        // Ensure prefabs array is not empty
        if (tetrominoPrefabs == null || tetrominoPrefabs.Length == 0)
        {
            Debug.LogError("No tetromino prefabs assigned in Inspector!");
            return;
        }

        if(gameCube == null)
        {
            Debug.LogError("No GameCube object assigned in Inspector!");
            return;
        }

        gameCubeCollider = gameCube.GetComponent<Collider>();
        if(gameCubeCollider == null)
        {
            Debug.LogError("Object has no collider!");
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = soundClip;

    }

    void Update()
    {
        // Check if tetromino exists
        if (currentTetromino == null)
        {
            Debug.LogWarning("No active tetromino. Spawning a new one.");
            SpawnTetromino();
            return;
        }
        //NEED TO IMPLEMENT TOUCH INPUT. 
        HandleTouch();
        HandleMouse();
  
  
    }

    private void HandleTouch()
    {
        //implement
    }

    private void HandleMouse()
    {
        //handle moving the tetromino
        if (Input.GetMouseButtonDown(0) && !isMovingTetromino)
        {
            float currentClickTime = Time.time;
            moveStartPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            if (IsTouchOnTetromino(Input.mousePosition) && moveDelta.magnitude > 0.01)
            {
                isMovingTetromino = true;
                if (cameraOrbitController != null)
                {
                    cameraOrbitController.isInputEnabled = false;
                }
                Debug.Log("Mouse down on tetromino: Starting movement");
                lastClickTime = 0f;
                return;
            }

            if(lastClickTime > 0f && currentClickTime  - lastClickTime <= maxClickTime)
            {
                LockTetrominoPosition();
                lastClickTime = 0f;
                return;
            }
            else
            {
                lastClickTime = currentClickTime;
                return;
            }
        }
        if (Input.GetMouseButton(0) && isMovingTetromino)
        {
            MoveTetrominoToTouch(Input.mousePosition);
            return;
        }

        if (Input.GetMouseButton(0) && !isMovingTetromino)
        {
            moveDelta = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - moveStartPos;
            return;
        }

        if (Input.GetMouseButtonUp(0) && isMovingTetromino)
        {
            isMovingTetromino = false;
            if (cameraOrbitController != null)
            {
                cameraOrbitController.isInputEnabled = true;
            }
        
            return;
        }
 

    }

    void SpawnTetromino()
    {
        if (tetrominoPrefabs == null || tetrominoPrefabs.Length == 0)
        {
            Debug.LogError("Tetromino prefabs array is empty or not assigned in Inspector!");
            return;
        }

        // Select a random prefab from the array
        int randomIndex = Random.Range(0, tetrominoPrefabs.Length);
        GameObject selectedPrefab = tetrominoPrefabs[randomIndex];

        // Instantiate tetromino at a starting position (adjust as needed)
        Vector3 spawnPosition = new Vector3(0.05f, 0.05f, 0.05f); // Example spawn point
        currentTetromino = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        currentTetromino.transform.localScale = new Vector3(.1f, .1f, .1f);
        //SetTetrominoColor(currentTetromino, Color.blue);

        Debug.Log("Spawned tetromino: " + selectedPrefab.name + " at: " + spawnPosition);
    }
    bool IsTouchOnTetromino(Vector2 screenPosition)
    {
        if (mainCamera == null || currentTetromino == null)
        {
            Debug.LogWarning("Cannot check touch: Main Camera or current tetromino is null.");
            return false;
        }

        // Cast a ray from the touch position
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Tetromino"); // Only hit objects on the "Tetromino" layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            // Check if the hit object is the tetromino or one of its children
            if (hit.transform.gameObject == currentTetromino || hit.transform.IsChildOf(currentTetromino.transform))
            {
                Debug.Log("Raycast hit: " + hit.transform.gameObject.name + " (part of tetromino)");
                return true;
            }
            Debug.Log("Raycast hit: " + hit.transform.gameObject.name + " (not part of tetromino)");
        }
        else
        {
            Debug.Log("Raycast did not hit any object on Tetromino layer");
        }
        return false;
    }

    public void MoveTetrominoToTouch(Vector2 mousePosition)
    {
        float snapIncrement = 0.1f; // Base increment for snapping
        float offset = 0.05f; // Offset to snap to 0.15, 0.25, 0.35, etc.

        // Define a plane perpendicular to the camera's forward vector, passing through the tetromino
        Plane movementPlane = new Plane(mainCamera.transform.forward, currentTetromino.transform.position);

        // Create a ray from the mouse position
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // Check if ray is nearly parallel to the plane

        // Calculate where the ray intersects the plane
        if (movementPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);
            Debug.Log($"World position before snapping: {worldPosition}, Distance: {distance}");

            // Snap the x and y coordinates to the world-space grid
            float snappedX = Mathf.Round((worldPosition.x - offset) / snapIncrement) * snapIncrement + offset;
            float snappedY = Mathf.Round((worldPosition.y - offset) / snapIncrement) * snapIncrement + offset;
            float snappedZ = Mathf.Round((worldPosition.z - offset) / snapIncrement) * snapIncrement + offset;

            //new position
            Vector3 newPosition = new Vector3(snappedX, snappedY, snappedZ);
            Vector3 positionDelta = newPosition - currentTetromino.transform.position;
            //check if new position overlaps with existing tetromino
            int tetrominoLayerMask = LayerMask.GetMask("Tetromino");
            //check if any tetromino cube, updated with new position collides.
            //this seems grossly inefficient by the way
            
            foreach(Transform child in currentTetromino.transform)
            {

                Collider child_collider = child.gameObject.GetComponent<Collider>();
                // Use OverlapBox to check for collisions at the collider's position
                Vector3 center = child_collider.bounds.center + positionDelta;
                Vector3 halfExtents = child_collider.bounds.extents; //correcting collision for slight overlap
                Quaternion orientation = Quaternion.identity; // Assume axis-aligned boxes

                Collider[] colliders = Physics.OverlapBox(center, halfExtents, orientation, tetrominoLayerMask);

                foreach (Collider collider in colliders)
                {
                    //if the collision not with itself, or another tetromino cube of the same tetromino
                    if (!collider.transform.IsChildOf(currentTetromino.transform))
                    {
                        return;
                    }
                }
            }
            //update position and play move sound/
            if(currentTetromino.transform.position != newPosition)
            {
                currentTetromino.transform.position = newPosition;
                audioSource.PlayOneShot(soundClip);
                /*
                if (audioSource != null && soundClip != null && !audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(soundClip);
                }
                else if (audioSource == null || soundClip == null)
                {
                    Debug.LogWarning("AudioSource or soundClip is not assigned!");
                }
                */
            }
 

        }
        else
        {
            Debug.LogWarning("Ray did not intersect the movement plane or distance too large.");
        }
    }


    private void LockTetrominoPosition()
    {
        Debug.Log("Locking Tetromino");
        //check for collision. If no collision with another tetromino then we lock position and change color/play effect
        if (currentTetromino.GetComponent<Tetromino>().isColliding)
        {
            return;
        }
        //if no collisions found. good place to lock position
        currentTetromino.GetComponent<Tetromino>().SetColor(Color.green);
        currentTetromino.GetComponent<Tetromino>().disableTetronimoCubes();
        currentTetromino.GetComponent<Tetromino>().enabled = false;
        currentTetromino = null;
    }
}