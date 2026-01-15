using UnityEngine;

/// <summary>
/// AutomaticScript handles dialogue display and management in a game world.
/// It displays text bubbles with dialogue content, optionally plays audio, and can auto-advance through dialogue lines.
/// Players can interact with dialogue via a key press or it can auto-play when they enter a trigger zone.
/// </summary>
public class AutomaticScript : MonoBehaviour
{
    // ===== DIALOGUE CONTENT =====
    [Header("Dialogue Content")]
    [SerializeField] private TextAsset scriptFile; // Text file containing dialogue lines (one line per line in the file)

    // ===== INTERACTION SETTINGS =====
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E; // Key required to advance dialogue manually
    [SerializeField] private bool autoPlayOnEnter = true; // If true, dialogue starts automatically when player enters; if false, player must press key

    // ===== DIALOGUE BUBBLE APPEARANCE =====
    [Header("Bubble Settings")]
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0f, 1.8f, 0f); // Position offset above the character
    [SerializeField] private Vector2 backgroundSize = new Vector2(4.2f, 2.2f); // Bubble dimensions in world units
    [SerializeField] private Color backgroundColor = new Color(0.12f, 0.12f, 0.16f, 0.9f); // Background color (dark blue-grey)
    [SerializeField] private Color textColor = new Color(0.92f, 0.92f, 0.92f, 1f); // Text color (light grey)
    [SerializeField, Range(10, 200)] private int textSize = 60; // Font size for dialogue text

    // ===== INTERACTION PROMPT DISPLAY =====
    [Header("Prompt Settings")]
    [SerializeField] private string promptText = "Press E to talk"; // Text shown above character prompting player to interact
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 2.5f, 0f); // Position where prompt appears relative to character
    [SerializeField] private Color promptColor = new Color(1f, 1f, 0.5f, 1f); // Prompt text color (yellow)

    // ===== TEXT FORMATTING =====
    [Header("Text Wrapping")]
    [SerializeField, Range(10, 100)] private int maxCharactersPerLine = 40; // Maximum characters per line before wrapping to next line

    // ===== AUTO-ADVANCE SETTINGS =====
    [Header("Auto Advance")]
    [SerializeField] private bool autoAdvance = true; // If true, dialogue advances automatically after delay
    [SerializeField, Range(1f, 10f)] private float secondsPerLine = 3f; // How long to display each dialogue line before advancing

    // ===== AUDIO SETTINGS =====
    [Header("Sound Settings")]
    [SerializeField] private AudioClip dialogueSound; // Audio clip to play when dialogue starts
    [SerializeField, Range(0.1f, 10f)] private float soundDuration = 2f; // How long to play the sound before stopping
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f; // Volume of the dialogue sound (0-1)

    // ===== INTERNAL STATE =====
    private GameObject activeBubble; // Currently active dialogue bubble (null if none)
    private AudioSource audioSource; // Audio source component for playing dialogue sound
    private GameObject promptBubble; // Prompt text shown to encourage interaction
    private bool playerInRange; // Whether player is currently in trigger area
    private string[] dialogueLines; // Array of dialogue lines loaded from text file
    private int currentLineIndex = 0; // Index of current line being displayed
    private TextMesh activeTextMesh; // Text mesh component of the dialogue bubble
    private float autoAdvanceTimer = 0f; // Timer for auto-advancing to next line

    public TextAsset ScriptFile => scriptFile;

    private void Update()
    {
        if (!autoPlayOnEnter)
        {
            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                if (activeBubble == null)
                {
                    SpawnBubble();
                    DespawnPrompt();
                }
                else
                {
                    AdvanceDialogue();
                }
            }
        }
        else if (autoPlayOnEnter && playerInRange)
        {
            if (activeBubble != null)
                UpdateBubblePosition();
        }

        // Auto-advance dialogue if enabled
        if (autoAdvance && activeBubble != null && dialogueLines != null)
        {
            autoAdvanceTimer += Time.deltaTime;
            if (autoAdvanceTimer >= secondsPerLine)
            {
                autoAdvanceTimer = 0f;
                AdvanceDialogue();
            }
        }

        // Update positions if they exist
        if (promptBubble != null)
            UpdatePromptPosition();
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
            // End of dialogue - close bubble and show prompt again
            DespawnBubble();
            currentLineIndex = 0;
            SpawnPrompt();
        }
    }

    private void SpawnBubble()
    {
        if (activeBubble != null) return;

        LoadDialogueLines();
        if (dialogueLines.Length == 0) return;

        autoAdvanceTimer = 0f; // Reset timer when starting dialogue

        // Play dialogue sound for a couple seconds
        PlayDialogueSound();

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

    private void UpdateBubblePosition()
    {
        if (activeBubble != null)
            activeBubble.transform.position = transform.position + bubbleOffset;
    }

    private void SpawnPrompt()
    {
        if (promptBubble != null) return;

        promptBubble = new GameObject("PromptText");
        promptBubble.transform.SetParent(transform);
        promptBubble.transform.position = transform.position + promptOffset;

        var textMesh = promptBubble.AddComponent<TextMesh>();
        textMesh.text = promptText;
        textMesh.fontSize = 80;
        textMesh.characterSize = 0.05f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = promptColor;

        var meshRenderer = promptBubble.GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = 25;
    }

    private void UpdatePromptPosition()
    {
        if (promptBubble != null)
            promptBubble.transform.position = transform.position + promptOffset;
    }

    private void DespawnPrompt()
    {
        if (promptBubble != null)
        {
            Destroy(promptBubble);
            promptBubble = null;
        }
    }

    private void PlayDialogueSound()
    {
        if (dialogueSound == null) return;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = dialogueSound;
        audioSource.volume = soundVolume;
        audioSource.Play();

        // Stop the sound after the specified duration
        StartCoroutine(StopSoundAfterDuration());
    }

    private System.Collections.IEnumerator StopSoundAfterDuration()
    {
        yield return new WaitForSeconds(soundDuration);
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void DespawnBubble()
    {
        if (activeBubble != null)
        {
            Destroy(activeBubble);
            activeBubble = null;
            activeTextMesh = null;
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
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!autoPlayOnEnter && activeBubble == null)
                SpawnPrompt();
            else if (autoPlayOnEnter && activeBubble == null)
                SpawnBubble(); // Auto-start dialogue when player walks through
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            DespawnPrompt();
            DespawnBubble();
            currentLineIndex = 0; // Reset dialogue progress when player leaves
        }
    }

    // Using 3D colliders - set them as triggers on your objects
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!autoPlayOnEnter && activeBubble == null)
                SpawnPrompt();
            else if (autoPlayOnEnter && activeBubble == null)
                SpawnBubble(); // Auto-start dialogue when player walks through
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            DespawnPrompt();
            DespawnBubble();
            currentLineIndex = 0; // Reset dialogue progress when player leaves
        }
    }

    private void OnDisable()
    {
        DespawnBubble();
        DespawnPrompt();
    }
}
