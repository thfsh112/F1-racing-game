using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace F12026.Editor
{
    public static class F12026_ProjectBootstrap
    {
        [MenuItem("F12026/Build Vehicle Test Scene")]
        public static void BuildVehicleTestScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("F12026_Car");
            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 798f;
            rb.centerOfMass = new Vector3(0f, -0.35f, 0f);
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var vehicle = root.AddComponent<F12026_VehiclePhysics>();
            root.AddComponent<F12026_VehicleInput>();
            root.AddComponent<F12026_Telemetry>();
            root.AddComponent<F12026_AeroModel>();
            root.AddComponent<F12026_PowerUnit>();

            CreateWheel(root.transform, "FrontLeft", new Vector3(-0.78f, 0f, 1.55f), true);
            CreateWheel(root.transform, "FrontRight", new Vector3(0.78f, 0f, 1.55f), true);
            CreateWheel(root.transform, "RearLeft", new Vector3(-0.78f, 0f, -1.55f), false);
            CreateWheel(root.transform, "RearRight", new Vector3(0.78f, 0f, -1.55f), false);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "TestTrack";
            floor.transform.localScale = Vector3.one * 8f;

            var light = new GameObject("Sun");
            var sun = light.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var camera = new GameObject("ChaseCamera");
            camera.transform.position = new Vector3(0f, 3f, -7f);
            camera.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            camera.AddComponent<Camera>();

            Selection.activeGameObject = root;
            EditorSceneManager.SaveScene(scene, "Assets/F12026/Scenes/F12026_VehicleTest.unity");
            Debug.Log("F12026 vehicle test scene created.");
        }

        private static void CreateWheel(Transform parent, string name, Vector3 localPosition, bool steering)
        {
            var holder = new GameObject(name);
            holder.transform.SetParent(parent);
            holder.transform.localPosition = localPosition;

            var collider = holder.AddComponent<WheelCollider>();
            collider.radius = 0.34f;
            collider.suspensionDistance = 0.12f;
            collider.mass = 20f;

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mesh.name = name + "_Mesh";
            mesh.transform.SetParent(holder.transform);
            mesh.transform.localPosition = Vector3.zero;
            mesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            mesh.transform.localScale = new Vector3(0.34f, 0.16f, 0.34f);
            Object.DestroyImmediate(mesh.GetComponent<Collider>());
        }
    }
}
