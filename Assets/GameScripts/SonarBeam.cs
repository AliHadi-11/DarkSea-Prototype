using System.Collections;
using UnityEngine;

// Renders a laser beam between two world-space points and fades it out.
// Usage: SonarBeam.Shoot(from, to, color) — no scene setup required.
public class SonarBeam : MonoBehaviour
{
    static SonarBeam _instance;
    void OnDestroy() { if (_instance == this) _instance = null; }

    static SonarBeam Get()
    {
        if (_instance != null) return _instance;
        var go = new GameObject("SonarBeamRunner");
        return _instance = go.AddComponent<SonarBeam>();
    }

    public static readonly Color PlayerColor = new Color(0.20f, 1.00f, 0.55f, 1f); // green-cyan
    public static readonly Color EnemyColor  = new Color(1.00f, 0.10f, 0.06f, 1f); // blood-red

    public static void Shoot(Vector3 from, Vector3 to, Color color)
    {
        var inst = Get();
        if (inst != null) inst.StartCoroutine(inst.BeamRoutine(from, to, color));
    }

    // ── Beam coroutine ────────────────────────────────────────────────────────

    IEnumerator BeamRoutine(Vector3 from, Vector3 to, Color color)
    {
        const float DURATION = 0.50f;
        const float OUTER_W  = 0.22f;
        const float INNER_W  = 0.07f;

        Material lineMat = MakeLineMat(color);
        Material coreMat = MakeLineMat(Color.white);

        // Outer glow beam — hard Destroy as safety net in case coroutine is interrupted
        var goOuter = new GameObject("LaserOuter");
        goOuter.transform.SetParent(transform); // child of runner so destroyed with it
        var lrOuter = MakeLR(goOuter, lineMat, OUTER_W, OUTER_W * 0.25f, from, to);
        Destroy(goOuter, DURATION + 0.2f);

        // Inner white core
        var goInner = new GameObject("LaserInner");
        goInner.transform.SetParent(transform);
        var lrInner = MakeLR(goInner, coreMat, INNER_W, INNER_W * 0.35f, from, to);
        Destroy(goInner, DURATION + 0.2f);

        // Impact flash sphere at target
        var flash = MakeFlash(to, color);

        float t = 0f;
        while (t < DURATION)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / DURATION);
            float inv  = 1f - norm;

            // Shrink width to zero (works with any shader — no alpha transparency needed)
            if (lrOuter != null) lrOuter.widthMultiplier = inv;
            if (lrInner != null) lrInner.widthMultiplier = inv;

            // Also fade via vertex color alpha for shaders that support it
            if (lrOuter != null)
            {
                var c = new Color(color.r, color.g, color.b, inv);
                lrOuter.startColor = c;
                lrOuter.endColor   = c;
            }
            if (lrInner != null)
            {
                var c = new Color(1f, 1f, 1f, inv);
                lrInner.startColor = c;
                lrInner.endColor   = c;
            }

            // Impact sphere: grow then shrink
            if (flash != null)
            {
                float sc = norm < 0.18f
                    ? Mathf.Lerp(0f,   0.80f, norm / 0.18f)
                    : Mathf.Lerp(0.80f, 0f,   (norm - 0.18f) / 0.82f);
                flash.transform.localScale = Vector3.one * sc;
            }

            yield return null;
        }

        if (goOuter != null) Destroy(goOuter);
        if (goInner != null) Destroy(goInner);
        if (flash != null) Destroy(flash);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    // Returns a transparent material that works with LineRenderer in any pipeline.
    // Tries URP Particles first, then Sprites/Default (legacy), then Standard fallback.
    static Material MakeLineMat(Color color)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                 ?? Shader.Find("Particles/Standard Unlit")
                 ?? Shader.Find("Sprites/Default")
                 ?? Shader.Find("Legacy Shaders/Particles/Additive")
                 ?? Shader.Find("Unlit/Color")
                 ?? Shader.Find("Standard");

        var mat = new Material(sh);

        // Set color via every possible property — at least one will work
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     color);
        if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", color);
        try { mat.color = color; } catch { }

        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return mat;
    }

    static LineRenderer MakeLR(GameObject go, Material mat,
                               float startW, float endW,
                               Vector3 from, Vector3 to)
    {
        var lr = go.AddComponent<LineRenderer>();
        lr.sharedMaterial    = mat;
        lr.useWorldSpace     = true;
        lr.positionCount     = 2;
        lr.numCapVertices    = 6;
        lr.startWidth        = startW;
        lr.endWidth          = endW;
        lr.startColor        = mat.color;
        lr.endColor          = mat.color;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows    = false;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        return lr;
    }

    static GameObject MakeFlash(Vector3 pos, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "LaserFlash";
        go.transform.position   = pos;
        go.transform.localScale = Vector3.zero;
        Destroy(go.GetComponent<Collider>());

        var mr = go.GetComponent<MeshRenderer>();
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit")
                 ?? Shader.Find("Unlit/Color")
                 ?? Shader.Find("Standard");
        var mat = new Material(sh);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     color);
        try { mat.color = color; } catch { }
        mr.sharedMaterial    = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows    = false;
        return go;
    }
}
