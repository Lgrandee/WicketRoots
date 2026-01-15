using UnityEngine;
using System.Collections;

public class HousePhone : MonoBehaviour
{
    [Header("Dialogue Content")]
    [SerializeField] private TextAsset scriptFile;

    [Header("Intro Sound Settings")]
    [SerializeField] private AudioClip introSound;
    [SerializeField, Range(0f, 1f)] private float introSoundVolume = 1f;
    [SerializeField, Range(0.5f, 10f)] private float introSoundDuration = 2f;

    [Header("Bubble Settings")]
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0f, 1.8f, 0f);
    [SerializeField] private Vector2 backgroundSize = new Vector2(4.2f, 2.2f); // world units
    [SerializeField] private Color backgroundColor = new Color(0.12f, 0.12f, 0.16f, 0.9f);
    [SerializeField] private Color textColor = new Color(0.92f, 0.92f, 0.92f, 1f);
    [SerializeField, Range(10, 200)] private int textSize = 60; // font size

    [Header("Text Wrapping")]
    [SerializeField, Range(10, 100)] private int maxCharactersPerLine = 40;

    [Header("Auto Advance")]
    [SerializeField] private bool autoAdvance = true;
    [SerializeField, Range(1f, 10f)] private float secondsPerLine = 3f;

    [Header("Trigger Settings")]
    [SerializeField, Range(0.5f, 10f)] private float triggerDelaySeconds = 3f;

    private AudioSource audioSource;
    private bool playerInTrigger = false;
    private float timeInTrigger = 0f;
    private GameObject activeBubble;
    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private TextMesh activeTextMesh;
    private float autoAdvanceTimer = 0f;
    private bool hasTriggered = false;
    private bool dialogueStarted = false;
    private Rigidbody2D playerRigidbody;
    private MonoBehaviour playerMovementScript;

    public TextAsset ScriptFile => scriptFile;

    private void Awake()
    {
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        // Track time player has been in trigger zone
        if (playerInTrigger && !hasTriggered)
        {
            timeInTrigger += Time.deltaTime;
            if (timeInTrigger >= triggerDelaySeconds)
            {
                hasTriggered = true;
                FreezePlayer();
                StartCoroutine(PlayIntroSoundThenDialogue());
            }
        }

        // Auto-advance dialogue if enabled and dialogue has started
        if (autoAdvance && dialogueStarted && activeBubble != null && dialogueLines != null)
        {
            autoAdvanceTimer += Time.deltaTime;
            if (autoAdvanceTimer >= secondsPerLine)
            {
                autoAdvanceTimer = 0f;
                AdvanceDialogue();
            }
        }
    }

    private void LoadDialogueLines()
    {
        if (scriptFile != null)
        {
            dialogueLines = scriptFile.text.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            dialogueLines = new string[0];
        }
    }

    private void AdvanceDialogue()
    {
        autoAdvanceTimer = 0f; // Reset timer
        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            UpdateBubbleText(dialogueLines[currentLineIndex]);
        }
        else
        {
            // End of dialogue - close bubble and unfreeze player
            DespawnBubble();
            currentLineIndex = 0;
            dialogueStarted = false;
            UnfreezePlayer();
        }
    }

    private void SpawnBubble()
    {
        if (activeBubble != null) return;

        LoadDialogueLines();
        if (dialogueLines.Length == 0) return;

        autoAdvanceTimer = 0f; // Reset timer when starting dialogue
        dialogueStarted = true;

        // Stop intro sound when dialogue starts
        StopIntroSound();

        // Create bubble root
        activeBubble = new GameObject("TextBubble");
        var root = activeBubble.transform;
        root.SetParent(transform);
        root.SetPositionAndRotation(transform.position + bubbleOffset, Quaternion.identity);

        // Background via SpriteRenderer (solid color)
        var renderer = activeBubble.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSolidSprite(backgroundColor);
        renderer.sortingOrder = 20;
        // scale 1x1 sprite to desired size
        root.localScale = new Vector3(backgroundSize.x, backgroundSize.y, 1f);

        // Text using legacy TextMesh (keeps dependencies minimal)
        var textObject = new GameObject("DialogueText");
        textObject.transform.SetParent(activeBubble.transform);
        textObject.transform.localPosition = Vector3.zero;
        textObject.transform.localScale = new Vector3(1f / backgroundSize.x, 1f / backgroundSize.y, 1f);

        activeTextMesh = textObject.AddComponent<TextMesh>();
        activeTextMesh.text = WrapText(dialogueLines[currentLineIndex]);
        activeTextMesh.fontSize = textSize;
        activeTextMesh.characterSize = 0.01f; // fixed small value
        activeTextMesh.anchor = TextAnchor.MiddleCenter;
        activeTextMesh.alignment = TextAlignment.Center;
        activeTextMesh.color = textColor;

        var meshRenderer = textObject.GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = 21;
    }

    private void UpdateBubbleText(string newText)
    {
        if (activeTextMesh != null)
        {
            activeTextMesh.text = WrapText(newText);
        }
    }

    private void DespawnBubble()
    {
        if (activeBubble != null)
        {
            Destroy(activeBubble);
            activeBubble = null;
            activeTextMesh = null;
            // Sound stays stopped after player pressed E
        }
    }

    private string WrapText(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxCharactersPerLine)
            return text;

        var words = text.Split(' ');
        var wrappedText = "";
        var currentLine = "";

        foreach (var word in words)
        {
            // Check if adding this word would exceed the limit
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            
            if (testLine.Length <= maxCharactersPerLine)
            {
                currentLine = testLine;
            }
            else
            {
                // Add current line to result and start new line
                if (!string.IsNullOrEmpty(wrappedText))
                    wrappedText += "\n";
                wrappedText += currentLine;
                currentLine = word;
            }
        }

        // Add the last line
        if (!string.IsNullOrEmpty(currentLine))
        {
            if (!string.IsNullOrEmpty(wrappedText))
                wrappedText += "\n";
            wrappedText += currentLine;
        }

        return wrappedText;
    }

    private Sprite CreateSolidSprite(Color color)
    {
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    // Using 2D colliders - set them as triggers on your objects
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            playerInTrigger = true;
            timeInTrigger = 0f;
            
            // Store player reference for later use
            playerRigidbody = other.GetComponent<Rigidbody2D>();
            
            // Try to find and disable player movement script
            // Common script names - add your own if different
            playerMovementScript = other.GetComponent<MonoBehaviour>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Reset timer if player leaves before trigger activates
            if (!hasTriggered)
            {
                playerInTrigger = false;
                timeInTrigger = 0f;
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        DespawnBubble();
        StopIntroSound();
    }

    private IEnumerator PlayIntroSoundThenDialogue()
    {
        // Play the intro sound effect
        if (introSound != null && audioSource != null)
        {
            audioSource.clip = introSound;
            audioSource.loop = false;
            audioSource.volume = introSoundVolume;
            audioSource.Play();
        }

        // Wait for the specified duration (2 seconds by default)
        yield return new WaitForSeconds(introSoundDuration);

        // Stop the sound and start dialogue
        StopIntroSound();
        SpawnBubble();
    }

    private void StopIntroSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void FreezePlayer()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void UnfreezePlayer()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
