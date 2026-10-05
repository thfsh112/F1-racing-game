using UnityEngine;

/// <summary>
/// Blue 2026 concept-car variant inspired by the user's supplied reference.
/// Uses the shared procedural chassis so it can coexist with other car variants.
/// </summary>
[DisallowMultipleComponent]
public class F12026BlueConceptCarModel : MonoBehaviour
{
    public bool buildOnAwake = true;

    [Header("Blue concept palette")]
    public Color bodyColor = new Color(0.015f, 0.10f, 0.48f);
    public Color carbonColor = new Color(0.012f, 0.015f, 0.020f);
    public Color accentColor = new Color(0.96f, 0.72f, 0.04f);
    public Color whiteColor = new Color(0.92f, 0.94f, 0.96f);

    private void Awake()
    {
        if (buildOnAwake)
            Build();
    }

    [ContextMenu("Build Blue Concept")]
    public void Build()
    {
        F12026OriginalCarModel baseModel =
            GetComponent<F12026OriginalCarModel>();

        if (baseModel == null)
            baseModel = gameObject.AddComponent<F12026OriginalCarModel>();

        baseModel.buildOnStart = false;
        baseModel.bodyMaterial = MakeMaterial(
            "Blue Body", bodyColor, 0.72f, 0.72f);
        baseModel.darkMaterial = MakeMaterial(
            "Blue Carbon", carbonColor, 0.82f, 0.55f);
        baseModel.tyreMaterial = MakeMaterial(
            "Pirelli Style Tyre", new Color(0.018f,0.018f,0.018f), 0.92f, 0.20f);
        baseModel.accentMaterial = MakeMaterial(
            "Yellow Accent", accentColor, 0.45f, 0.55f);
        baseModel.glassMaterial = MakeMaterial(
            "Visor Blue", new Color(0.01f,0.025f,0.06f), 0.35f, 0.75f);

        baseModel.Build();

        AddBlueSpecificDetails();
    }

    private void AddBlueSpecificDetails()
    {
        AddPanel("BlueFloorEdgeL",
            new Vector3(-0.86f, 0.23f, 0.10f),
            new Vector3(0.08f, 0.07f, 3.10f));

        AddPanel("BlueFloorEdgeR",
            new Vector3(0.86f, 0.23f, 0.10f),
            new Vector3(0.08f, 0.07f, 3.10f));

        AddPanel("BlueSideHighlightL",
            new Vector3(-0.60f, 0.57f, 0.08f),
            new Vector3(0.035f, 0.025f, 1.60f));

        AddPanel("BlueSideHighlightR",
            new Vector3(0.60f, 0.57f, 0.08f),
            new Vector3(0.035f, 0.025f, 1.60f));

        AddPanel("WhiteNoseStripe",
            new Vector3(0f, 0.555f, 1.56f),
            new Vector3(0.075f, 0.025f, 0.82f));

        AddPanel("YellowNoseStripe",
            new Vector3(0f, 0.568f, 1.14f),
            new Vector3(0.035f, 0.020f, 0.34f));
    }

    private void AddPanel(string name, Vector3 position, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material = name.Contains("Yellow")
                ? FindMaterial("Yellow Accent")
                : name.Contains("White")
                    ? FindMaterial("White")
                    : FindMaterial("Blue Body");

        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);
    }

    private Material FindMaterial(string type)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (r.material != null && r.material.name.StartsWith(type))
                return r.material;
        }

        return null;
    }

    private Material MakeMaterial(
        string name, Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material m = new Material(shader);
        m.name = name;
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
}
