using UnityEngine;
using UnityEngine.SceneManagement;

// Procedurally configures a ParticleSystem to simulate floating underwater
// dust/micro-bubbles. Auto-spawns in gameplay scenes via RuntimeInitialize.
[RequireComponent(typeof(ParticleSystem))]
public class UnderwaterAtmosphere : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        // Only spawn in gameplay scenes
        if (!sceneName.Contains("Level") && !sceneName.Equals("DarkSea")) return;

        var go = new GameObject("[UnderwaterAtmosphere]");
        go.AddComponent<ParticleSystem>();
        go.AddComponent<UnderwaterAtmosphere>();
    }

    ParticleSystem _ps;
    Transform      _cam;

    void Awake()
    {
        _ps  = GetComponent<ParticleSystem>();
        _cam = Camera.main?.transform;
        Configure();
    }

    void LateUpdate()
    {
        // Particle system follows the camera so bubbles always surround the player
        if (_cam != null)
            transform.position = _cam.position;
    }

    void Configure()
    {
        // ── Main ──────────────────────────────────────────────────────
        var main          = _ps.main;
        main.loop         = true;
        main.duration     = 5f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(7f, 14f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.03f, 0.15f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.012f, 0.048f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(0.18f, 0.60f, 0.75f, 0.08f),
            new Color(0.10f, 0.38f, 0.52f, 0.18f));
        main.gravityModifier  = -0.012f;   // slight upward float
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.maxParticles     = 180;

        // ── Emission ──────────────────────────────────────────────────
        var emission            = _ps.emission;
        emission.enabled        = true;
        emission.rateOverTime   = 14f;

        // ── Shape — sphere around camera ─────────────────────────────
        var shape               = _ps.shape;
        shape.enabled           = true;
        shape.shapeType         = ParticleSystemShapeType.Sphere;
        shape.radius            = 8f;
        shape.radiusThickness   = 1f;

        // ── Velocity over lifetime — gentle random drift ──────────────
        var vel         = _ps.velocityOverLifetime;
        vel.enabled     = true;
        vel.space       = ParticleSystemSimulationSpace.World;
        vel.x           = new ParticleSystem.MinMaxCurve(-0.035f, 0.035f);
        vel.y           = new ParticleSystem.MinMaxCurve(0.025f, 0.095f);
        vel.z           = new ParticleSystem.MinMaxCurve(-0.035f, 0.035f);

        // ── Color over lifetime — fade in then fade out ───────────────
        var col         = _ps.colorOverLifetime;
        col.enabled     = true;
        var grad        = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(new Color(0.18f, 0.60f, 0.76f), 0f),
                new GradientColorKey(new Color(0.12f, 0.42f, 0.58f), 1f)
            },
            new[] {
                new GradientAlphaKey(0f,    0f),
                new GradientAlphaKey(0.22f, 0.12f),
                new GradientAlphaKey(0.22f, 0.88f),
                new GradientAlphaKey(0f,    1f)
            });
        col.color       = new ParticleSystem.MinMaxGradient(grad);

        // ── Size over lifetime — small grow-in / shrink-out ───────────
        var sizeLT      = _ps.sizeOverLifetime;
        sizeLT.enabled  = true;
        var sc          = new AnimationCurve();
        sc.AddKey(new Keyframe(0f,    0f,   0f, 4f));
        sc.AddKey(new Keyframe(0.15f, 1f,   0f, 0f));
        sc.AddKey(new Keyframe(0.88f, 1f,   0f, 0f));
        sc.AddKey(new Keyframe(1f,    0f,   -4f, 0f));
        sizeLT.size = new ParticleSystem.MinMaxCurve(1f, sc);

        // ── Renderer ──────────────────────────────────────────────────
        var rend          = _ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode   = ParticleSystemRenderMode.Billboard;
        rend.sortingOrder = 10;
        rend.minParticleSize = 0.001f;
        rend.maxParticleSize = 0.05f;

        // Try URP particle shader; fall back gracefully
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        if (shader != null)
        {
            var mat = new Material(shader);
            mat.SetFloat("_Mode", 2f);                         // Fade blend mode
            mat.renderQueue = 3000;
            rend.material   = mat;
        }

        _ps.Play();
    }
}
