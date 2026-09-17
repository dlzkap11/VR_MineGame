using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Low Poly Nature Bundle의 URP 설정을 문서(__READ_ME/Documentation.pdf, "Unity URP > Fix Pink Materials")대로 적용한다.
/// Render Pipeline Converter와 같은 업그레이더를 쓰되, 이 번들 폴더의 머티리얼만 변환한다.
/// </summary>
public static class LowPolyNatureURPFixer
{
    const string BundleFolder = "Assets/Imports/LMHPOLY/Low Poly Nature Bundle";
    const string GrassPlaneFolder = BundleFolder + "/Vegetation/Vegetation Assets/Materials";
    const string UTerrainMaterialPath = BundleFolder + "/Modular Terrain/Terrain_Assets/Materials/U_Terrain.mat";
    const string UTerrainPrefabFolder = BundleFolder + "/Modular Terrain/Terrain_Assets/Prefabs/Terrain/U";
    const string LitShaderName = "Universal Render Pipeline/Lit";
    const string TerrainLitShaderName = "Universal Render Pipeline/Terrain/Lit";

    [MenuItem("Tools/Low Poly Nature/Fix Materials for URP")]
    public static void Fix()
    {
        if (!GraphicsSettings.isScriptableRenderPipelineEnabled)
        {
            Debug.LogError("[LowPolyNature] URP가 활성화되어 있지 않습니다.");
            return;
        }

        var log = new StringBuilder();
        try
        {
            UpgradeMaterials(log);
            EnableGrassPlaneAlphaClipping(log);
            ApplyUTerrainMaterial(log);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        // 문서의 "Reimport" 단계: 프리팹 미리보기를 갱신한다.
        AssetDatabase.ImportAsset(BundleFolder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);

        Debug.Log("[LowPolyNature] 완료\n" + log);
    }

    static void UpgradeMaterials(StringBuilder log)
    {
        List<MaterialUpgrader> upgraders = MaterialUpgrader.FetchAllUpgradersForPipeline(GraphicsSettings.currentRenderPipelineAssetType);
        Shader lit = Shader.Find(LitShaderName);

        int upgraded = 0;
        var skipped = new List<string>();
        var lostValues = new List<string>();

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { BundleFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            EditorUtility.DisplayProgressBar("Low Poly Nature - Material Upgrade", path, (float)i / guids.Length);

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
                continue;

            string message = string.Empty;
            if (MaterialUpgrader.Upgrade(mat, upgraders, MaterialUpgrader.UpgradeFlags.None, ref message))
            {
                EditorUtility.SetDirty(mat);
                upgraded++;
                continue;
            }

            skipped.Add($"{path} ({mat.shader.name})");

            // 셰이더만 Lit으로 바꿔 원본 텍스처/색상이 지워진 머티리얼 (패키지 재임포트 필요)
            if (mat.shader == lit && mat.GetTexture("_BaseMap") == null && mat.GetColor("_BaseColor") == Color.white)
                lostValues.Add(path);
        }

        log.AppendLine($"머티리얼 변환: {upgraded}개, 건너뜀: {skipped.Count}개");
        foreach (string s in skipped)
            log.AppendLine("  skip: " + s);

        if (lostValues.Count > 0)
            Debug.LogWarning("[LowPolyNature] 텍스처/색상이 비어 있는 Lit 머티리얼입니다. 원래 흰색인 머티리얼이 아니라면 패키지에서 원본을 재임포트한 뒤 다시 실행하세요.\n  "
                             + string.Join("\n  ", lostValues));
    }

    static void EnableGrassPlaneAlphaClipping(StringBuilder log)
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("GrassPlane t:Material", new[] { GrassPlaneFolder }))
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (mat == null || mat.shader.name != LitShaderName)
                continue;

            mat.SetFloat("_AlphaClip", 1f);
            mat.SetFloat("_Cutoff", 0.5f);
            BaseShaderGUI.SetMaterialKeywords(mat, LitGUI.SetMaterialKeywords);
            EditorUtility.SetDirty(mat);
            count++;
        }

        log.AppendLine($"GrassPlane Alpha Clipping: {count}개");
    }

    static void ApplyUTerrainMaterial(StringBuilder log)
    {
        Material terrainMat = AssetDatabase.LoadAssetAtPath<Material>(UTerrainMaterialPath);
        if (terrainMat == null)
        {
            Shader terrainLit = Shader.Find(TerrainLitShaderName);
            if (terrainLit == null)
            {
                Debug.LogError($"[LowPolyNature] {TerrainLitShaderName} 셰이더를 찾을 수 없습니다.");
                return;
            }

            terrainMat = new Material(terrainLit) { name = Path.GetFileNameWithoutExtension(UTerrainMaterialPath) };
            AssetDatabase.CreateAsset(terrainMat, UTerrainMaterialPath);
        }

        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { UTerrainPrefabFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                bool changed = false;
                foreach (Terrain terrain in root.GetComponentsInChildren<Terrain>(true))
                {
                    if (terrain.materialTemplate == terrainMat)
                        continue;

                    terrain.materialTemplate = terrainMat;
                    changed = true;
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    count++;
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        log.AppendLine($"U_Terrain 머티리얼 적용 프리팹: {count}개");
    }
}
