using UnityEngine;

// =========================================================
//  Dark Sea — Underwater Dust / Particle Fog
//  Scene mein kisi bhi empty GameObject par lagao (ya Camera
//  ke child par). Particle system khud banata hai — koi
//  Inspector setup nahi chahiye.
//
//  Effect: slow-drifting micro-particles (bioluminescent dust)
//  jo player ke aas-paas float karte hain — deep-sea feel.
// =========================================================
public class UnderwaterParticles : MonoBehaviour
{
    [Header("Appearance")]
    public Color particleColor    = new Color(0.4f, 0.8f, 1f, 0.35f); // cyan-white, semi-transparent
    public float particleSize     = 0.04f;
    public int   maxParticles     = 120;
    public float emissionRate     = 18f;
    public float particleLifetime = 7f;
    public float spreadRadius     = 8f;  // sphere radius around this object

    ParticleSystem _ps;

    void Awake()
    {
        _ps = gameObject.AddComponent<ParticleSystem>();
        ConfigureParticleSystem();
    }

    void ConfigureParticleSystem()
    {
        // ── Main module ──────────────────────────────────
        var main = _ps.main;
        main.loop             = true;
        main.startLifetime    = particleLifetime;
        main.startSpeed       = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
        main.startSize        = new ParticleSystem.MinMaxCurve(particleSize * 0.5f, particleSize * 1.5f);
        main.startColor       = particleColor;
        main.maxParticles     = maxParticles;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.gravityModifier  = new ParticleSystem.MinMaxCurve(-0.005f, 0.003f); // almost weightless

        // ── Emission ─────────────────────────────────────
        var emission = _ps.emission;
        emission.rateOverTime = emissionRate;

        // ── Shape: sphere around player ──────────────────
        var shape = _ps.shape;
        shape.enabled      = true;
        shape.shapeType    = ParticleSystemShapeType.Sphere;
        shape.radius       = spreadRadius;
        shape.radiusThickness = 1f; // emit from whole volume

        // ── Velocity over lifetime: gentle random drift ──
        var vel = _ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
        vel.y = new ParticleSystem.MinMaxCurve( 0.01f, 0.06f); // slight upward drift
        vel.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

        // ── Color over lifetime: fade in and fade out ────
        var colorOverLife = _ps.colorOverLifetime;
        colorOverLife.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f,    0f),
                new GradientAlphaKey(0.85f, 0.15f),
                new GradientAlphaKey(0.85f, 0.85f),
                new GradientAlphaKey(0f,    1f)
            }
        );
        colorOverLife.color = new ParticleSystem.MinMaxGradient(gradient);

        // ── Renderer: use built-in particle material ─────
        var renderer = _ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode     = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder   = 0;

        // Assign the default particle material
        var mat = new Material(Shader.Find("Particles/Standard Unlit"));
        if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if (mat.shader != null)
        {
            mat.SetColor("_BaseColor", particleColor);
            renderer.material = mat;
        }
    }
}
