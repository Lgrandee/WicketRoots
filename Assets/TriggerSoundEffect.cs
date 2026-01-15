using UnityEngine;

public class TriggerSoundEffect : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("The sound effect to play when player enters")]
    public AudioClip soundEffect;
    
    [Tooltip("Should the sound loop until player presses E?")]
    public bool loopSound = true;
    
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Player Settings")]
    [Tooltip("Tag of the player object")]
    public string playerTag = "Player";

    private AudioSource audioSource;
    private bool playerInTrigger = false;
    private bool hasBeenStopped = false;

    void Start()
    {
        // Add AudioSource component if not already attached
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure the audio source
        audioSource.clip = soundEffect;
        audioSource.loop = loopSound;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        // Check if player is in trigger and presses E
        if (playerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            StopSound();
            OnPlayerPressedE();
        }
    }

    // Called when something enters the trigger collider
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !hasBeenStopped)
        {
            playerInTrigger = true;
            PlaySound();
        }
    }

    // For 2D games - Called when something enters the 2D trigger collider
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !hasBeenStopped)
        {
            playerInTrigger = true;
            PlaySound();
        }
    }

    // Called when something exits the trigger collider
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInTrigger = false;
        }
    }

    // For 2D games - Called when something exits the 2D trigger collider
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInTrigger = false;
        }
    }

    void PlaySound()
    {
        if (soundEffect != null && audioSource != null)
        {
            audioSource.Play();
            Debug.Log("Sound effect started playing");
        }
        else
        {
            Debug.LogWarning("No sound effect assigned to TriggerSoundEffect script!");
        }
    }

    void StopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            hasBeenStopped = true;
            Debug.Log("Sound effect stopped");
        }
    }

    // Override this method or add your custom logic here
    void OnPlayerPressedE()
    {
        // Add any additional actions you want to happen when E is pressed
        Debug.Log("Player pressed E - Add your custom actions here!");
        
        // Example: Disable this script after use
        // enabled = false;
        
        // Example: Destroy this game object
        // Destroy(gameObject);
    }

    // Public method to reset the trigger (can be called from other scripts)
    public void ResetTrigger()
    {
        hasBeenStopped = false;
    }
}
