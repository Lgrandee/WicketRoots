using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Hometeleporter : MonoBehaviour
{
    [Header("Destination")]
    [Tooltip("Scene to load when using this door.")]
    public string sceneToLoad;

    [Header("FX")]
    public float fadeDuration = 1f;

    private bool playerInRange;
    private float fadeAlpha;
    private bool isLoading;

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && !isLoading && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        isLoading = true;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeAlpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        fadeAlpha = 0f;
        isLoading = false;
    }

    private void OnGUI()
    {
        if (fadeAlpha > 0f)
        {
            GUI.color = new Color(1f, 1f, 1f, fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // Display prompt when player is in range
        if (playerInRange && !isLoading)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;

            string message = "Press E to go back inside";
            Vector2 size = style.CalcSize(new GUIContent(message));
            Rect rect = new Rect((Screen.width - size.x) / 2, Screen.height * 0.7f, size.x, size.y);
            
            GUI.Label(rect, message, style);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = false;
    }
}
