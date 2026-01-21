using UnityEngine;

/// <summary>
    /// AutomaticScript handles dialogue display and management in a 2D point-and-click game.
    /// It displays text bubbles with dialogue content, optionally plays audio, and can auto-advance through dialogue lines.
    /// Players can interact with dialogue via a key press or it can auto-play when they enter a trigger zone.
/// </summary>
public class AutomaticScript : MonoBehaviour
{
    [Header("Dialogue Content")]
    [SerializeField] private TextAsset scriptFile;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool autoPlayOnEnter = true;

    [Header("Dialogue Screen Position")]
    [SerializeField] private Camera mainCamera;
    [SerializeField, Range(0f, 1f)] private float screenX = 0.5f;
    [SerializeField, Range(0f, 1f)] private float screenY = 0.2f;

    [Header("Dialogue Bubble")]
    [SerializeField] private Vector2 backgroundSize = new Vector2(10f, 2.5f);
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int textSize = 60;

    [Header("Screen Overlay")]
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.65f);

    [Header("Prompt")]
    [SerializeField] private string promptText = "Press E to talk";
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private Color promptColor = Color.yellow;

    [Header("Text Wrapping")]
    [SerializeField] private int maxCharactersPerLine = 40;

    [Header("Auto Advance")]
    [SerializeField] private bool autoAdvance = true;
    [SerializeField] private float secondsPerLine = 2f;  // <-- fixed to 2 seconds

    private GameObject overlay;
    private GameObject bubble;
    private TextMesh textMesh;
    private GameObject prompt;

    private string[] lines;
    private int index;
    private float timer;
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
            if (bubble == null)
            {
                SpawnDialogue();
                DespawnPrompt();
            }
            else
            {
                NextLine();
            }
        }

        if (autoAdvance && bubble != null)
        {
            timer += Time.deltaTime;
            if (timer >= secondsPerLine)
            {
                timer = 0f;
                NextLine();
            }
        }

        if (bubble != null)
            bubble.transform.position = GetScreenPosition();

        if (overlay != null)
            overlay.transform.position = GetScreenPosition();
    }

    private void SpawnDialogue()
    {
        LoadLines();
        if (lines.Length == 0) return;

        index = 0;
        timer = 0f;

        SpawnOverlay();

        bubble = new GameObject("DialogueBubble");
        bubble.transform.position = GetScreenPosition();

        SpriteRenderer bg = bubble.AddComponent<SpriteRenderer>();
        bg.sprite = CreateSprite(backgroundColor);
        bg.sortingOrder = 10;
        bubble.transform.localScale = new Vector3(backgroundSize.x, backgroundSize.y, 1f);

        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(bubble.transform);
        textObj.transform.localPosition = Vector3.zero;

        textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = WrapText(lines[index]);
        textMesh.fontSize = textSize;
        textMesh.characterSize = 0.03f; // <-- smaller text
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = textColor;

        textObj.GetComponent<MeshRenderer>().sortingOrder = 15;
    }

    private void NextLine()
    {
        index++;

        if (index < lines.Length)
        {
            textMesh.text = WrapText(lines[index]);
        }
        else
        {
            DespawnDialogue();
            SpawnPrompt();
        }
    }

    private void DespawnDialogue()
    {
        if (bubble != null) Destroy(bubble);
        if (overlay != null) Destroy(overlay);

        bubble = null;
        overlay = null;
        textMesh = null;
    }

    private void SpawnOverlay()
    {
        overlay = new GameObject("DialogueOverlay");
        overlay.transform.position = GetScreenPosition();

        SpriteRenderer sr = overlay.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite(overlayColor);
        sr.sortingOrder = 0;

        // Use camera size to fill screen (works for most resolutions)
        float height = mainCamera.orthographicSize * 2f;
        float width = height * mainCamera.aspect;

        overlay.transform.localScale = new Vector3(width, height, 1f);
    }

    private void SpawnPrompt()
    {
        if (prompt != null) return;

        prompt = new GameObject("Prompt");
        prompt.transform.position = transform.position + promptOffset;

        TextMesh tm = prompt.AddComponent<TextMesh>();
        tm.text = promptText;
        tm.fontSize = 80;
        tm.characterSize = 0.05f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = promptColor;

        prompt.GetComponent<MeshRenderer>().sortingOrder = 20;
    }

    private void DespawnPrompt()
    {
        if (prompt != null) Destroy(prompt);
        prompt = null;
    }

    private Vector3 GetScreenPosition()
    {
        Vector3 pos = mainCamera.ViewportToWorldPoint(
            new Vector3(screenX, screenY, 0f)
        );
        pos.z = 0f;
        return pos;
    }

    private void LoadLines()
    {
        lines = scriptFile != null
            ? scriptFile.text.Split('\n')
            : new string[0];
    }

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

    private Sprite CreateSprite(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (autoPlayOnEnter)
            SpawnDialogue();
        else
            SpawnPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        DespawnDialogue();
        DespawnPrompt();
    }
}
