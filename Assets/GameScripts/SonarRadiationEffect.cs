using UnityEngine;
using UnityEngine.SceneManagement;

// Spawns a particle burst around the player whenever sonar fires (Spacebar).
// Auto-spawns in gameplay scenes — no manual scene setup needed.
public class SonarRadiationEffect : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!scene.Contains("Level") && !scene.Equals("DarkSea")) return;

        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var go = new GameObject("SonarRadiationEffect");
        go.transform.SetParent(player.transform);
        go.transform.localPosition = Vector3.zero;
        go.AddComponent<SonarRadiationEffect>();
    }

    ParticleSystem _burst;

    void Awake()
    {
        _burst = BuildParticleSystem();
        SonarSystem.OnPinged += OnSonarFired;
    }

    void OnDestroy()
    {
        SonarSystem.OnPinged -= OnSonarFired;
    }

    void OnSonarFired(Transform enemy, float eDist, bool hasWall, float wDist)
    {
        if (_burst == null) return;
        _burst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _burst.Play();
    }

    ParticleSystem BuildParticleSystem()
    {
        var go = new GameObject("RadiationBurst");
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Main
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 1.0f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(4f, 10f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 80;

        // Burst emission
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

        // Sphere shape — radiates outward from player
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;

        // Colour: bright green-teal fading to transparent
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(new Color(0.20f, 0.90f, 0.55f), 0f),
                new GradientColorKey(new Color(0.10f, 0.60f, 0.35f), 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // Size shrinks toward end
        var sizeLife = ps.sizeOverLifetime;
        sizeLife.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.7f, 0.6f),
            new Keyframe(1f, 0.05f)
        );
        sizeLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Renderer
        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Legacy Shaders/Particles/Additive");
        if (shader != null)
        {
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", Color.white);
            rend.material = mat;
        }
        rend.sortingOrder = 5;

        return ps;
    }
}
