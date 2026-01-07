using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Doortransition : MonoBehaviour
{
    public string sceneToLoad;
    public float fadeDuration = 1f;
    public string spawnPointName = "DoorSpawn";

    private bool playerInRange;
    private float fadeAlpha = 0f;


    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeAlpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            yield return null;
        }
        fadeAlpha = 1f;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject spawn = GameObject.Find(spawnPointName);

        if (player != null && spawn != null)
        {
            player.transform.position = spawn.transform.position;
        }

        fadeAlpha = 0f;
    }

    private void OnGUI()
    {
        if (fadeAlpha > 0)
        {
            GUI.color = new Color(1, 1, 1, fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // Display prompt when player is in range
        if (playerInRange)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;

            string message = "Press E to go outside";
            Vector2 size = style.CalcSize(new GUIContent(message));
            Rect rect = new Rect((Screen.width - size.x) / 2, Screen.height * 0.7f, size.x, size.y);
            
            GUI.Label(rect, message, style);
        }
    }

    private void OnTriggerEnter2D(Collider2D collistion)
    {
        if (collistion.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collistion)
    {
        if (collistion.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
