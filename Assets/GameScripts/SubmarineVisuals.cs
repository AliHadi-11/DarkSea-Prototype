using UnityEngine;

// Procedurally built deep-sea military/research submarine from Unity primitives.
// Attached as "Submarine" child of the Player by SubmarineToggle.
// Propeller spins in Update. Call SetActive(false) to hide without destroying.
public class SubmarineVisuals : MonoBehaviour
{
    Transform _propellerParent;

    static Material _hullMat;
    static Material _towerMat;
    static Material _glassMat;
    static Material _metalMat;
    static Material _amberMat;
    static Material _redLightMat;
    static Material _greenLightMat;

    void Awake()
    {
        EnsureMaterials();
        Build();
    }

    void Update()
    {
        if (_propellerParent != null)
            _propellerParent.Rotate(0f, 0f, 360f * Time.deltaTime, Space.Self);
    }

    // ── Build ──────────────────────────────────────────────────────────────────

    void Build()
    {
        // ── MAIN HULL (horizontal capsule — rounded torpedo body) ──────────────
        // Capsule default: long axis = Y. Euler(-90,0,0) rotates that axis to +Z (forward).
        Part(PrimitiveType.Capsule, "Hull", transform,
            Vector3.zero, new Vector3(0.9f, 1.80f, 0.9f),
            Quaternion.Euler(-90, 0, 0), _hullMat);

        // Extra nose sphere to sharpen the bow shape
        Part(PrimitiveType.Sphere, "NoseFairing", transform,
            new Vector3(0f, 0f, 1.95f), new Vector3(0.86f, 0.82f, 0.5f),
            Quaternion.identity, _hullMat);

        // Extra tail sphere to taper the stern
        Part(PrimitiveType.Sphere, "TailFairing", transform,
            new Vector3(0f, 0f, -1.95f), new Vector3(0.86f, 0.82f, 0.5f),
            Quaternion.identity, _hullMat);

        // ── CONNING TOWER ──────────────────────────────────────────────────────
        Part(PrimitiveType.Cube, "Tower", transform,
            new Vector3(0f, 1.02f, 0.12f), new Vector3(0.30f, 0.65f, 0.50f),
            Quaternion.identity, _towerMat);

        // Tower top cap (rounded cylinder)
        Part(PrimitiveType.Cylinder, "TowerCap", transform,
            new Vector3(0f, 1.42f, 0.12f), new Vector3(0.162f, 0.055f, 0.27f),
            Quaternion.identity, _towerMat);

        // Tower front slope fairing
        Part(PrimitiveType.Cube, "TowerFront", transform,
            new Vector3(0f, 0.80f, 0.36f), new Vector3(0.28f, 0.24f, 0.18f),
            Quaternion.Euler(-22f, 0f, 0f), _towerMat);

        // Tower rear slope
        Part(PrimitiveType.Cube, "TowerRear", transform,
            new Vector3(0f, 0.80f, -0.12f), new Vector3(0.28f, 0.22f, 0.18f),
            Quaternion.Euler(18f, 0f, 0f), _towerMat);

        // ── PERISCOPE ──────────────────────────────────────────────────────────
        Part(PrimitiveType.Cylinder, "Periscope", transform,
            new Vector3(0f, 1.72f, 0.26f), new Vector3(0.038f, 0.24f, 0.038f),
            Quaternion.identity, _metalMat);

        // Periscope elbow (horizontal arm)
        Part(PrimitiveType.Cylinder, "PeriscopeArm", transform,
            new Vector3(0f, 1.96f, 0.13f), new Vector3(0.028f, 0.14f, 0.028f),
            Quaternion.Euler(-90f, 0f, 0f), _metalMat);

        // Lens eye
        Part(PrimitiveType.Sphere, "PeriscopeLens", transform,
            new Vector3(0f, 1.96f, 0.01f), new Vector3(0.055f, 0.055f, 0.038f),
            Quaternion.identity, _glassMat);

        // Antenna mast (thin, behind periscope)
        Part(PrimitiveType.Cylinder, "Antenna", transform,
            new Vector3(0.06f, 1.82f, -0.04f), new Vector3(0.014f, 0.32f, 0.014f),
            Quaternion.identity, _metalMat);

        // ── FRONT VIEWPORT (observation dome) ─────────────────────────────────
        Part(PrimitiveType.Sphere, "Viewport", transform,
            new Vector3(0f, 0.14f, 2.30f), new Vector3(0.34f, 0.30f, 0.13f),
            Quaternion.identity, _glassMat);

        // Viewport rim ring
        Part(PrimitiveType.Cylinder, "ViewportRim", transform,
            new Vector3(0f, 0.14f, 2.23f), new Vector3(0.40f, 0.028f, 0.40f),
            Quaternion.identity, _metalMat);

        // ── SONAR DOME (keel-forward bulge) ───────────────────────────────────
        Part(PrimitiveType.Sphere, "SonarDome", transform,
            new Vector3(0f, -0.72f, 1.55f), new Vector3(0.28f, 0.22f, 0.28f),
            Quaternion.identity, _glassMat);

        // ── TORPEDO TUBES (2 — front) ─────────────────────────────────────────
        for (int i = -1; i <= 1; i += 2)
        {
            // Outer tube barrel
            Part(PrimitiveType.Cylinder, "Tube_" + i, transform,
                new Vector3(i * 0.27f, 0.04f, 2.30f),
                new Vector3(0.088f, 0.34f, 0.088f),
                Quaternion.Euler(-90f, 0f, 0f), _metalMat);

            // Tube opening (darker inset sphere)
            Part(PrimitiveType.Sphere, "TubeMouth_" + i, transform,
                new Vector3(i * 0.27f, 0.04f, 2.60f),
                new Vector3(0.085f, 0.085f, 0.045f),
                Quaternion.identity, _glassMat);
        }

        // ── MID-HULL HYDROPLANES (diving planes) ──────────────────────────────
        for (int side = -1; side <= 1; side += 2)
        {
            Part(PrimitiveType.Cube, "Hydroplane_" + side, transform,
                new Vector3(side * 1.20f, -0.08f, -0.08f),
                new Vector3(0.65f, 0.052f, 0.52f),
                Quaternion.Euler(0f, 0f, side * -4f), _hullMat);

            // Tip of hydroplane
            Part(PrimitiveType.Sphere, "HydroTip_" + side, transform,
                new Vector3(side * 1.52f, -0.08f, -0.08f),
                new Vector3(0.12f, 0.052f, 0.42f),
                Quaternion.identity, _hullMat);
        }

        // ── STERN CONTROL SURFACES ─────────────────────────────────────────────
        // Vertical rudder
        Part(PrimitiveType.Cube, "Rudder", transform,
            new Vector3(0f, 0.42f, -2.20f), new Vector3(0.05f, 0.70f, 0.50f),
            Quaternion.identity, _hullMat);
        // Rudder top fin cap
        Part(PrimitiveType.Sphere, "RudderTip", transform,
            new Vector3(0f, 0.78f, -2.06f), new Vector3(0.052f, 0.14f, 0.25f),
            Quaternion.identity, _hullMat);

        // Horizontal stabilizers (left + right)
        for (int side = -1; side <= 1; side += 2)
        {
            Part(PrimitiveType.Cube, "Stabilizer_" + side, transform,
                new Vector3(side * 0.60f, -0.06f, -2.18f),
                new Vector3(0.50f, 0.052f, 0.38f),
                Quaternion.Euler(0f, 0f, side * -6f), _hullMat);
        }

        // ── KEEL ──────────────────────────────────────────────────────────────
        Part(PrimitiveType.Cube, "Keel", transform,
            new Vector3(0f, -0.88f, 0f), new Vector3(0.07f, 0.04f, 3.10f),
            Quaternion.identity, _metalMat);

        // ── TOP HULL STRIPE ────────────────────────────────────────────────────
        Part(PrimitiveType.Cube, "TopStripe", transform,
            new Vector3(0f, 0.90f, 0f), new Vector3(0.10f, 0.025f, 2.90f),
            Quaternion.identity, _towerMat);

        // ── WATERLINE ACCENT STRIPES ───────────────────────────────────────────
        for (int side = -1; side <= 1; side += 2)
        {
            Part(PrimitiveType.Cube, "WaterStripe_" + side, transform,
                new Vector3(side * 0.72f, 0.24f, 0f),
                new Vector3(0.018f, 0.025f, 3.10f),
                Quaternion.identity, _amberMat);
        }

        // ── HULL NUMBER / ID PLATE (on tower side) ────────────────────────────
        Part(PrimitiveType.Cube, "IDPlate", transform,
            new Vector3(0.162f, 0.98f, 0.12f), new Vector3(0.008f, 0.16f, 0.26f),
            Quaternion.identity, _towerMat);
        Part(PrimitiveType.Cube, "IDMark", transform,
            new Vector3(0.170f, 0.98f, 0.12f), new Vector3(0.006f, 0.10f, 0.16f),
            Quaternion.identity, _amberMat);

        // ── PORTHOLES (3 each side) ────────────────────────────────────────────
        for (int side = -1; side <= 1; side += 2)
        {
            for (int j = 0; j < 3; j++)
            {
                float z = -0.55f + j * 0.62f;
                // Glass
                Part(PrimitiveType.Sphere, "Port_" + side + "_" + j, transform,
                    new Vector3(side * 0.916f, 0.14f, z),
                    new Vector3(0.092f, 0.092f, 0.040f),
                    Quaternion.identity, _glassMat);
                // Metal frame ring (flat disc perpendicular to hull)
                Part(PrimitiveType.Cylinder, "PortFrame_" + side + "_" + j, transform,
                    new Vector3(side * 0.908f, 0.14f, z),
                    new Vector3(0.14f, 0.022f, 0.14f),
                    Quaternion.Euler(0f, 0f, 90f), _metalMat);
            }
        }

        // ── PROPELLER ASSEMBLY ─────────────────────────────────────────────────
        var propGo = new GameObject("PropellerParent");
        propGo.transform.SetParent(transform);
        propGo.transform.localPosition = new Vector3(0f, 0f, -2.44f);
        propGo.transform.localRotation = Quaternion.identity;
        _propellerParent = propGo.transform;

        // Central hub
        Part(PrimitiveType.Sphere, "PropHub", _propellerParent,
            Vector3.zero, new Vector3(0.15f, 0.15f, 0.20f),
            Quaternion.identity, _metalMat);

        // 4 blades at 90° offsets
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            float rad   = angle * Mathf.Deg2Rad;

            var bladeGo = new GameObject("BladeRoot_" + i);
            bladeGo.transform.SetParent(_propellerParent);
            bladeGo.transform.localPosition = Vector3.zero;
            bladeGo.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            // Long flat blade
            Part(PrimitiveType.Cube, "Blade_" + i, bladeGo.transform,
                new Vector3(0f, 0.33f, 0f),
                new Vector3(0.07f, 0.52f, 0.14f),
                Quaternion.Euler(0f, 12f, 0f), _metalMat);
        }

