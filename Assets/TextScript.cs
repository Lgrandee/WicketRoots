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
    [SerializeField, Range(0.02f, 0.2f)] private float characterSize = 0.08f;

    [Header("Prompt Settings")]
    [SerializeField] private string promptText = "Press E to talk";
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private Color promptColor = new Color(1f, 1f, 0.5f, 1f);

    private GameObject activeBubble;
    private GameObject promptBubble;
    private bool playerInRange;

    public TextAsset ScriptFile => scriptFile;

    private void Update()
    {
        if (triggerOnKeyPress)
        {
            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                if (activeBubble == null)
                {
                    SpawnBubble(GetDialogueText());
                    DespawnPrompt();
                }
                else
                {
                    DespawnBubble();
                    SpawnPrompt();
                }
            }
        }
        else
        {
            if (activeBubble == null)
                SpawnBubble(GetDialogueText());
            else
                UpdateBubblePosition();
        }

        // Update positions if they exist
        if (promptBubble != null)
            UpdatePromptPosition();
    }

    private string GetDialogueText()
    {
        return scriptFile != null ? scriptFile.text : string.Empty;
    }

    private void SpawnBubble(string textContent)
    {
        if (activeBubble != null) return;

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
        // place near top-left of background
        textObject.transform.localPosition = new Vector3(-backgroundSize.x * 0.48f, backgroundSize.y * 0.45f, -0.01f);

        var textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = textContent;
        textMesh.fontSize = 100; // base resolution, paired with characterSize
        textMesh.characterSize = characterSize;
        textMesh.anchor = TextAnchor.UpperLeft;
        textMesh.alignment = TextAlignment.Left;
        textMesh.color = textColor;
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
        }
    }

    private Sprite CreateSolidSprite(Color color)
    {
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    // If using 3D colliders set them as triggers on your Player object
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            playerInRange = true;
            if (triggerOnKeyPress && activeBubble == null)
                SpawnPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            playerInRange = false;
            DespawnPrompt();
            DespawnBubble();
        }
    }

    private void OnDisable()
    {
        DespawnBubble();
        DespawnPrompt();
    }
}
