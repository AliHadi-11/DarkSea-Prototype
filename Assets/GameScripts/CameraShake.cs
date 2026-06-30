using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraShake : MonoBehaviour
{
    static CameraShake _instance;
    void OnDestroy() { if (_instance == this) _instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!scene.Contains("Level") && !scene.Contains("DarkSea")) return;
        if (_instance != null) return;
        _instance = new GameObject("CameraShake").AddComponent<CameraShake>();
    }

    // Call from anywhere: CameraShake.Shake(0.25f, 0.08f)
    public static void Shake(float duration = 0.25f, float magnitude = 0.08f)
    {
        if (_instance == null) return;
        _instance.StopAllCoroutines();
        _instance.StartCoroutine(_instance.DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 origin = cam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float damped = magnitude * (1f - elapsed / duration);
            cam.transform.localPosition = new Vector3(
                origin.x + Random.Range(-1f, 1f) * damped,
                origin.y + Random.Range(-1f, 1f) * damped,
                origin.z
            );
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cam.transform.localPosition = origin;
    }
}
