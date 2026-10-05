using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedural original 2026-style single-seater model based on the supplied
/// front/top/rear visual references. It is intentionally an original game asset,
/// not a 1:1 copy of a real team's car or livery.
/// </summary>
public class F12026OriginalCarModel : MonoBehaviour
{
    [Header("Materials")]
    public Material bodyMaterial;
    public Material darkMaterial;
    public Material tyreMaterial;
    public Material accentMaterial;
    public Material glassMaterial;

    [Header("Build")]
    public bool buildOnStart = true;
    public bool addVisualController = true;

    private readonly List<GameObject> parts = new();

    private Transform frontWing;
    private Transform rearWing;
    private Transform steeringWheel;
    private Transform frontLeftWheel;
    private Transform frontRightWheel;

    private void Start()
    {
        if (buildOnStart)
            Build();
    }

    [ContextMenu("Rebuild Car")]
    public void Build()
    {
        ClearGenerated();

        EnsureMaterials();

        // Main proportions: long nose, narrow central cockpit, wide front/rear tyre stance.
        Make("Floor", PrimitiveType.Cube,
            new Vector3(0f, 0.18f, 0f),
            new Vector3(1.72f, 0.10f, 3.70f), darkMaterial);

        Make("MainChassis", PrimitiveType.Cube,
            new Vector3(0f, 0.39f, 0.05f),
            new Vector3(0.94f, 0.27f, 2.45f), bodyMaterial);

        Make("FrontMonocoque", PrimitiveType.Sphere,
            new Vector3(0f, 0.43f, 0.86f),
            new Vector3(0.76f, 0.30f, 1.55f), bodyMaterial);

        Make("LongNose", PrimitiveType.Sphere,
            new Vector3(0f, 0.42f, 1.66f),
            new Vector3(0.42f, 0.20f, 1.55f), bodyMaterial);

        // Nose tip and central camera/pitot detail.
        Make("NoseTip", PrimitiveType.Cube,
            new Vector3(0f, 0.36f, 2.31f),
            new Vector3(0.16f, 0.08f, 0.34f), darkMaterial);

        Make("NoseAccent", PrimitiveType.Cube,
            new Vector3(0f, 0.455f, 1.93f),
            new Vector3(0.44f, 0.018f, 0.045f), accentMaterial);

        // Cockpit and headrest.
        Make("CockpitOpening", PrimitiveType.Cube,
            new Vector3(0f, 0.61f, 0.28f),
            new Vector3(0.47f, 0.09f, 0.78f), darkMaterial);

        Make("DriverSeat", PrimitiveType.Sphere,
            new Vector3(0f, 0.60f, 0.03f),
            new Vector3(0.31f, 0.15f, 0.48f), darkMaterial);

        Make("Headrest", PrimitiveType.Sphere,
            new Vector3(0f, 0.67f, -0.25f),
            new Vector3(0.32f, 0.20f, 0.38f), darkMaterial);

        // Sidepods with a neutral carbon inlet treatment; livery is intentionally kept separate from the base model.
        BuildSidepod(-1f);
        BuildSidepod(1f);

        // Engine cover and airbox.
        Make("EngineCover", PrimitiveType.Sphere,
            new Vector3(0f, 0.59f, -0.65f),
            new Vector3(0.68f, 0.38f, 1.35f), bodyMaterial);

        Make("Airbox", PrimitiveType.Cylinder,
            new Vector3(0f, 0.92f, -0.40f),
            new Vector3(0.23f, 0.20f, 0.23f), darkMaterial);

        Make("AirboxLip", PrimitiveType.Cylinder,
            new Vector3(0f, 1.10f, -0.40f),
            new Vector3(0.28f, 0.045f, 0.28f), accentMaterial);

        // Halo: three beams forming a clean original halo silhouette.
        BuildHalo();

        // Front wing: multi-element and curved-looking through staggered sections.
        frontWing = new GameObject("FrontWing").transform;
        frontWing.SetParent(transform, false);
        frontWing.localPosition = new Vector3(0f, 0.29f, 2.28f);

        MakeWingElement(frontWing, "MainPlane", 1.78f, 0.13f, 0.42f, 0f, 0.00f, bodyMaterial);
        MakeWingElement(frontWing, "UpperPlane", 1.62f, 0.08f, 0.34f, 0.08f, 0.02f, darkMaterial);
        MakeWingElement(frontWing, "LowerPlane", 1.86f, 0.06f, 0.30f, -0.07f, 0.03f, accentMaterial);

        Make("FrontWingEndL", PrimitiveType.Cube,
            new Vector3(-0.91f, 0.38f, 2.28f),
            new Vector3(0.07f, 0.28f, 0.45f), darkMaterial);
        Make("FrontWingEndR", PrimitiveType.Cube,
            new Vector3(0.91f, 0.38f, 2.28f),
            new Vector3(0.07f, 0.28f, 0.45f), darkMaterial);

        // Rear body, diffuser and gearbox area.
        Make("RearBody", PrimitiveType.Sphere,
            new Vector3(0f, 0.49f, -1.12f),
            new Vector3(0.96f, 0.34f, 1.10f), bodyMaterial);

        Make("Diffuser", PrimitiveType.Cube,
            new Vector3(0f, 0.27f, -1.46f),
            new Vector3(1.28f, 0.12f, 0.55f), darkMaterial);

        // Rear wing, three visible planes + tall endplates.
        rearWing = new GameObject("RearWing").transform;
        rearWing.SetParent(transform, false);
        rearWing.localPosition = new Vector3(0f, 1.04f, -1.57f);

        MakeWingElement(rearWing, "MainPlane", 1.56f, 0.10f, 0.24f, 0f, 0f, darkMaterial);
        MakeWingElement(rearWing, "UpperPlane", 1.48f, 0.08f, 0.20f, 0.17f, 0.02f, bodyMaterial);
        MakeWingElement(rearWing, "LowerPlane", 1.52f, 0.07f, 0.18f, -0.16f, 0.03f, darkMaterial);

        Make("RearWingEndL", PrimitiveType.Cube,
            new Vector3(-0.78f, 1.05f, -1.57f),
            new Vector3(0.07f, 0.72f, 0.12f), darkMaterial);
        Make("RearWingEndR", PrimitiveType.Cube,
            new Vector3(0.78f, 1.05f, -1.57f),
            new Vector3(0.07f, 0.72f, 0.12f), darkMaterial);

        MakeBeam("RearWingSupportL",
            new Vector3(-0.20f, 0.55f, -1.40f),
            new Vector3(-0.20f, 1.03f, -1.57f), 0.045f, darkMaterial);
        MakeBeam("RearWingSupportR",
            new Vector3(0.20f, 0.55f, -1.40f),
            new Vector3(0.20f, 1.03f, -1.57f), 0.045f, darkMaterial);

        // Suspension arms and wheels.
        BuildCorner(-1f, 1.18f, true);
        BuildCorner(1f, 1.18f, true);
        BuildCorner(-1f, -1.18f, false);
        BuildCorner(1f, -1.18f, false);

        // Steering wheel visual.
        steeringWheel = new GameObject("SteeringWheel").transform;
        steeringWheel.SetParent(transform, false);
        steeringWheel.localPosition = new Vector3(0f, 0.67f, 0.56f);
        Make("SteeringWheelRing", PrimitiveType.Cylinder,
            steeringWheel.localPosition,
            new Vector3(0.12f, 0.035f, 0.12f), darkMaterial).transform.SetParent(steeringWheel, true);

        if (addVisualController)
            SetupVisualController();
    }

