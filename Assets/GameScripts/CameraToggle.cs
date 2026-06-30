using UnityEngine;
using UnityEngine.UIElements;

// Press [V] to toggle: 1st Person ↔ 3rd Person.
// Shows/hides PlayerArms (FP arms) and PlayerBody (3rd-person humanoid) accordingly.
// Reads PrefPOV (0/1) from PlayerPrefs on Start; saves on every toggle.
public class CameraToggle : MonoBehaviour
{
    enum Mode { FirstPerson = 0, ThirdPerson = 1 }
    Mode _mode = Mode.FirstPerson;

    Camera     _cam;
    GameObject _armsRoot;
    GameObject _bodyRoot;
    Button     _btn;
    Label      _btnLabel;

    Vector3    _fpLocalPos;
    Quaternion _fpLocalRot;

    // Camera position for 3rd-person mode (local to player root)
    static readonly Vector3    TP_POS = new Vector3(0f, 1.80f, -3.5f);
    static readonly Quaternion TP_ROT = Quaternion.Euler(12f, 0f, 0f);

    // ─── UI ───────────────────────────────────────────────────────

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null) return;

        _btn = doc.rootVisualElement?.Q<Button>("btn-pov");
        if (_btn != null)
        {
            _btnLabel = _btn.Q<Label>();
            _btn.clicked += Toggle;
        }
    }

    void OnDisable()
    {
        if (_btn != null) _btn.clicked -= Toggle;
    }

    // ─── Init ─────────────────────────────────────────────────────

    void Start()
    {
        _cam = Camera.main;
        if (_cam != null)
        {
            _fpLocalPos = _cam.transform.localPosition;
            _fpLocalRot = _cam.transform.localRotation;
            _armsRoot   = _cam.transform.Find("PlayerArms")?.gameObject;
        }

        // Apply saved preference (0=FP, 1=TP)
        int pref = PlayerData.GetInt("PrefPOV", 0);
        _mode = (Mode)Mathf.Clamp(pref, 0, 1);
        Apply();
    }

    // ─── Input ────────────────────────────────────────────────────

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) Toggle();
    }

    // ─── Toggle ───────────────────────────────────────────────────

    void Toggle()
    {
        _mode = (Mode)(((int)_mode + 1) % 2);
        Apply();
        PlayerData.SetInt("PrefPOV", (int)_mode);
        PlayerPrefs.Save();
    }

    void Apply()
    {
        if (_cam == null) return;

        // ── Camera position ─────────────────────────────
        if (_mode == Mode.FirstPerson)
        {
            _cam.transform.localPosition = _fpLocalPos;
            _cam.transform.localRotation = _fpLocalRot;
        }
        else
        {
            _cam.transform.localPosition = TP_POS;
            _cam.transform.localRotation = TP_ROT;
        }

        bool isFP   = _mode == Mode.FirstPerson;
        bool isBody = _mode == Mode.ThirdPerson;

        // ── Arms / body visibility ──────────────────────
        if (_armsRoot != null) _armsRoot.SetActive(isFP);

        // Lazy-find body (spawned after Start via RuntimeInitialize)
        if (_bodyRoot == null)
        {
            Transform playerRoot = _cam.transform.parent;
            if (playerRoot != null)
                _bodyRoot = playerRoot.Find("PlayerBody")?.gameObject;
        }
        if (_bodyRoot != null) _bodyRoot.SetActive(isBody);

        // ── HUD button label ────────────────────────────
        if (_btnLabel != null)
            _btnLabel.text = _mode == Mode.FirstPerson
                ? "[ V ]  1ST PERSON"
                : "[ V ]  3RD PERSON";
    }
}
