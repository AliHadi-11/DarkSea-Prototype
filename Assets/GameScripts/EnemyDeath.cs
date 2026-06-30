using System.Collections;
using UnityEngine;

// Plays a dissolve animation on an enemy before destroying it.
// Usage: EnemyDeath.Dissolve(enemyGameObject);
public class EnemyDeath : MonoBehaviour
{
    static EnemyDeath _instance;
    void OnDestroy() { if (_instance == this) _instance = null; }

    static EnemyDeath Get()
    {
        if (_instance != null) return _instance;
        return _instance = new GameObject("EnemyDeathRunner").AddComponent<EnemyDeath>();
    }

    public static void Dissolve(GameObject enemy, System.Action onComplete = null)
    {
        if (enemy == null) { onComplete?.Invoke(); return; }

        // Disable collider + nav agent immediately so sonar doesn't re-detect
        foreach (var col in enemy.GetComponentsInChildren<Collider>())
            col.enabled = false;
        var nav = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav != null) nav.enabled = false;

        Get().StartCoroutine(Get().DissolveRoutine(enemy, onComplete));
    }

    IEnumerator DissolveRoutine(GameObject enemy, System.Action onComplete)
    {
        if (enemy == null) { onComplete?.Invoke(); yield break; }

        var renderers = enemy.GetComponentsInChildren<Renderer>();

        // Cache original colors and create material instances
        var mats = new System.Collections.Generic.List<(Material m, Color orig)>();
        foreach (var r in renderers)
            foreach (var m in r.materials) // r.materials creates instances automatically
                mats.Add((m, GetColor(m)));

        // Flash white 3 times
        for (int i = 0; i < 3; i++)
        {
            foreach (var (m, _) in mats) SetColor(m, Color.white);
            yield return new WaitForSecondsRealtime(0.055f);
            foreach (var (m, orig) in mats) SetColor(m, orig);
            yield return new WaitForSecondsRealtime(0.055f);
        }

        // Scale to zero over 0.28s
        Vector3 startScale = enemy.transform.localScale;
        float elapsed = 0f;
        while (elapsed < 0.28f && enemy != null)
        {
            elapsed += Time.deltaTime;
            float s = 1f - Mathf.SmoothStep(0f, 1f, elapsed / 0.28f);
            enemy.transform.localScale = startScale * s;
            yield return null;
        }

        if (enemy != null) Destroy(enemy);
        AchievementSystem.Unlock("FirstElim");
        onComplete?.Invoke();
    }

    static Color GetColor(Material m)
    {
        if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor");
        if (m.HasProperty("_Color"))     return m.GetColor("_Color");
        return Color.white;
    }

    static void SetColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color"))     m.SetColor("_Color", c);
        try { m.color = c; } catch { }
    }
}
