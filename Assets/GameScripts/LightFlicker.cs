using UnityEngine;

// =========================================================
//  Dark Sea — Light Flicker
//  Kisi bhi Light component par lagao.
//  Perlin noise se smooth flicker + kabhi kabhi horror stutter.
//
//  Setup:
//   - Light GameObject par ye script lagao
//   - Inspector mein Min/Max Intensity set karo
//   - Hard Flicker Chance: 0 = bilkul nahi, 0.02 = subtle
// =========================================================
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Range")]
    public float minIntensity  = 0.4f;
    public float maxIntensity  = 1.2f;
    public float flickerSpeed  = 3.5f;

    [Header("Horror Stutter")]
    [Range(0f, 0.1f)]
    public float hardFlickerChance = 0.015f; // per frame chance of an instant dip

    Light _light;
    float _baseOffset;
    float _stutterTimer;

    void Awake()
    {
        _light = GetComponent<Light>();
        _baseOffset = Random.Range(0f, 100f); // so each light flickers differently
    }

    void Update()
    {
        // Smooth Perlin-noise flicker
        float noise = Mathf.PerlinNoise(_baseOffset + Time.time * flickerSpeed, 0f);
        float target = Mathf.Lerp(minIntensity, maxIntensity, noise);

        // Occasional hard stutter (instant drop to near-zero then recover)
        _stutterTimer -= Time.deltaTime;
        if (_stutterTimer <= 0f)
        {
            if (Random.value < hardFlickerChance)
            {
                _light.intensity = minIntensity * 0.1f;
                _stutterTimer = 0.06f; // recover in 60 ms
                return;
            }
            _stutterTimer = 0.05f;
        }

        _light.intensity = Mathf.Lerp(_light.intensity, target, Time.deltaTime * 12f);
    }
}
