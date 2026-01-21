using UnityEngine;

/// <summary>
/// AutomaticScript handles dialogue display and management in a 2D point-and-click game.
/// It displays text bubbles with dialogue content, optionally plays audio, and can auto-advance through dialogue lines.
/// Players can interact with dialogue via a key press or it can auto-play when they enter a trigger zone.
/// </summary>
public class AutomaticScript : MonoBehaviour
{
    // ===== DIALOGUE CONTENT =====
    [Header("Dialogue Content")]
    [SerializeField] private TextAsset scriptFile;

    // ===== INTERACTION SETTINGS =====
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool autoPlayOnEnter = true;

    // ===== SCREEN POSITIONING =====
    [Header("Dialogue Screen Position")]
    [SerializeField] private Camera mainCamera;
    [SerializeField, Range(0f, 1f)] private float screenX = 0.5f; // center
    [SerializeField, Range(0f, 1f)] private float screenY = 0.2f; // lower-middle
    [SerializeField] private float screenDepth = 10f;

    // ===== DIALOGUE BUBBLE APPEARANCE =====
    [Header("Dialogue Bubble Appearance")]
    [SerializeField] private Vector2 backgroundSize = new Vector2(4.2f, 2.2f);
    [SerializeField] private Color backgroundColor = new Color(0.12f, 0.12f, 0.16f, 0.9f);
    [SerializeField] private Color textColor = new Color(0.92f, 0.92f, 0.92f, 1f);
    [SerializeField, Range(10, 200)] private int textSize = 120;

    // ===== INTERACTION PROMPT =====
    [Header("Prompt Settings")]
    [SerializeField] private string promptText = "Press E to talk";
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private Color promptColor = new Color(1f, 1f, 0.5f, 1f);

    // ===== TEXT FORMATTING =====
    [Header("Text Wrapping")]
    [SerializeField, Range(10, 100)] private int maxCharactersPerLine = 40;

    // ===== AUTO ADVANCE =====
    [Header("Auto Advance")]
    [SerializeField] private bool autoAdvance = true;
    [SerializeField, Range(1f, 10f)] private float secondsPerLine = 3f;

    // ===== AUDIO =====
    [Header("Sound Settings")]
    [SerializeField] private AudioClip dialogueSound;
    [SerializeField, Range(0.1f, 10f)] private float soundDuration = 2f;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;

    // ===== INTERNAL STATE =====
    private GameObject activeBubble;
    private TextMesh activeTextMesh;
    private GameObject promptBubble;
    private AudioSource audioSource;

    private string[] dialogueLines;
    private int currentLineIndex;
    private float autoAdvanceTimer;
    private bool playerInRange;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!autoPlayOnEnter && playerInRange && Input.GetKeyDown(interactKey))
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

        if (autoAdvance && activeBubble != null && dialogueLines != null)
        {
            autoAdvanceTimer += Time.deltaTime;
            if (autoAdvanceTimer >= secondsPerLine)
            {
                autoAdvanceTimer = 0f;
                AdvanceDialogue();
            }
        }

        if (activeBubble != null)
            UpdateBubblePosition();

        if (promptBubble != null)
            UpdatePromptPosition();
    }

    // ===== DIALOGUE =====

    private void LoadDialogueLines()
    {
        dialogueLines = scriptFile != null
            ? scriptFile.text.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
            : new string[0];
    }

    private void SpawnBubble()
    {
        if (activeBubble != null) return;

        LoadDialogueLines();
        if (dialogueLines.Length == 0) return;

        currentLineIndex = 0;
        autoAdvanceTimer = 0f;

        PlayDialogueSound();

        activeBubble = new GameObject("DialogueBubble");
        activeBubble.transform.position = GetScreenPosition();

        var bg = activeBubble.AddComponent<SpriteRenderer>();
        bg.sprite = CreateSolidSprite(backgroundColor);
        bg.sortingOrder = 20;
        activeBubble.transform.localScale = new Vector3(backgroundSize.x, backgroundSize.y, 1f);

        var textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(activeBubble.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localScale = new Vector3(
            1f / backgroundSize.x,
            1f / backgroundSize.y,
            1f
        );

        activeTextMesh = textObj.AddComponent<TextMesh>();
        activeTextMesh.text = WrapText(dialogueLines[currentLineIndex]);
        activeTextMesh.fontSize = textSize;
        activeTextMesh.characterSize = 0.02f;
        activeTextMesh.anchor = TextAnchor.MiddleCenter;
        activeTextMesh.alignment = TextAlignment.Center;
        activeTextMesh.color = textColor;

        textObj.GetComponent<MeshRenderer>().sortingOrder = 21;
    }

    private void AdvanceDialogue()
    {
        autoAdvanceTimer = 0f;
        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            activeTextMesh.text = WrapText(dialogueLines[currentLineIndex]);
        }
        else
        {
            DespawnBubble();
            SpawnPrompt();
        }
    }

    private void UpdateBubblePosition()
    {
        activeBubble.transform.position = GetScreenPosition();
    }

    private Vector3 GetScreenPosition()
    {
        return mainCamera.ViewportToWorldPoint(
            new Vector3(screenX, screenY, screenDepth)
        );
    }

    private void DespawnBubble()
    {
        if (activeBubble != null)
            Destroy(activeBubble);

        activeBubble = null;
        activeTextMesh = null;
    }

    // ===== PROMPT =====

    private void SpawnPrompt()
    {
        if (promptBubble != null) return;

        promptBubble = new GameObject("PromptText");
        promptBubble.transform.SetParent(transform);
        promptBubble.transform.position = transform.position + promptOffset;

        var tm = promptBubble.AddComponent<TextMesh>();
        tm.text = promptText;
        tm.fontSize = 120;
        tm.characterSize = 0.06f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = promptColor;

        promptBubble.GetComponent<MeshRenderer>().sortingOrder = 25;
    }

    private void UpdatePromptPosition()
    {
        promptBubble.transform.position = transform.position + promptOffset;
    }

    private void DespawnPrompt()
    {
        if (promptBubble != null)
            Destroy(promptBubble);

        promptBubble = null;
    }

    // ===== AUDIO =====

    private void PlayDialogueSound()
    {
        if (dialogueSound == null) return;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = dialogueSound;
        audioSource.volume = soundVolume;
        audioSource.Play();
        StartCoroutine(StopSoundAfterDuration());
    }

    private System.Collections.IEnumerator StopSoundAfterDuration()
    {
        yield return new WaitForSeconds(soundDuration);
        if (audioSource != null)
            audioSource.Stop();
    }

    // ===== HELPERS =====

    private string WrapText(string text)
    {
        if (text.Length <= maxCharactersPerLine) return text;

        string result = "";
        string line = "";

        foreach (string word in text.Split(' '))
        {
            if ((line + word).Length > maxCharactersPerLine)
            {
                result += line + "\n";
                line = word + " ";
            }
            else
            {
                line += word + " ";
            }
        }

        return result + line;
    }

    private Sprite CreateSolidSprite(Color color)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
    }

    // ===== TRIGGERS =====

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (autoPlayOnEnter)
            SpawnBubble();
        else
            SpawnPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        DespawnBubble();
        DespawnPrompt();
        currentLineIndex = 0;
    }
}