        // Prop shaft
        Part(PrimitiveType.Cylinder, "PropShaft", transform,
            new Vector3(0f, 0f, -2.28f), new Vector3(0.068f, 0.20f, 0.068f),
            Quaternion.Euler(-90f, 0f, 0f), _metalMat);

        // ── NAVIGATION / RUNNING LIGHTS ────────────────────────────────────────
        // Port (left) = red
        Part(PrimitiveType.Sphere, "LightRed", transform,
            new Vector3(-0.93f, 0.16f, 1.45f), Vector3.one * 0.068f,
            Quaternion.identity, _redLightMat);

        // Starboard (right) = green
        Part(PrimitiveType.Sphere, "LightGreen", transform,
            new Vector3(0.93f, 0.16f, 1.45f), Vector3.one * 0.068f,
            Quaternion.identity, _greenLightMat);

        // Stern white light
        Part(PrimitiveType.Sphere, "LightStern", transform,
            new Vector3(0f, -0.55f, -2.30f), Vector3.one * 0.055f,
            Quaternion.identity, _amberMat);

        // Masthead (top of tower)
        Part(PrimitiveType.Sphere, "LightMast", transform,
            new Vector3(0f, 1.74f, 0.12f), Vector3.one * 0.068f,
            Quaternion.identity, _amberMat);

