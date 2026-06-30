using UnityEngine;
using UnityEngine.SceneManagement;

// Procedurally builds a stylised diving-suit humanoid (Subway-Surfers-style proportions).
// Big round helmet, wide shoulders, chunky gloves/boots, knee+elbow pads, utility belt.
// Auto-spawns as "PlayerBody" child of the Player in gameplay scenes.
public class PlayerBodyVisuals : MonoBehaviour
{
    // ─── Auto-spawn ───────────────────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!scene.Contains("Level") && !scene.Equals("DarkSea")) return;

        var player = GameObject.FindWithTag("Player");
        if (player == null || player.transform.Find("PlayerBody") != null) return;

        // Humanoid model (Ch45) already present — no need for procedural body
        if (player.GetComponentInChildren<Animator>() != null) return;

        var go = new GameObject("PlayerBody");
        go.transform.SetParent(player.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.AddComponent<PlayerBodyVisuals>();
    }

    // ─── Pivot references ─────────────────────────────────────────────────────

    Transform _headPivot;
    Transform _lArmPivot,  _rArmPivot;
    Transform _lElbowPivot, _rElbowPivot;
    Transform _lLegPivot,  _rLegPivot;
    Transform _lKneePivot, _rKneePivot;

    Rigidbody _rb;
    float     _walkCycle;

    static Material _suitMat;
    static Material _visorMat;
    static Material _detailMat;
    static Material _accentMat;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    void Awake()
    {
        _rb = GetComponentInParent<Rigidbody>();
        EnsureMaterials();
        if (transform.childCount == 0) Build();
        CachePivots();
        int pov = PlayerData.GetInt("PrefPOV", 0);
        gameObject.SetActive(pov != 0);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;
        Animate();
    }

    // ─── Animation ────────────────────────────────────────────────────────────

    void Animate()
    {
        float speed  = _rb != null
            ? new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z).magnitude
            : 0f;
        bool  moving = speed > 0.3f;

        _walkCycle += Time.deltaTime * (moving ? speed * 3.5f : 0.6f);

        float swing = moving
            ? Mathf.Sin(_walkCycle) * 42f
            : Mathf.Sin(_walkCycle * 0.5f) * 2.5f;

        if (_lArmPivot) _lArmPivot.localRotation = Quaternion.Euler( swing, 0f, -10f);
        if (_rArmPivot) _rArmPivot.localRotation = Quaternion.Euler(-swing, 0f,  10f);

        if (_lLegPivot) _lLegPivot.localRotation = Quaternion.Euler(-swing * 0.85f, 0f, 0f);
        if (_rLegPivot) _rLegPivot.localRotation = Quaternion.Euler( swing * 0.85f, 0f, 0f);

        float bend = Mathf.Abs(swing);
        if (_lElbowPivot) _lElbowPivot.localRotation = Quaternion.Euler(bend * 0.40f, 0f, 0f);
        if (_rElbowPivot) _rElbowPivot.localRotation = Quaternion.Euler(bend * 0.40f, 0f, 0f);
        if (_lKneePivot)  _lKneePivot.localRotation  = Quaternion.Euler(bend * 0.52f, 0f, 0f);
        if (_rKneePivot)  _rKneePivot.localRotation  = Quaternion.Euler(bend * 0.52f, 0f, 0f);

        // Head: slight forward tilt while moving + idle sway
        if (_headPivot)
        {
            float sway    = Mathf.Sin(_walkCycle * 0.5f) * (moving ? 2.5f : 1.0f);
            float forward = moving ? 6f : 0f;
            _headPivot.localRotation = Quaternion.Euler(forward, 0f, sway);
        }

