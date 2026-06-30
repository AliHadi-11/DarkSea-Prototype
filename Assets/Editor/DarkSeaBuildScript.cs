using UnityEditor;
using UnityEngine;
using System.IO;

public class DarkSeaBuildScript
{
    static readonly string[] scenes = new[]
    {
        "Assets/Scenes/RegisterScene.unity",
        "Assets/Scenes/LoginScene.unity",
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/LevelSelect.unity",
        "Assets/Scenes/Level1_Transition.unity",
        "Assets/Scenes/DarkSea.unity",
        "Assets/Scenes/Level2_Transition.unity",
        "Assets/Scenes/Level_2.unity",
        "Assets/Scenes/Level3_Transition.unity",
        "Assets/Scenes/Level_3.unity"
    };

    [MenuItem("DarkSea/Build WebGL (Web)")]
    public static void BuildWebGL()
    {
        // Step 1: Force switch to WebGL platform first
        bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.WebGL, BuildTarget.WebGL);

        if (!switched)
        {
            Debug.LogError("[DarkSea Build] Could not switch to WebGL platform. " +
                           "Make sure WebGL Build Support module is installed via Unity Hub.");
            return;
        }

        Debug.Log("[DarkSea Build] Platform switched to WebGL successfully.");

        // Step 2: Fix settings for WebGL
        PlayerSettings.companyName = "UOL_FYP";
        PlayerSettings.productName = "Dark Sea";
        PlayerSettings.colorSpace  = ColorSpace.Gamma;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

        // Step 3: Set output path
        string buildPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "DarkSea_WebGL_Build"
        );

        if (Directory.Exists(buildPath))
            Directory.Delete(buildPath, true);
        Directory.CreateDirectory(buildPath);

        // Step 4: Build
        var options = new BuildPlayerOptions
        {
            scenes           = scenes,
            locationPathName = buildPath,
            target           = BuildTarget.WebGL,
            options          = BuildOptions.None
        };

        Debug.Log("[DarkSea Build] Building → " + buildPath);

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log("[DarkSea Build] SUCCESS! Desktop → DarkSea_WebGL_Build");
        else
            Debug.LogError("[DarkSea Build] FAILED with " + report.summary.totalErrors +
                           " errors. Check Console for details.");
    }

    [MenuItem("DarkSea/Build Windows EXE")]
    public static void BuildWindows()
    {
        string buildPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "DarkSea_Windows_Build",
            "DarkSea.exe"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));

        PlayerSettings.companyName = "UOL_FYP";
        PlayerSettings.productName = "Dark Sea";

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log("[DarkSea Build] Starting Windows build → " + buildPath);

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log("[DarkSea Build] SUCCESS! EXE saved to: " + buildPath);
        else
            Debug.LogError("[DarkSea Build] FAILED: " + report.summary.totalErrors + " errors.");
    }
}