    private void BuildSidepod(float side)
    {
        string s = side < 0f ? "L" : "R";

        Make("Sidepod" + s, PrimitiveType.Sphere,
            new Vector3(0.56f * side, 0.47f, 0.18f),
            new Vector3(0.48f, 0.30f, 1.22f), bodyMaterial);

        // Deep side inlet / cooling mouth.
        Make("SideInlet" + s, PrimitiveType.Sphere,
            new Vector3(0.73f * side, 0.53f, 0.40f),
            new Vector3(0.23f, 0.16f, 0.56f), darkMaterial);

        Make("SideInletGlow" + s, PrimitiveType.Cube,
            new Vector3(0.74f * side, 0.53f, 0.40f),
            new Vector3(0.025f, 0.045f, 0.34f), accentMaterial);

        // Upper body shoulder.
        Make("SideShoulder" + s, PrimitiveType.Cube,
            new Vector3(0.49f * side, 0.61f, -0.34f),
            new Vector3(0.22f, 0.12f, 0.82f), bodyMaterial);
    }

    private void BuildHalo()
    {
        MakeBeam("HaloCenter",
            new Vector3(0f, 0.62f, 0.44f),
            new Vector3(0f, 0.94f, 0.10f), 0.055f, darkMaterial);

        MakeBeam("HaloLeft",
            new Vector3(0f, 0.94f, 0.10f),
            new Vector3(-0.30f, 0.91f, 0.18f), 0.055f, darkMaterial);

        MakeBeam("HaloRight",
            new Vector3(0f, 0.94f, 0.10f),
            new Vector3(0.30f, 0.91f, 0.18f), 0.055f, darkMaterial);

        MakeBeam("HaloTop",
            new Vector3(-0.30f, 0.91f, 0.18f),
            new Vector3(0.30f, 0.91f, 0.18f), 0.055f, darkMaterial);
    }

