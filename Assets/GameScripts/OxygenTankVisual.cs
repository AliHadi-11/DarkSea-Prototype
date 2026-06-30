using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Finds all OxygenPickup objects in gameplay scenes, hides the plain cylinder,
// and replaces it with an animated billboard sprite using OxygenTank.png.
// PNG must be at: Assets/Resources/OxygenTank.png
public class OxygenTankVisual : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterCallback()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.Contains("Level") && !scene.name.Contains("DarkSea")) return;
        new GameObject("OxygenTankVisualSetup").AddComponent<OxygenTankVisual>();
    }

    static Sprite _cachedSprite;

    IEnumerator Start()
    {
        yield return null; // wait one frame for all scene objects to init

        // Load or reuse sprite
        if (_cachedSprite == null)
        {
            var tex = Resources.Load<Texture2D>("OxygenTank");
            if (tex == null)
            {
                Debug.LogWarning("[OxygenTankVisual] OxygenTank.png not found in Resources.");
                Destroy(gameObject);
                yield break;
            }
            _cachedSprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        // Apply billboard to every OxygenPickup AND CollectibleTank in the scene
        int count = 0;
        foreach (var pickup in Object.FindObjectsByType<OxygenPickup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (pickup.GetComponent<OxygenTankBillboard>() != null) continue;
            pickup.gameObject.AddComponent<OxygenTankBillboard>().Init(_cachedSprite);
            count++;
        }
        foreach (var tank in Object.FindObjectsByType<CollectibleTank>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (tank.GetComponent<OxygenTankBillboard>() != null) continue;
            tank.gameObject.AddComponent<OxygenTankBillboard>().Init(_cachedSprite);
            count++;
        }

        Destroy(gameObject); // spawner done
    }
}

// ── Per-tank component ─────────────────────────────────────────────────────────
// Attached to each OxygenPickup: shows animated sprite, hides cylinder mesh.
public class OxygenTankBillboard : MonoBehaviour
{
    SpriteRenderer _sr;
    SpriteRenderer _glowSr;
    Camera         _cam;
    Vector3        _origin;
    float          _phase;      // random start phase so tanks don't all bob in sync

    public void Init(Sprite sprite)
    {
        // Hide original cylinder render (keep collider for trigger)
        foreach (var mr in GetComponents<MeshRenderer>())
            mr.enabled = false;
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
            mr.enabled = false;

        _cam    = Camera.main;
        _origin = transform.position;
        _phase  = Random.Range(0f, Mathf.PI * 2f);

        // ── Main tank sprite ───────────────────────────────────────────────────
        var mainGo = new GameObject("TankSprite");
        mainGo.transform.SetParent(transform);
        mainGo.transform.localPosition = Vector3.zero;

        _sr        = mainGo.AddComponent<SpriteRenderer>();
        _sr.sprite = sprite;

        // Scale to ~0.9 world units tall regardless of image resolution
        float targetH      = 0.9f;
        float spriteHUnits = sprite.rect.height / sprite.pixelsPerUnit;
        float s            = (spriteHUnits > 0f) ? targetH / spriteHUnits : 0.009f;
        mainGo.transform.localScale = new Vector3(s, s, s);

        // ── Glow ring (slightly larger, teal, low alpha) ───────────────────────
        var glowGo = new GameObject("TankGlow");
        glowGo.transform.SetParent(transform);
        glowGo.transform.localPosition = Vector3.zero;
        glowGo.transform.localScale    = mainGo.transform.localScale * 1.55f;

        _glowSr        = glowGo.AddComponent<SpriteRenderer>();
        _glowSr.sprite = sprite;
        _glowSr.color  = new Color(0.18f, 0.85f, 0.62f, 0.18f);

        // Draw glow behind main sprite
        _glowSr.sortingOrder = _sr.sortingOrder - 1;
    }

    void LateUpdate()
    {
        if (_cam == null) { _cam = Camera.main; return; }

        // ── Billboard: always face camera ─────────────────────────────────────
        Vector3 dir = transform.position - _cam.transform.position;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        // ── Float/bob animation ────────────────────────────────────────────────
        float bob = Mathf.Sin(Time.time * 1.6f + _phase) * 0.12f;
        transform.position = _origin + Vector3.up * bob;

        // ── Glow pulse ────────────────────────────────────────────────────────
        if (_glowSr != null)
        {
            float pulse = 0.14f + Mathf.Sin(Time.time * 2.2f + _phase) * 0.08f;
            _glowSr.color = new Color(0.18f, 0.85f, 0.62f, pulse);
            float glowScale = 1.55f + Mathf.Sin(Time.time * 1.8f + _phase) * 0.06f;
            if (_sr != null)
                _glowSr.transform.localScale = _sr.transform.localScale * glowScale;
        }
    }
}
