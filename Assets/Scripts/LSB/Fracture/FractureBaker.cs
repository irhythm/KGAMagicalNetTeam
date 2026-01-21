#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 에디터 전용 툴: 오브젝트를 미리 파괴하여 "하이브리드 프리팹"을 생성합니다.
/// 하이브리드 프리팹 = [멀쩡한 모델(Solid)] + [파편 모델(Fractured, 비활성화)]
/// </summary>
public class FractureBaker : EditorWindow
{
    private GameObject targetObject;       // 파괴할 원본 오브젝트
    private int chunkCount = 100;          // 생성할 파편 수
    private float density = 50f;           // 밀도
    private float breakForce = 50f;        // 파편 결합력

    private string savePath = "Assets/Resources/DestructibleProps/"; // 프리팹 저장 경로

    private Material insideMat;            // 절단면 머티리얼
    private Material outsideMat;           // 겉면 머티리얼

    // 에디터 메뉴에 툴 등록
    [MenuItem("Tools/Fracture Baker (Hybrid)")]
    public static void ShowWindow()
    {
        GetWindow<FractureBaker>("Fracture Baker");
    }

    // GUI 그리기
    private void OnGUI()
    {
        GUILayout.Label("Hybrid Prefab Baker", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 타겟 오브젝트 선택 필드
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        GUILayout.Space(5);
        chunkCount = EditorGUILayout.IntField("Chunk Count", chunkCount);
        density = EditorGUILayout.FloatField("Density", density);

        GUILayout.Space(5);
        insideMat = (Material)EditorGUILayout.ObjectField("Inside Mat", insideMat, typeof(Material), false);
        outsideMat = (Material)EditorGUILayout.ObjectField("Outside Mat", outsideMat, typeof(Material), false);

        GUILayout.Space(10);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        GUILayout.Space(20);
        if (GUILayout.Button("Bake Hybrid Prefab", GUILayout.Height(40)))
        {
            if (targetObject == null) return;
            BakeHybrid(); // 베이킹 시작
        }
    }

    /// <summary>
    /// 실제 베이킹 로직: Solid + Fractured 구조 생성 및 저장
    /// </summary>
    private void BakeHybrid()
    {
        // 저장 경로 폴더 생성
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        // 머티리얼 자동 할당 (없으면 타겟의 것 사용)
        if (insideMat == null) insideMat = targetObject.GetComponent<MeshRenderer>().sharedMaterial;
        if (outsideMat == null) outsideMat = targetObject.GetComponent<MeshRenderer>().sharedMaterial;

        // 루트 오브젝트 생성
        GameObject root = new GameObject(targetObject.name + "_Destructible");

        // 멀쩡한 모델(Solid) 복제하여 자식으로 넣기
        GameObject solid = Instantiate(targetObject, root.transform);
        solid.name = "Solid_Model";
        solid.transform.localPosition = Vector3.zero;
        solid.transform.localRotation = Quaternion.identity;
        solid.transform.localScale = Vector3.one;

        // 파편 모델(Fractured) 생성
        int seed = Random.Range(0, 999999);
        ChunkGraphManager fracturedManager = Fracture.FractureGameObject(
            targetObject, Anchor.Bottom, seed, chunkCount,
            insideMat, outsideMat, breakForce, density
        );

        GameObject fractured = fracturedManager.gameObject;
        fractured.name = "Fractured_Root";
        fractured.transform.SetParent(root.transform);
        fractured.transform.localPosition = Vector3.zero;
        fractured.transform.localRotation = Quaternion.identity;
        fractured.transform.localScale = Vector3.one;

        // 이웃 관계(Neighbours) 데이터를 배열로 직렬화 (프리팹 저장용)
        ChunkNode[] allNodes = fractured.GetComponentsInChildren<ChunkNode>();
        foreach (var node in allNodes)
        {
            if (node != null) node.SyncNeighboursToArray(); // HashSet -> Array 변환
        }

        // 매니저에 노드 리스트 등록
        fracturedManager.Setup(allNodes);

        // 모든 파편을 Kinematic(물리 영향 안 받음)으로 설정
        Rigidbody[] rbs = fractured.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs) rb.isKinematic = true;

        // 생성된 메쉬를 실제 에셋 파일(.asset)로 저장
        SaveMeshesToAssets(fractured, root.name + "_" + seed);

        // 런타임 제어 스크립트(DestructibleWall) 부착 및 설정
        DestructibleWall script = root.AddComponent<DestructibleWall>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("solidModel").objectReferenceValue = solid;
        so.FindProperty("fracturedRoot").objectReferenceValue = fractured;
        so.ApplyModifiedProperties();

        // 파편 루트는 평소에 안 보이게 비활성화
        fractured.SetActive(false);

        // 프리팹 파일로 저장
        string finalPath = $"{savePath}/{root.name}_{seed}.prefab".Replace("//", "/");
        PrefabUtility.SaveAsPrefabAsset(root, finalPath);

        // 씬에 있는 임시 객체 삭제 및 정리
        DestroyImmediate(root);
        AssetDatabase.Refresh();
        Debug.Log($"하이브리드 프리팹 생성 완료 (메쉬 저장됨): {finalPath}");
    }

    /// <summary>
    /// 생성된 절차적 메쉬들을 에셋 파일로 추출하여 저장합니다.
    /// </summary>
    private void SaveMeshesToAssets(GameObject fracturedRoot, string baseName)
    {
        string meshFolder = $"{savePath}/Meshes/{baseName}".Replace("//", "/");
        if (!Directory.Exists(meshFolder)) Directory.CreateDirectory(meshFolder);

        MeshFilter[] filters = fracturedRoot.GetComponentsInChildren<MeshFilter>();

        for (int i = 0; i < filters.Length; i++)
        {
            Mesh mesh = filters[i].sharedMesh;
            if (mesh == null) continue;

            // 이미 저장된 에셋이면 패스
            if (AssetDatabase.Contains(mesh)) continue;

            string assetPath = $"{meshFolder}/Chunk_{i}.asset";
            AssetDatabase.CreateAsset(mesh, assetPath);
        }
        AssetDatabase.SaveAssets();
    }
}
#endif