    private void BuildCorner(float side, float z, bool front)
    {
        string prefix = (front ? "Front" : "Rear") + (side < 0f ? "L" : "R");
        float x = side * 0.82f;

        MakeBeam(prefix + "UpperArm",
            new Vector3(side * 0.37f, 0.50f, z + (front ? -0.15f : 0.15f)),
            new Vector3(x, 0.45f, z), 0.035f, darkMaterial);

        MakeBeam(prefix + "LowerArm",
            new Vector3(side * 0.32f, 0.30f, z + (front ? 0.15f : -0.15f)),
            new Vector3(x, 0.34f, z), 0.035f, darkMaterial);

        GameObject wheel = Make(prefix + "Wheel", PrimitiveType.Cylinder,
            new Vector3(x, 0.36f, z),
            new Vector3(0.37f, 0.18f, 0.37f), tyreMaterial);

        wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        if (front)
        {
            if (side < 0f) frontLeftWheel = wheel.transform;
            else frontRightWheel = wheel.transform;
        }

        // Neutral tyre sidewall accent; teams/liveries can replace this material later.
        Make(prefix + "TyreBand", PrimitiveType.Cylinder,
            new Vector3(x, 0.36f, z + (side < 0f ? -0.19f : 0.19f)),
            new Vector3(0.375f, 0.012f, 0.375f), accentMaterial)
            .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private void MakeWingElement(
        Transform parent, string name, float width, float height, float length,
        float yOffset, float zOffset, Material material)
    {
        GameObject go = Make(name, PrimitiveType.Cube,
            parent.position + new Vector3(0f, yOffset, zOffset),
            new Vector3(width, height, length), material);

        go.transform.SetParent(parent, true);
    }

    private GameObject MakeBeam(
        string name, Vector3 a, Vector3 b, float thickness, Material material)
    {
        Vector3 delta = b - a;
        GameObject go = Make(name, PrimitiveType.Cylinder,
            (a + b) * 0.5f,
            new Vector3(thickness, delta.magnitude * 0.5f, thickness),
            material);

        go.transform.localRotation =
            Quaternion.FromToRotation(Vector3.up, delta.normalized);

        return go;
    }

    private GameObject Make(
        string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.material = material;

        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        parts.Add(go);
        return go;
    }

    private void SetupVisualController()
    {
        F12026CarVisual visual = GetComponent<F12026CarVisual>();
        if (visual == null)
            visual = gameObject.AddComponent<F12026CarVisual>();

        visual.body = transform;
        visual.frontWing = frontWing;
        visual.rearWing = rearWing;
        visual.steeringWheel = steeringWheel;
        visual.frontLeftWheel = frontLeftWheel;
        visual.frontRightWheel = frontRightWheel;
    }

    private void EnsureMaterials()
    {
        if (bodyMaterial == null)
            bodyMaterial = CreateRuntimeMaterial("F1 Body", new Color(0.72f, 0.72f, 0.70f), 0.70f, 0.42f);

        if (darkMaterial == null)
            darkMaterial = CreateRuntimeMaterial("Carbon Black", new Color(0.018f, 0.022f, 0.025f), 0.78f, 0.22f);

        if (tyreMaterial == null)
            tyreMaterial = CreateRuntimeMaterial("Tyre", new Color(0.025f, 0.025f, 0.025f), 0.95f, 0.05f);

        if (accentMaterial == null)
            accentMaterial = CreateRuntimeMaterial("Red Accent", new Color(0.18f, 0.19f, 0.20f), 0.55f, 0.30f);

        if (glassMaterial == null)
            glassMaterial = CreateRuntimeMaterial("Visor", new Color(0.02f, 0.035f, 0.045f), 0.20f, 0.65f);
    }

    private Material CreateRuntimeMaterial(
        string materialName, Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material m = new Material(shader);
        m.name = materialName;

        if (m.HasProperty("_BaseColor"))
            m.SetColor("_BaseColor", color);
        else if (m.HasProperty("_Color"))
            m.SetColor("_Color", color);

        if (m.HasProperty("_Metallic"))
            m.SetFloat("_Metallic", metallic);

        if (m.HasProperty("_Smoothness"))
            m.SetFloat("_Smoothness", smoothness);

        return m;
    }

    private void ClearGenerated()
    {
        for (int i = parts.Count - 1; i >= 0; i--)
        {
            if (parts[i] != null)
                Destroy(parts[i]);
        }

        parts.Clear();

        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = children.Length - 1; i >= 0; i--)
        {
            Transform child = children[i];
            if (child != transform && child.parent == transform)
                Destroy(child.gameObject);
        }

        frontWing = null;
        rearWing = null;
        steeringWheel = null;
        frontLeftWheel = null;
        frontRightWheel = null;
    }
}
