using UnityEngine;
using TMPro;

// =========================================================
//  Dark Sea - Sonar System (Canonical)
//  Player par lagani hai (Player par AudioSource bhi hona chahiye).
//
//  Handles:
//    - Ping sound  (Inspector mein 'Ping Sound' assign karo,
//                   ya AudioSource.clip set karo — dono fallback)
//    - Enemy detection (OverlapSphere, sonarRange)
//    - Enemy kill if within killRange (Space ke waqt)
//    - Wall/obstacle distance (forward Raycast)
//    - UI feedback via GameMessage.Show() — koi Inspector link nahi
//
//  PlayerMovementFinal.cs Space input par FireSonar() call
//  karta hai — wahan koi sonar logic nahi hai.
// =========================================================
[RequireComponent(typeof(AudioSource))]
public class SonarSystem : MonoBehaviour
{
    // AdvancedHUD subscribes to this to drive the radar display.
    // Args: nearestEnemy (null if none), enemyDist, hasWall, wallDist
    public static event System.Action<Transform, float, bool, float> OnPinged;

    [Header("Sonar")]
    public float sonarRange = 20f;
    public AudioClip pingSound;       // Inspector mein assign karo
    public TMP_Text sonarLogText;     // Oxygen text ke paas wala UI text drag karo

    [Header("Enemy Kill")]
    public float killRange = 3.5f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (sonarLogText != null) sonarLogText.text = "Sonar Ready...";
    }

    public void FireSonar()
    {
        PlayPing();

        // 1. Find all enemies in sonar range
        Collider[] hits = Physics.OverlapSphere(transform.position, sonarRange);
        Transform nearestEnemy = null;
        float enemyDist = Mathf.Infinity;

        var toKill = new System.Collections.Generic.List<Transform>();
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float d = Vector3.Distance(transform.position, hit.transform.position);
                if (d < enemyDist) { enemyDist = d; nearestEnemy = hit.transform; }
                toKill.Add(hit.transform);
            }
        }

        // Kill ALL detected enemies on sonar fire
        if (toKill.Count > 0)
        {
            Vector3 shootFrom = transform.position + Vector3.up * 0.8f;
            foreach (var e in toKill)
            {
                if (e == null) continue;
                SonarBeam.Shoot(shootFrom, e.position + Vector3.up * 0.8f, SonarBeam.PlayerColor);
                EnemyDeath.Dissolve(e.gameObject); // animated death instead of instant Destroy
            }

            AchievementSystem.Unlock("FirstElim");

            string msg = toKill.Count > 1
                ? "ELIMINATED " + toKill.Count + " ENEMIES!"
                : "ENEMY ELIMINATED!";
            NotificationUI.Show(msg, NotificationUI.NotifType.Kill);
            OnPinged?.Invoke(null, -1f, false, -1f);
            return;
        }

        // 2. Wall distance via forward raycast
        string wallMsg;
        Vector3 origin = transform.position + Vector3.up * 0.5f + transform.forward * 0.6f;
        RaycastHit rayHit;
        bool hasWall = Physics.Raycast(origin, transform.forward, out rayHit, sonarRange);
        float wallDist = hasWall ? rayHit.distance : -1f;
        if (hasWall)
            wallMsg = "Wall ahead: " + (int)rayHit.distance + "m";
        else
            wallMsg = "Clear ahead (" + (int)sonarRange + "m+)";

        // 3. Enemy warning if detected but out of kill range
        string enemyMsg = nearestEnemy != null ? "   |   Enemy: " + (int)enemyDist + "m" : "";

        if (sonarLogText != null) sonarLogText.text = wallMsg + enemyMsg;

        OnPinged?.Invoke(nearestEnemy, enemyDist < Mathf.Infinity ? enemyDist : -1f, hasWall, wallDist);
    }

    void PlayPing()
    {
        if (audioSource == null) return;

        if (pingSound != null)
            audioSource.PlayOneShot(pingSound);
        else if (audioSource.clip != null)
            audioSource.Play();
        else
            Debug.LogWarning("SonarSystem: No ping sound set. Assign 'Ping Sound' in the Inspector on " + gameObject.name + ".");
    }
}
