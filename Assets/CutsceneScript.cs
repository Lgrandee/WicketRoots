using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneScript : MonoBehaviour
{
    [Header("Dialogue Content")]
    [SerializeField] private TextAsset scriptFile;

    [Header("Scene Transition")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float delayBeforeSceneLoad = 0.5f;

    [Header("Interaction")]
    [SerializeField] private KeyCode startKey = KeyCode.E;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private bool allowSkip = true;

    [Header("Bubble Settings")]
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0f, 1.8f, 0f);
    [SerializeField] private Vector2 backgroundSize = new Vector2(4.2f, 2.2f);
    [SerializeField] private Color backgroundColor = new Color(0.12f, 0.12f, 0.16f, 0.9f);
    [SerializeField] private Color textColor = new Color(0.92f, 0.92f, 0.92f, 1f);
    [SerializeField, Range(10, 500)] private int textSize = 60;

    [Header("Prompt Settings")]
    [SerializeField] private string promptText = "Press E to start";
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private Color promptColor = new Color(1f, 1f, 0.5f, 1f);

    [Header("Text Wrapping")]
    [SerializeField, Range(10, 100)] private int maxCharactersPerLine = 40;

    [Header("Auto Advance")]
    [SerializeField] private bool autoAdvance = true;
    [SerializeField, Range(1f, 10f)] private float secondsPerLine = 3f;

    private GameObject activeBubble;
    private GameObject promptBubble;
    private bool cutsceneStarted = false;
    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private TextMesh activeTextMesh;
    private float autoAdvanceTimer = 0f;

    public TextAsset ScriptFile => scriptFile;

    private void Start()
    {
        // Show prompt to start cutscene
        SpawnPrompt();
    }

    private void Update()
    {
        // Wait for E to start the cutscene
        if (!cutsceneStarted && Input.GetKeyDown(startKey))
        {
            cutsceneStarted = true;
            DespawnPrompt();
            SpawnBubble();
        }

        // Allow skipping to next line with key press
        if (allowSkip && activeBubble != null && Input.GetKeyDown(skipKey))
        {
            AdvanceDialogue();
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

        // Keep bubble positioned
        if (activeBubble != null)
            UpdateBubblePosition();
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
        autoAdvanceTimer = 0f;
        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            UpdateBubbleText(dialogueLines[currentLineIndex]);
        }
        else
        {
            // End of dialogue - load the next scene
            DespawnBubble();
            currentLineIndex = 0;
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneAfterDelay());
        }
        else
        {
            Debug.LogWarning("CutsceneScript: No scene specified to load after cutscene!");
        }
    }

    private System.Collections.IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeSceneLoad);
        SceneManager.LoadScene(sceneToLoad);
    }

    private void SpawnBubble()
    {
        if (activeBubble != null) return;

        LoadDialogueLines();
        if (dialogueLines.Length == 0) return;

        autoAdvanceTimer = 0f;

        // Create bubble root
        activeBubble = new GameObject("TextBubble");
        var root = activeBubble.transform;
        root.SetParent(transform);
        root.SetPositionAndRotation(transform.position + bubbleOffset, Quaternion.identity);

        // Background via SpriteRenderer (solid color)
        var renderer = activeBubble.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSolidSprite(backgroundColor);
        renderer.sortingOrder = 20;
        root.localScale = new Vector3(backgroundSize.x, backgroundSize.y, 1f);

        // Text using legacy TextMesh
        var textObject = new GameObject("DialogueText");
        textObject.transform.SetParent(activeBubble.transform);
        textObject.transform.localPosition = Vector3.zero;
        textObject.transform.localScale = new Vector3(1f / backgroundSize.x, 1f / backgroundSize.y, 1f);

        activeTextMesh = textObject.AddComponent<TextMesh>();
        activeTextMesh.text = WrapText(dialogueLines[currentLineIndex]);
        activeTextMesh.fontSize = textSize;
        activeTextMesh.characterSize = 0.01f;
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
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            
            if (testLine.Length <= maxCharactersPerLine)
            {
                currentLine = testLine;
            }
            else
            {
                if (!string.IsNullOrEmpty(wrappedText))
                    wrappedText += "\n";
                wrappedText += currentLine;
                currentLine = word;
            }
        }

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

    private void OnDisable()
    {
        DespawnBubble();
        DespawnPrompt();
    }
}
