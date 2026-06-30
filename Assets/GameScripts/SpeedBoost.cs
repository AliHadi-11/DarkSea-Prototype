using UnityEngine;
using UnityEngine.SceneManagement;

// Press [F] to sprint: adds +5 speed for 4 seconds.
// Active only in Level 2 and Level 3 scenes. Auto-spawns — no manual setup needed.
public class SpeedBoost : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!scene.Contains("Level_2") && !scene.Contains("Level_3")) return;

        var go = new GameObject("SpeedBoost");
        go.AddComponent<SpeedBoost>();
    }

    const float BOOST_AMOUNT   = 5f;
    const float BOOST_DURATION = 4f;
    const float COOLDOWN       = 6f;

    PlayerMovementFinal _player;
    float _boostTimer;
    float _cooldownTimer;
    bool  _isBoosting;

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) _player = p.GetComponent<PlayerMovementFinal>();
    }

    void Update()
    {
        if (_player == null) return;

        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.R) && !_isBoosting && _cooldownTimer <= 0f)
            StartBoost();

        if (_isBoosting)
        {
            _boostTimer -= Time.deltaTime;
            if (_boostTimer <= 0f) EndBoost();
        }
    }

    void StartBoost()
    {
        _player.moveSpeed += BOOST_AMOUNT;
        _boostTimer = BOOST_DURATION;
        _isBoosting = true;
        NotificationUI.Show("SPRINT ACTIVE  [R]  [+" + (int)BOOST_AMOUNT + " SPD]", NotificationUI.NotifType.Warning);
    }

    void EndBoost()
    {
        _player.moveSpeed -= BOOST_AMOUNT;
        _isBoosting = false;
        _cooldownTimer = COOLDOWN;
    }
}
