using UnityEngine;
using UnityEngine.AI;

// =========================================================
//  Dark Sea - FINAL Enemy AI (Mimic)
//  Yeh purani EnemyAI.cs ki jagah lagani hai. Aik hi script
//  teeno levels ke liye kaam karti hai - sirf "Mode" badlo:
//
//   Mode = Passive  -> Level 2: sirf patrol karta hai, player
//                      ko nahi maarta (peeche nahi aata).
//   Mode = Chase    -> Level 1: player ka peecha karta hai.
//   Mode = Hunt     -> Level 3: tezi se peecha + "Mimic" voice
//                      lure clips (Help me / I can hear you)
//                      bajata hai. Oxygen 2x drain level alag
//                      se OxygenSystem mein set hota hai.
//
//  Sab modes mein: Player Space daba kar (qareeb hote hue)
//  enemy ko maar sakta hai (killRange ke andar).
//
//  Setup:
//   - Enemy par NavMeshAgent component lagao.
//   - Scene mein NavMesh "Bake" karo (Window > AI > Navigation).
//   - Passive patrol ke liye "Patrol Points" array mein khaali
//     GameObjects (waypoints) dalo.
//   - Hunt mode ke liye "Lure Clips" mein voice clips dalo aur
//     AudioSource component lagao.
// =========================================================
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_Final : MonoBehaviour
{
    public enum Mode { Passive, Chase, Hunt }

    [Header("Behaviour")]
    public Mode mode = Mode.Chase;
    public Transform player;

    [Header("Speeds")]
    public float chaseSpeed = 3.5f;
    public float huntSpeed = 5.0f;

    [Header("Attack Settings")]
    public float catchRange       = 2.5f;  // Attack range (units)
    public float oxygenDrainPct   = 5f;    // Har hit mein kitni oxygen drain ho (%)
    public float attackCooldown   = 1.5f;  // Cooldown between hits (seconds)

    [Header("Passive Patrol")]
    public Transform[] patrolPoints; // Khaali GameObjects (waypoints)
    public float patrolWaitTime = 1.5f;

    [Header("Hunt - Mimic Voice (Level 3)")]
    public AudioClip[] lureClips;    // "Help me", "I can hear you..." etc.
    public float lureInterval = 6f;  // Kitni der baad voice bajay

    [Header("Notifications")]
    public float approachWarningRange = 12f; // Itne distance par "ENEMY APPROACHING!" show ho

    private NavMeshAgent agent;
    private bool isGameOver = false;
    private int patrolIndex = 0;
    private float patrolTimer = 0f;
    private float lureTimer = 0f;
    private float approachCooldown = 0f;
    private float _attackTimer = 0f;
    private AudioSource audioSource;
    private bool _alertPlayed = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Mode ke hisaab se speed set karo
        if (mode == Mode.Hunt) agent.speed = huntSpeed;
        else agent.speed = chaseSpeed;
        agent.speed *= DifficultyManager.EnemySpeedMultiplier;
    }

    void Update()
    {
        // Stop audio when global game-over triggered (Phase 4 fix)
        if (OxygenSystem.IsGameOver)
        {
            if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
            if (agent != null && !agent.isStopped) agent.isStopped = true;
            return;
        }

        if (isGameOver || player == null) return;

        if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);

        // --- Approach warning notification + alert VFX ---
        if ((mode == Mode.Chase || mode == Mode.Hunt) && approachCooldown <= 0f)
        {
            if (distance < approachWarningRange)
            {
                NotificationUI.Show("ENEMY APPROACHING!", NotificationUI.NotifType.Danger);
                approachCooldown = 10f;
                AudioManager.PlayEnemyAlert();

                // First-detect burst particle above enemy head
                if (!_alertPlayed)
                {
                    _alertPlayed = true;
                    SpawnAlertBurst();
                }
            }
        }
        if (approachCooldown > 0f) approachCooldown -= Time.deltaTime;

        // --- Mode-specific behaviour ---
        switch (mode)
        {
            case Mode.Passive:
                Patrol();
                // Passive enemy player ko nahi maarta
                break;

            case Mode.Chase:
                agent.SetDestination(player.position);
                if (distance < catchRange) AttackPlayer();
                break;

            case Mode.Hunt:
                agent.SetDestination(player.position);
                PlayLure();
                if (distance < catchRange) AttackPlayer();
                break;
        }
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // Agar destination ke qareeb pohonch gaye to wait + next point
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                patrolTimer = 0f;
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                if (patrolPoints[patrolIndex] != null)
                    agent.SetDestination(patrolPoints[patrolIndex].position);
            }
        }
    }

    void PlayLure()
    {
        if (audioSource == null || lureClips == null || lureClips.Length == 0) return;

        lureTimer += Time.deltaTime;
        if (lureTimer >= lureInterval && !audioSource.isPlaying)
        {
            lureTimer = 0f;
            audioSource.clip = lureClips[Random.Range(0, lureClips.Length)];
            audioSource.Play();
            Debug.Log("Mimic voice lure played.");
        }
    }

    void SpawnAlertBurst()
    {
        var go = new GameObject("EnemyAlert");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.up * 2.4f;

        var ps  = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration        = 0.4f;
        main.loop            = false;
        main.startLifetime   = 0.7f;
        main.startSpeed      = 2.5f;
        main.startSize       = 0.12f;
        main.startColor      = new ParticleSystem.MinMaxGradient(new Color(1f, 0.15f, 0.05f));
        main.maxParticles    = 18;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 18) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.08f;

        ps.Play();
        Destroy(go, 2f);
    }

    void AttackPlayer()
    {
        if (_attackTimer > 0f) return; // cooldown active

        _attackTimer = attackCooldown;

        CameraShake.Shake(0.35f, 0.12f);

        // Red attack beam (visual hit indicator)
        SonarBeam.Shoot(
            transform.position + Vector3.up * 0.8f,
            player.position    + Vector3.up * 0.8f,
            SonarBeam.EnemyColor
        );

        OxygenSystem ox = player.GetComponent<OxygenSystem>();
        if (ox == null) return;

        float drain = oxygenDrainPct;
        ox.currentOxygen -= drain;
        if (ox.currentOxygen < 0f) ox.currentOxygen = 0f;

        NotificationUI.Show("ENEMY ATTACK! -" + drain + "% OXYGEN", NotificationUI.NotifType.Danger);
        Debug.Log($"Enemy attacked player! -{drain}% oxygen (now {ox.currentOxygen:F1}%)");
    }
}