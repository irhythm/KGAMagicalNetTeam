#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class FractureBaker : EditorWindow
{
    private GameObject targetObject;
    private int chunkCount = 100;
    private float density = 50f;
    private float breakForce = 50f;
    private float debrisLifetime = 10f;

    // 앵커 설정
    private Anchor anchor = Anchor.Bottom;

    private string savePath = "Assets/Resources/DestructibleProps/";
    private Material insideMat;
    private Material outsideMat;

    [MenuItem("Tools/Fracture Baker (Fractured Only)")]
    public static void ShowWindow()
    {
        GetWindow<FractureBaker>("Fracture Baker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Fractured Prefab Baker", EditorStyles.boldLabel);
        GUILayout.Space(10);

        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        GUILayout.Space(5);
        chunkCount = EditorGUILayout.IntField("Chunk Count", chunkCount);
        density = EditorGUILayout.FloatField("Density", density);

        debrisLifetime = EditorGUILayout.FloatField("Debris Lifetime (sec)", debrisLifetime);

        anchor = (Anchor)EditorGUILayout.EnumFlagsField("Anchor Mask", anchor);

        GUILayout.Space(5);
        insideMat = (Material)EditorGUILayout.ObjectField("Inside Mat", insideMat, typeof(Material), false);
        outsideMat = (Material)EditorGUILayout.ObjectField("Outside Mat", outsideMat, typeof(Material), false);

        GUILayout.Space(10);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        GUILayout.Space(20);
        if (GUILayout.Button("Bake Fractured Prefab", GUILayout.Height(40)))
        {
            if (targetObject == null) return;
            BakeFracturedOnly();
        }
    }

    private void BakeFracturedOnly()
    {
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        if (insideMat == null) insideMat = targetObject.GetComponent<MeshRenderer>().sharedMaterial;
        if (outsideMat == null) outsideMat = targetObject.GetComponent<MeshRenderer>().sharedMaterial;

        // 루트 오브젝트 생성
        // 파편 생성 전, 루트를 원본과 똑같은 위치/회전으로 만듦
        GameObject root = new GameObject(targetObject.name + "_Destructible");
        root.transform.position = targetObject.transform.position;
        root.transform.rotation = targetObject.transform.rotation;
        root.transform.localScale = targetObject.transform.localScale;

        int seed = Random.Range(0, 999999);

        // 파편 생성
        ChunkGraphManager fracturedManager = Fracture.FractureGameObject(
            targetObject, anchor, seed, chunkCount,
            insideMat, outsideMat, breakForce, density, debrisLifetime
        );

        GameObject fracturedObj = fracturedManager.gameObject;

        // 생성된 파편들을 루트의 자식으로 옮김
        ChunkNode[] children = fracturedObj.GetComponentsInChildren<ChunkNode>();
        foreach (var child in children)
        {
            child.transform.SetParent(root.transform);
        }

        // 임시 껍데기 삭제
        DestroyImmediate(fracturedObj);

        // 파편을 다 담았으니, 루트를 월드 (0,0,0)으로 이동
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        // 매니저 및 노드 설정
        ChunkGraphManager newManager = root.AddComponent<ChunkGraphManager>();
        ChunkNode[] allNodes = root.GetComponentsInChildren<ChunkNode>();

        foreach (var node in allNodes)
        {
            if (node != null) node.SyncNeighboursToArray();
        }

        newManager.Setup(allNodes);

        // 물리 설정 (Kinematic)
        Rigidbody[] rbs = root.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs) rb.isKinematic = true;

        // 메쉬 에셋 저장 (필수)
        SaveMeshesToAssets(root, targetObject.name + "_" + seed);

        // 식별용 스크립트 추가
        root.AddComponent<DestructibleWall>();

        // 프리팹 파일 저장
        string finalPath = $"{savePath}/{targetObject.name}_{seed}.prefab".Replace("//", "/");
        PrefabUtility.SaveAsPrefabAsset(root, finalPath);

        DestroyImmediate(root);
        AssetDatabase.Refresh();
        Debug.Log($"[Bake] 위치 보정된 프리팹 생성 완료: {finalPath}");
    }

    private void SaveMeshesToAssets(GameObject root, string baseName)
    {
        string meshFolder = $"{savePath}/Meshes/{baseName}".Replace("//", "/");
        if (!Directory.Exists(meshFolder)) Directory.CreateDirectory(meshFolder);

        MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();

        for (int i = 0; i < filters.Length; i++)
        {
            Mesh mesh = filters[i].sharedMesh;
            if (mesh == null) continue;
            if (AssetDatabase.Contains(mesh)) continue;

            string assetPath = $"{meshFolder}/Chunk_{i}.asset";
            AssetDatabase.CreateAsset(mesh, assetPath);
        }
        AssetDatabase.SaveAssets();
    }
}
#endif