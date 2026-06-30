using UnityEngine;

// =========================================================
//  Dark Sea - Player Controller
//
//  Controls:
//    W / S            → Forward / Back
//    A / D + Mouse    → Rotate
//    G (hold)         → Speed boost (no animation change)
//    SPACE            → Fire Sonar
// =========================================================
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SonarSystem))]
public class PlayerMovementFinal : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed           = 5f;
    [Tooltip("G hold speed. Auto-set by scene if 0: DarkSea=12, Level_2=15, Level_3=16")]
    public float boostSpeed          = 0f;
    public float keyboardRotateSpeed = 100f;
    public float mouseRotateSpeed    = 100f;
    public bool  useMouseLook        = true;
    public bool  lockCursor          = true;

    [Header("Animator (optional)")]
    [Tooltip("Drag the child Model's Animator here, or leave blank to auto-find.")]
    [SerializeField] Animator _anim;

    [Header("Sonar")]
    public SonarSystem sonarSystem;

    // ── Internal ───────────────────────────────────────────────────────────────
    Rigidbody _rb;
    float     _currentSpeed;
    bool      _boosting;

    static readonly int _hashSpeed    = Animator.StringToHash("Speed");
    static readonly int _hashIsMoving = Animator.StringToHash("IsMoving");

    public float moveSpeed { get => walkSpeed; set => walkSpeed = value; }

    void Awake()
    {
        // Auto-set boostSpeed by scene if left at default 0
        if (boostSpeed <= 0f)
        {
            string s = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            boostSpeed = s == "Level_2" ? 15f : s == "Level_3" ? 16f : 12f;
        }
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (sonarSystem == null)
            sonarSystem = GetComponent<SonarSystem>();

        if (_anim == null)
            _anim = GetComponentInChildren<Animator>();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    void Update()
    {
        // ── Rotation ──────────────────────────────────────────────────────────
        float keyRot   = Input.GetAxis("Horizontal") * keyboardRotateSpeed * Time.deltaTime;
        float mouseRot = useMouseLook
            ? Input.GetAxis("Mouse X") * mouseRotateSpeed * Time.deltaTime
            : 0f;
        transform.Rotate(0f, keyRot + mouseRot, 0f);

        // ── Speed boost (hold G) ──────────────────────────────────────────────
        _boosting = Input.GetKey(KeyCode.G);

        // ── Sonar ──────────────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.Space) && sonarSystem != null)
            sonarSystem.FireSonar();

        DriveAnimator();
    }

    void FixedUpdate()
    {
        float z     = Input.GetAxis("Vertical");
        float speed = _boosting ? boostSpeed : walkSpeed;
        Vector3 move = transform.forward * z * speed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + move);
    }

    void DriveAnimator()
    {
        if (_anim == null) return;

        float inputZ   = Mathf.Abs(Input.GetAxis("Vertical"));
        float target   = inputZ * walkSpeed; // animator always uses walkSpeed (walk anim only)
        _currentSpeed  = Mathf.Lerp(_currentSpeed, target, Time.deltaTime * 8f);

        _anim.SetFloat(_hashSpeed,    _currentSpeed);
        _anim.SetBool (_hashIsMoving, _currentSpeed > 0.15f);
    }

    public void FreezePlayer()
    {
        enabled = false;
        if (_rb != null) _rb.linearVelocity = Vector3.zero;
        if (_anim != null)
        {
            _anim.SetFloat(_hashSpeed, 0f);
            _anim.SetBool(_hashIsMoving, false);
        }
    }
}
