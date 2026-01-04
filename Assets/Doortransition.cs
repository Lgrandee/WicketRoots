// 4-1-2026 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteraction : MonoBehaviour
{
    public string sceneToLoad = "outside"; // Name of the scene to load
    public GameObject player; // Reference to the player GameObject

    private void Update()
    {
        // Check if the player is near the door and presses the "E" key
        if (player != null && Vector3.Distance(player.transform.position, transform.position) < 2f)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                LoadScene();
            }
        }
    }

    private void LoadScene()
    {
        // Load the specified scene
        SceneManager.LoadScene(sceneToLoad);
    }
}