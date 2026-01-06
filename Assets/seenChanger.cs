using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class seenChanger : MonoBehaviour
{
    public string sceneToLoad;
    public float fadeDuration = 1f;
    public string destinationSpawnName;

    private float fadeAlpha = 0f;
    private bool isFading = false;
    private Transform playerTransform;
    private bool destroyAfterMove;


    private void OnTriggerEnter2D(Collider2D collistion)
    {
        if(collistion.gameObject.tag == "Player" && !isFading)
        {
            playerTransform = collistion.transform;
            destroyAfterMove = true;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        isFading = true;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeAlpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            yield return null;
        }
        fadeAlpha = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Prefer the cached player reference; fall back to locating by tag if it was recreated in the new scene.
        var player = playerTransform != null ? playerTransform : GameObject.FindGameObjectWithTag("Player")?.transform;

        if (!string.IsNullOrEmpty(destinationSpawnName))
        {
            var target = GameObject.Find(destinationSpawnName);
            if (target != null && player != null)
            {
                player.position = target.transform.position;
            }
        }

        if (destroyAfterMove)
        {
            Destroy(gameObject);
        }
    }

    private void OnGUI()
    {
        if (fadeAlpha > 0)
        {
            GUI.color = new Color(1, 1, 1, fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
