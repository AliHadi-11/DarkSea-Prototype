using UnityEngine;
using UnityEngine.SceneManagement;

// Adds gentle multi-frequency intensity oscillation to lights —
// simulates sunlight refracting through moving water overhead.
// Auto-attaches to all Point/Spot lights in gameplay scenes.
public class UnderwaterLightRipple : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoAttach()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Level") && !sceneName.Equals("DarkSea")) return;

        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include))
        {
            if ((light.type == LightType.Point || light.type == LightType.Spot) &&
                light.GetComponent<UnderwaterLightRipple>() == null)
            {
                light.gameObject.AddComponent<UnderwaterLightRipple>();
            }
        }
    }

    Light _light;
    float _base;
    float _phase;
    float _f1, _f2, _f3;   // three overlapping frequencies
    float _a1, _a2, _a3;   // amplitudes (small — subtle effect)

    void Awake()
    {
        _light = GetComponent<Light>();
        if (_light == null) { enabled = false; return; }

        _base  = _light.intensity;
        _phase = Random.Range(0f, Mathf.PI * 2f);

        // Slightly randomise per-light so multiple lights don't pulse in sync
        _f1 = Random.Range(0.8f, 1.3f);
        _f2 = Random.Range(1.9f, 2.6f);
        _f3 = Random.Range(0.35f, 0.55f);

        float spread = _base * 0.08f;   // ripple ≤ 8 % of base intensity
        _a1 = spread * 0.50f;
        _a2 = spread * 0.28f;
        _a3 = spread * 0.22f;
    }

    void Update()
    {
        if (_light == null) return;
        float t = Time.time + _phase;
        _light.intensity = _base
            + Mathf.Sin(t * _f1) * _a1
            + Mathf.Sin(t * _f2) * _a2
            + Mathf.Sin(t * _f3) * _a3;
    }
}
