using UnityEngine;

public class F12026OriginalCarModel : MonoBehaviour
{
    public Material bodyMaterial;
    public Material darkMaterial;
    public Material tyreMaterial;

    private void Start()
    {
        Build();
    }

    private void Build()
    {
        Make("Chassis", PrimitiveType.Cube, new Vector3(0,0.34f,0), new Vector3(1.05f,0.22f,3.35f), bodyMaterial);
        Make("Nose", PrimitiveType.Cube, new Vector3(0,0.40f,1.65f), new Vector3(0.58f,0.18f,1.35f), bodyMaterial);
        Make("Cockpit", PrimitiveType.Cube, new Vector3(0,0.55f,0.35f), new Vector3(0.58f,0.22f,0.95f), darkMaterial);
        Make("SidepodL", PrimitiveType.Cube, new Vector3(-0.58f,0.43f,0.25f), new Vector3(0.32f,0.26f,1.25f), bodyMaterial);
        Make("SidepodR", PrimitiveType.Cube, new Vector3(0.58f,0.43f,0.25f), new Vector3(0.32f,0.26f,1.25f), bodyMaterial);
        Make("RearBody", PrimitiveType.Cube, new Vector3(0,0.48f,-1.10f), new Vector3(0.92f,0.30f,1.10f), bodyMaterial);

        Make("FrontWing", PrimitiveType.Cube, new Vector3(0,0.28f,2.25f), new Vector3(1.75f,0.08f,0.38f), bodyMaterial);
        Make("RearWing", PrimitiveType.Cube, new Vector3(0,1.00f,-1.62f), new Vector3(1.48f,0.58f,0.10f), bodyMaterial);
        Make("RearWingSupport", PrimitiveType.Cube, new Vector3(0,0.70f,-1.60f), new Vector3(0.10f,0.60f,0.10f), darkMaterial);

        CreateWheel("FL", -0.78f, 0.31f, 1.18f);
        CreateWheel("FR",  0.78f, 0.31f, 1.18f);
        CreateWheel("RL", -0.78f, 0.31f,-1.18f);
        CreateWheel("RR",  0.78f, 0.31f,-1.18f);
    }

    private void CreateWheel(string n,float x,float y,float z)
    {
        var go=Make(n,PrimitiveType.Cylinder,new Vector3(x,y,z),new Vector3(0.34f,0.16f,0.34f),tyreMaterial);
        go.transform.localRotation=Quaternion.Euler(90f,0f,0f);
    }

    private GameObject Make(string n,PrimitiveType type,Vector3 pos,Vector3 scale,Material mat)
    {
        GameObject go=GameObject.CreatePrimitive(type);
        go.name=n;
        go.transform.SetParent(transform,false);
        go.transform.localPosition=pos;
        go.transform.localScale=scale;
        if(mat!=null) go.GetComponent<Renderer>().material=mat;
        Destroy(go.GetComponent<Collider>());
        return go;
    }
}