        // Tower amber indicator strip (front)
        Part(PrimitiveType.Cube, "TowerIndicator", transform,
            new Vector3(0f, 1.10f, 0.292f), new Vector3(0.20f, 0.028f, 0.018f),
            Quaternion.identity, _amberMat);

        // ── BALLAST VENTS (bottom hull — 4 grille slots) ───────────────────────
        for (int v = 0; v < 4; v++)
        {
            float z = -0.9f + v * 0.62f;
            Part(PrimitiveType.Cube, "BallastVent_" + v, transform,
                new Vector3(0f, -0.88f, z), new Vector3(0.18f, 0.030f, 0.10f),
                Quaternion.identity, _glassMat);
        }
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    static void Part(PrimitiveType type, string partName, Transform parent,
                     Vector3 localPos, Vector3 localScale, Quaternion localRot, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = partName;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
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

    // ── Materials ─────────────────────────────────────────────────────────────

    static void EnsureMaterials()
    {
        if (_hullMat != null) return;

        Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        _hullMat = new Material(lit) { name = "Sub_Hull" };
        _hullMat.color = new Color(0.08f, 0.12f, 0.17f);  // deep navy

        _towerMat = new Material(lit) { name = "Sub_Tower" };
        _towerMat.color = new Color(0.11f, 0.16f, 0.22f);  // slightly lighter navy

        _glassMat = new Material(lit) { name = "Sub_Glass" };
        _glassMat.color = new Color(0.04f, 0.07f, 0.13f);  // near-black blue

        _metalMat = new Material(lit) { name = "Sub_Metal" };
        _metalMat.color = new Color(0.21f, 0.20f, 0.18f);  // dark warm steel

        _amberMat = new Material(lit) { name = "Sub_Amber" };
        _amberMat.color = new Color(0.84f, 0.48f, 0.06f);  // amber accent

        _redLightMat = new Material(lit) { name = "Sub_RedLight" };
        _redLightMat.color = new Color(1.00f, 0.10f, 0.06f);  // bright red

        _greenLightMat = new Material(lit) { name = "Sub_GreenLight" };
        _greenLightMat.color = new Color(0.08f, 1.00f, 0.28f);  // bright green
    }
}
