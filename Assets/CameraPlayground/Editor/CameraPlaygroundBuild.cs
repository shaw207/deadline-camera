using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CameraPlaygroundBuild
{
    static readonly string[] Scenes = {
        "Assets/CameraPlayground/CameraPlayground.unity"
    };

    [MenuItem("DeadLine/Compilar el laboratorio de cámara para Windows")]
    public static void BuildWindows()
    {
        foreach (var path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path);
            var components = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true));
            foreach (var item in components)
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject) != 0)
                    throw new InvalidDataException("Falta un componente en " + path + " / " + item.name);
            if (!Camera.main || !Camera.main.GetComponent<CameraController>())
                throw new InvalidDataException("Falta la cámara original en " + path);
        }
        EditorBuildSettings.scenes = Scenes.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
        PlayerSettings.companyName = "Chengxun Wu";
        PlayerSettings.productName = "DeadLine - Cámara lateral";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        var output = Environment.GetEnvironmentVariable("DEADLINE_SMOOTH_BUILD");
        if (string.IsNullOrWhiteSpace(output)) output = Path.GetFullPath("Build/Windows");
        Directory.CreateDirectory(output);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = Scenes,
            locationPathName = Path.Combine(output, "DeadLine-Camera-Spline3.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded) throw new IOException(report.summary.result.ToString());
        Debug.Log("Windows camera playground ready: " + report.summary.totalSize + " bytes");
    }
}