        // Body vertical bob
        float bobY = moving ? Mathf.Abs(Mathf.Sin(_walkCycle)) * 0.030f : 0f;
        Vector3 lp = transform.localPosition;
        transform.localPosition = new Vector3(lp.x, bobY, lp.z);
    }

    // ─── Hierarchy builder ────────────────────────────────────────────────────

    void Build()
    {
        // ══ BOOTS / FLIPPERS (wide, cartoony) ════════════════════════════════
        MakePrim(PrimitiveType.Cube, "LFoot", transform,
            new Vector3(-0.10f, 0.046f,  0.07f), new Vector3(0.14f, 0.062f, 0.30f), _suitMat);
        MakePrim(PrimitiveType.Cube, "RFoot", transform,
            new Vector3( 0.10f, 0.046f,  0.07f), new Vector3(0.14f, 0.062f, 0.30f), _suitMat);

        // Boot cuff band
        MakePrim(PrimitiveType.Cube, "LBootCuff", transform,
            new Vector3(-0.10f, 0.095f, 0.02f), new Vector3(0.15f, 0.028f, 0.21f), _detailMat);
        MakePrim(PrimitiveType.Cube, "RBootCuff", transform,
            new Vector3( 0.10f, 0.095f, 0.02f), new Vector3(0.15f, 0.028f, 0.21f), _detailMat);

        // ══ LEGS ══════════════════════════════════════════════════════════════
        var lLeg  = Pivot("LLegPivot",  transform, new Vector3(-0.105f, 0.92f, 0f));
        MakePrim(PrimitiveType.Capsule, "LUpperLeg", lLeg,
            new Vector3(0f, -0.165f, 0f), new Vector3(0.135f, 0.27f, 0.135f), _suitMat);

        var lKnee = Pivot("LKneePivot", lLeg, new Vector3(0f, -0.31f, 0f));
        MakePrim(PrimitiveType.Capsule, "LLowerLeg", lKnee,
            new Vector3(0f, -0.135f, 0f), new Vector3(0.115f, 0.23f, 0.115f), _suitMat);
        // Knee pad
        MakePrim(PrimitiveType.Cube, "LKneePad", lKnee,
            new Vector3(0f, 0.025f, 0.075f), new Vector3(0.092f, 0.075f, 0.042f), _detailMat);

        var rLeg  = Pivot("RLegPivot",  transform, new Vector3( 0.105f, 0.92f, 0f));
        MakePrim(PrimitiveType.Capsule, "RUpperLeg", rLeg,
            new Vector3(0f, -0.165f, 0f), new Vector3(0.135f, 0.27f, 0.135f), _suitMat);

        var rKnee = Pivot("RKneePivot", rLeg, new Vector3(0f, -0.31f, 0f));
        MakePrim(PrimitiveType.Capsule, "RLowerLeg", rKnee,
            new Vector3(0f, -0.135f, 0f), new Vector3(0.115f, 0.23f, 0.115f), _suitMat);
        MakePrim(PrimitiveType.Cube, "RKneePad", rKnee,
            new Vector3(0f, 0.025f, 0.075f), new Vector3(0.092f, 0.075f, 0.042f), _detailMat);

        // ══ HIPS + UTILITY BELT ════════════════════════════════════════════════
        MakePrim(PrimitiveType.Cube, "Hips", transform,
            new Vector3(0f, 0.94f, 0f), new Vector3(0.33f, 0.145f, 0.21f), _suitMat);

        // Belt strap
        MakePrim(PrimitiveType.Cube, "Belt", transform,
            new Vector3(0f, 0.940f, 0.095f), new Vector3(0.31f, 0.042f, 0.065f), _detailMat);
        // Belt buckle (amber)
        MakePrim(PrimitiveType.Cube, "BeltBuckle", transform,
            new Vector3(0f, 0.940f, 0.140f), new Vector3(0.065f, 0.042f, 0.032f), _visorMat);
        // Side pouches
        MakePrim(PrimitiveType.Cube, "LPouch", transform,
            new Vector3(-0.175f, 0.920f, 0f), new Vector3(0.055f, 0.090f, 0.075f), _detailMat);
        MakePrim(PrimitiveType.Cube, "RPouch", transform,
            new Vector3( 0.175f, 0.920f, 0f), new Vector3(0.055f, 0.090f, 0.075f), _detailMat);

        // ══ TORSO (broad — hero silhouette) ═══════════════════════════════════
        MakePrim(PrimitiveType.Capsule, "Torso", transform,
            new Vector3(0f, 1.22f, 0f), new Vector3(0.40f, 0.31f, 0.25f), _suitMat);

        // Chest panel / badge
        MakePrim(PrimitiveType.Cube, "ChestPanel", transform,
            new Vector3(0f, 1.24f, 0.122f), new Vector3(0.17f, 0.13f, 0.030f), _detailMat);
        // Amber chest logo / indicator light
        MakePrim(PrimitiveType.Cylinder, "ChestLight", transform,
            new Vector3(0f, 1.28f, 0.148f), new Vector3(0.040f, 0.012f, 0.040f), _visorMat);

        // Shoulder pads — wide, flat cubes that stick out to sides
        MakePrim(PrimitiveType.Cube, "LShoulderPad", transform,
            new Vector3(-0.255f, 1.42f, 0f), new Vector3(0.110f, 0.065f, 0.175f), _detailMat);
        MakePrim(PrimitiveType.Cube, "RShoulderPad", transform,
            new Vector3( 0.255f, 1.42f, 0f), new Vector3(0.110f, 0.065f, 0.175f), _detailMat);

        // ══ OXYGEN TANK (back — prominent) ════════════════════════════════════
        MakePrim(PrimitiveType.Capsule, "OxygenTank", transform,
            new Vector3(0f, 1.24f, -0.19f), new Vector3(0.175f, 0.300f, 0.175f), _detailMat);
        // Tank amber stripe
        MakePrim(PrimitiveType.Cube, "TankStripe", transform,
            new Vector3(0f, 1.30f, -0.280f), new Vector3(0.065f, 0.21f, 0.022f), _visorMat);
        // Harness straps (vertical + horizontal)
        MakePrim(PrimitiveType.Cube, "StrapTop", transform,
            new Vector3(0f, 1.38f, -0.105f), new Vector3(0.28f, 0.028f, 0.065f), _accentMat);
        MakePrim(PrimitiveType.Cube, "StrapBot", transform,
            new Vector3(0f, 1.10f, -0.105f), new Vector3(0.28f, 0.028f, 0.065f), _accentMat);
        MakePrim(PrimitiveType.Cube, "StrapVert", transform,
            new Vector3(0f, 1.24f, -0.065f), new Vector3(0.028f, 0.30f, 0.028f), _accentMat);

        // ══ ARMS ══════════════════════════════════════════════════════════════
        var lArm   = Pivot("LArmPivot",   transform, new Vector3(-0.255f, 1.42f, 0f));
        MakePrim(PrimitiveType.Capsule, "LUpperArm", lArm,
            new Vector3(0f, -0.135f, 0f), new Vector3(0.115f, 0.190f, 0.115f), _suitMat);

        var lElbow = Pivot("LElbowPivot", lArm, new Vector3(0f, -0.28f, 0f));
        MakePrim(PrimitiveType.Capsule, "LForearm", lElbow,
            new Vector3(0f, -0.115f, 0f), new Vector3(0.105f, 0.170f, 0.105f), _suitMat);
        // Elbow pad
        MakePrim(PrimitiveType.Cube, "LElbowPad", lElbow,
            new Vector3(0f, 0.025f, -0.062f), new Vector3(0.082f, 0.065f, 0.042f), _detailMat);
        // Glove — boxy, thick, like a diving glove
        MakePrim(PrimitiveType.Cube, "LGlove", lElbow,
            new Vector3(0f, -0.260f, 0f), new Vector3(0.115f, 0.095f, 0.115f), _detailMat);

        var rArm   = Pivot("RArmPivot",   transform, new Vector3( 0.255f, 1.42f, 0f));
        MakePrim(PrimitiveType.Capsule, "RUpperArm", rArm,
            new Vector3(0f, -0.135f, 0f), new Vector3(0.115f, 0.190f, 0.115f), _suitMat);

        var rElbow = Pivot("RElbowPivot", rArm, new Vector3(0f, -0.28f, 0f));
        MakePrim(PrimitiveType.Capsule, "RForearm", rElbow,
            new Vector3(0f, -0.115f, 0f), new Vector3(0.105f, 0.170f, 0.105f), _suitMat);
        MakePrim(PrimitiveType.Cube, "RElbowPad", rElbow,
            new Vector3(0f, 0.025f, -0.062f), new Vector3(0.082f, 0.065f, 0.042f), _detailMat);
        MakePrim(PrimitiveType.Cube, "RGlove", rElbow,
            new Vector3(0f, -0.260f, 0f), new Vector3(0.115f, 0.095f, 0.115f), _detailMat);

        // ══ NECK + COLLAR ══════════════════════════════════════════════════════
        MakePrim(PrimitiveType.Capsule, "Neck", transform,
            new Vector3(0f, 1.535f, 0f), new Vector3(0.105f, 0.105f, 0.105f), _suitMat);
        // Collar locking ring
        MakePrim(PrimitiveType.Cylinder, "CollarRing", transform,
            new Vector3(0f, 1.570f, 0f), new Vector3(0.195f, 0.020f, 0.195f), _detailMat);

        // ══ HEAD / HELMET ══════════════════════════════════════════════════════
        // Cartoony: head is noticeably large relative to body
        var head = Pivot("HeadPivot", transform, new Vector3(0f, 1.71f, 0f));

        // Main helmet — round, big
        MakePrim(PrimitiveType.Sphere, "Helmet", head,
            Vector3.zero, new Vector3(0.315f, 0.340f, 0.315f), _suitMat);

        // Equator ridge band
        MakePrim(PrimitiveType.Cylinder, "HelmetRidge", head,
            new Vector3(0f, -0.025f, 0f), new Vector3(0.330f, 0.020f, 0.330f), _detailMat);

        // Visor frame (dark surround — drawn first so visor sits on top)
        MakePrim(PrimitiveType.Cube, "VisorFrame", head,
            new Vector3(0f, 0.015f, 0.144f), new Vector3(0.225f, 0.108f, 0.040f), _accentMat);
        // Visor glass (amber, wide — very visible)
        MakePrim(PrimitiveType.Cube, "Visor", head,
            new Vector3(0f, 0.015f, 0.151f), new Vector3(0.205f, 0.090f, 0.042f), _visorMat);

        // Head light on top-front
        MakePrim(PrimitiveType.Cube, "LightHousing", head,
            new Vector3(0f, 0.155f, 0.072f), new Vector3(0.080f, 0.044f, 0.060f), _detailMat);
        MakePrim(PrimitiveType.Cylinder, "HeadLight", head,
            new Vector3(0f, 0.155f, 0.100f), new Vector3(0.052f, 0.024f, 0.052f), _visorMat);

        // Side vents — gives the helmet a mechanical look
        MakePrim(PrimitiveType.Cube, "LVent", head,
            new Vector3(-0.148f, -0.040f, 0f), new Vector3(0.028f, 0.080f, 0.100f), _detailMat);
        MakePrim(PrimitiveType.Cube, "RVent", head,
            new Vector3( 0.148f, -0.040f, 0f), new Vector3(0.028f, 0.080f, 0.100f), _detailMat);

        // Hose connector at back-bottom of helmet
        MakePrim(PrimitiveType.Cylinder, "HosePort", head,
            new Vector3(0f, -0.120f, -0.152f), new Vector3(0.048f, 0.048f, 0.048f), _detailMat);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    Transform Pivot(string name, Transform parent, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        return go.transform;
    }

    void MakePrim(PrimitiveType type, string name, Transform parent,
                  Vector3 localPos, Vector3 localScale, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale    = localScale;

        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null && mat != null)
        {
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }

    void CachePivots()
    {
        _headPivot   = transform.Find("HeadPivot");
        _lArmPivot   = transform.Find("LArmPivot");
        _rArmPivot   = transform.Find("RArmPivot");
        _lElbowPivot = _lArmPivot?.Find("LElbowPivot");
        _rElbowPivot = _rArmPivot?.Find("RElbowPivot");
        _lLegPivot   = transform.Find("LLegPivot");
        _rLegPivot   = transform.Find("RLegPivot");
        _lKneePivot  = _lLegPivot?.Find("LKneePivot");
        _rKneePivot  = _rLegPivot?.Find("RKneePivot");
    }

    static void EnsureMaterials()
    {
        if (_suitMat != null && _visorMat != null && _detailMat != null && _accentMat != null) return;

        Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        if (_suitMat == null)
        {
            _suitMat = new Material(lit) { name = "DivingSuit" };
            _suitMat.color = new Color(0.10f, 0.16f, 0.14f, 1f);   // dark teal
        }
        if (_visorMat == null)
        {
            _visorMat = new Material(lit) { name = "Visor" };
            _visorMat.color = new Color(0.76f, 0.40f, 0.06f, 1f);   // amber
        }
        if (_detailMat == null)
        {
            _detailMat = new Material(lit) { name = "DivingDetail" };
            _detailMat.color = new Color(0.18f, 0.27f, 0.24f, 1f);  // lighter teal
        }
        if (_accentMat == null)
        {
            _accentMat = new Material(lit) { name = "DivingAccent" };
            _accentMat.color = new Color(0.05f, 0.08f, 0.07f, 1f);  // very dark teal (contrast)
        }
    }
}
