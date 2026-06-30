using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent audio manager — survives scene loads.
// Place audio clips in Assets/Resources/Audio/:
//   bgMusic.mp3      — main menu loop
//   ambience.mp3     — underwater gameplay loop
//   sonarPing.mp3    — sonar fire one-shot
//   breathing.mp3    — breathing loop (volume scales with oxygen danger)
//   enemyAlert.mp3   — enemy approaching one-shot
//
// Everything is optional — missing clips produce no errors.
public class AudioManager : MonoBehaviour
{
    static AudioManager _instance;

    AudioSource _musicSrc;
    AudioSource _ambienceSrc;
    AudioSource _breathingSrc;
    AudioSource _sfxSrc;

    AudioClip _bgMusic;
    AudioClip _ambience;
    AudioClip _sonarPing;
    AudioClip _breathing;
    AudioClip _enemyAlert;

    OxygenSystem _oxygen;
    float _breathCheckTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        if (_instance != null) return;
        var go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<AudioManager>();
    }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;

        _musicSrc    = AddSource(0.55f, true);
        _ambienceSrc = AddSource(0.40f, true);
        _breathingSrc= AddSource(0f,    true);
        _sfxSrc      = AddSource(0.80f, false);

        // Load from Resources (silent if absent)
        _bgMusic    = Resources.Load<AudioClip>("Audio/bgMusic");
        _ambience   = Resources.Load<AudioClip>("Audio/ambience");
        _sonarPing  = Resources.Load<AudioClip>("Audio/sonarPing");
        _breathing  = Resources.Load<AudioClip>("Audio/breathing");
        _enemyAlert = Resources.Load<AudioClip>("Audio/enemyAlert");

        SceneManager.sceneLoaded += OnSceneLoaded;
        SonarSystem.OnPinged     += OnSonarPinged;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SonarSystem.OnPinged     -= OnSonarPinged;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _oxygen = null;
        AudioListener.pause = false; // reset any pause left by GameOver/Win

        bool isGameplay = scene.name.Contains("Level") || scene.name.Contains("DarkSea");

        if (isGameplay)
        {
            // Gameplay: start ambience + breathing only
            StopAll();
            PlayLoop(_ambienceSrc, _ambience);
            PlayLoop(_breathingSrc, _breathing);
        }
        else
        {
            // Menu/non-gameplay: complete silence
            StopAll();
        }
    }

    void Update()
    {
        // Find OxygenSystem lazily
        if (_oxygen == null)
            _oxygen = Object.FindFirstObjectByType<OxygenSystem>();

        // Breathing volume — intensifies as oxygen drops below 60%
        if (_breathingSrc != null && _breathingSrc.isPlaying && _oxygen != null)
        {
            _breathCheckTimer -= Time.deltaTime;
            if (_breathCheckTimer <= 0f)
            {
                _breathCheckTimer = 0.2f;
                float danger = Mathf.Clamp01(1f - _oxygen.currentOxygen / 60f);
                _breathingSrc.volume = danger * 0.75f;
                _breathingSrc.pitch  = 1f + danger * 0.35f; // faster breathing at low oxygen
            }
        }
    }

    void OnSonarPinged(Transform _, float __, bool ___, float ____)
    {
        PlayOneShot(_sonarPing, 0.85f);
    }

    // ── Public static API ────────────────────────────────────────────

    public static void PlayEnemyAlert()
        => _instance?.PlayOneShot(_instance._enemyAlert, 0.7f);

    // Call when Win or GameOver panel appears — stops all loops immediately.
    public static void StopAll()
    {
        if (_instance == null) return;
        _instance._musicSrc?.Stop();
        _instance._ambienceSrc?.Stop();
        _instance._breathingSrc?.Stop();
        _instance._sfxSrc?.Stop();
    }

    // ── Helpers ──────────────────────────────────────────────────────

    AudioSource AddSource(float volume, bool loop)
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.volume = volume;
        src.loop   = loop;
        src.playOnAwake = false;
        src.spatialBlend = 0f; // 2D
        return src;
    }

    void PlayLoop(AudioSource src, AudioClip clip)
    {
        if (src == null || clip == null) return;
        if (src.clip == clip && src.isPlaying) return;
        src.Stop();
        src.clip = clip;
        src.Play();
    }

    void StopMusic()    { _musicSrc?.Stop(); }
    void StopAmbience() { _ambienceSrc?.Stop(); _breathingSrc?.Stop(); }

    void PlayOneShot(AudioClip clip, float vol)
    {
        if (_sfxSrc == null || clip == null) return;
        _sfxSrc.PlayOneShot(clip, vol);
    }
}
