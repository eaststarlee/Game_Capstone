using UnityEngine;

public class PetLookAtPlayer : MonoBehaviour
{
    private Transform playerTransform;
    
    // Floating effect variables
    public float floatSpeed = 1f; // How fast it floats
    public float floatHeight = 0.5f; // How high it floats
    private Vector3 startPosition;

    void Start()
    {
        // Find the player GameObject by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player GameObject has the 'Player' tag.");
        }
        
        // Store the starting position for the floating effect
        startPosition = transform.position;
    }

    void Update()
    {
        // --- Look at Player ---
        if (playerTransform != null)
        {
            transform.LookAt(playerTransform);
        }

        // --- Floating Effect ---
        // Calculate the new Y position using a sine wave
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
