using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;

// =========================================================
//  Dark Sea - UI Scene Setup (Editor Tool)
//
//  Unity menu bar: DarkSea > Setup UI Scenes
//
//  Run this AFTER Unity has finished compiling all scripts.
//  It will:
//   1. Find or create a PanelSettings asset.
//   2. Update Assets/Scenes/MainMenu.unity   (adds UIDocument,
//      disables old Canvas-based UI).
//   3. Create  Assets/Scenes/LevelSelect.unity.
//   4. Add LevelSelect to Build Settings.
// =========================================================
public static class DarkSeaUISetup
{
    [MenuItem("DarkSea/Setup UI Scenes")]
    public static void SetupUIScenes()
    {
        // 1 — PanelSettings
        PanelSettings ps = GetOrCreatePanelSettings();

        // 2 — MainMenu
        SetupScene(
            scenePath:    "Assets/Scenes/MainMenu.unity",
            uiGoName:     "MainMenuUI",
            uxmlPath:     "Assets/UI/MainMenu.uxml",
            scriptClass:  "MainMenuUI",
            panelSettings: ps,
            isExisting:   true
        );

        // 3 — LevelSelect
        SetupScene(
            scenePath:    "Assets/Scenes/LevelSelect.unity",
            uiGoName:     "LevelSelectUI",
            uxmlPath:     "Assets/UI/LevelSelect.uxml",
            scriptClass:  "LevelSelectUI",
            panelSettings: ps,
            isExisting:   false
        );

        // 4 — Build Settings
        AddSceneToBuildSettings("Assets/Scenes/LevelSelect.unity");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DarkSea] UI scene setup complete!");
    }

    // -------------------------------------------------------

    static PanelSettings GetOrCreatePanelSettings()
    {
        // Look for an existing PanelSettings anywhere in the project
        string[] guids = AssetDatabase.FindAssets("t:PanelSettings");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Debug.Log("[DarkSea] Using PanelSettings: " + path);
            return AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
        }

        // Create one with sensible defaults
        var ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.scaleMode           = PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1920, 1080);
        ps.screenMatchMode     = PanelScreenMatchMode.MatchWidthOrHeight;
        ps.match               = 0.5f;

        if (!AssetDatabase.IsValidFolder("Assets/UI"))
            AssetDatabase.CreateFolder("Assets", "UI");

        AssetDatabase.CreateAsset(ps, "Assets/UI/DarkSea PanelSettings.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("[DarkSea] Created PanelSettings at Assets/UI/DarkSea PanelSettings.asset");
        return ps;
    }

    static void SetupScene(string scenePath, string uiGoName, string uxmlPath,
                           string scriptClass, PanelSettings panelSettings, bool isExisting)
    {
        // Open or create the scene
        UnityEngine.SceneManagement.Scene scene;
        if (isExisting && File.Exists(Path.Combine(Application.dataPath, "..", scenePath)))
        {
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }

        // Disable existing Canvas-based UI on the MainMenu scene so it doesn't overlap
        if (isExisting)
        {
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                canvas.gameObject.SetActive(false);
                Debug.Log("[DarkSea] Disabled old Canvas: " + canvas.gameObject.name);
            }
        }

        // Skip if UI GameObject already exists
        if (GameObject.Find(uiGoName) != null)
        {
            Debug.Log("[DarkSea] " + uiGoName + " already present — skipping creation.");
        }
        else
        {
            var go    = new GameObject(uiGoName);
            var uiDoc = go.AddComponent<UIDocument>();

            // Link UXML
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if (uxml != null)
                uiDoc.visualTreeAsset = uxml;
            else
                Debug.LogError("[DarkSea] UXML not found: " + uxmlPath);

            uiDoc.panelSettings = panelSettings;

            // Add the C# script component
            System.Type type = System.Type.GetType(scriptClass + ", Assembly-CSharp");
            if (type != null)
                go.AddComponent(type);
            else
                Debug.LogWarning("[DarkSea] Script type not found: " + scriptClass +
                                 " — add it manually to " + uiGoName + ".");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[DarkSea] Saved scene: " + scenePath);
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var list = EditorBuildSettings.scenes.ToList();
        if (list.All(s => s.path != scenePath))
        {
            list.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log("[DarkSea] Added to Build Settings: " + scenePath);
        }
    }
}
