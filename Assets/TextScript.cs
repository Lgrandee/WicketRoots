using UnityEngine;

public class TextScript : MonoBehaviour
{
    [Header("Dialogue Content")]
    [SerializeField] private TextAsset scriptFile;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool triggerOnKeyPress = true;

    [Header("Bubble Settings")]
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0f, 1.8f, 0f);
    [SerializeField] private Vector2 backgroundSize = new Vector2(4.2f, 2.2f); // world units
    [SerializeField] private Color backgroundColor = new Color(0.12f, 0.12f, 0.16f, 0.9f);
    [SerializeField] private Color textColor = new Color(0.92f, 0.92f, 0.92f, 1f);
    [SerializeField, Range(10, 500)] private int textSize = 500; // font size

    [Header("Prompt Settings")]
    [SerializeField] private string promptText = "Press E to talk";
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private Color promptColor = new Color(1f, 1f, 0.5f, 1f);

    [Header("Text Wrapping")]
    [SerializeField, Range(10, 100)] private int maxCharactersPerLine = 40;

    [Header("Auto Advance")]
    [SerializeField] private bool autoAdvance = true;
    [SerializeField, Range(1f, 10f)] private float secondsPerLine = 3f;

    private GameObject activeBubble;
    private GameObject promptBubble;
    private bool playerInRange;
    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private TextMesh activeTextMesh;
    private float autoAdvanceTimer = 0f;

    public TextAsset ScriptFile => scriptFile;

    private void Update()
    {
        if (triggerOnKeyPress)
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
        else
        {
            if (activeBubble == null)
                SpawnBubble();
            else
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
            if (triggerOnKeyPress && activeBubble == null)
                SpawnPrompt();
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

    private void OnDisable()
    {
        DespawnBubble();
        DespawnPrompt();
    }
}
