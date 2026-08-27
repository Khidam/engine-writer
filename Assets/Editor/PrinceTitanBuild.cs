using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrinceTitan.Editor
{
    public static class PrinceTitanBuild
    {
        private const string RuntimeScene = "Assets/PrinceTitan/Scenes/PrinceTitanRuntime.unity";

        [MenuItem("Prince Titan/Create & Open Runtime Scene")]
        public static void CreateAndOpenRuntimeScene()
        {
            EnsureRuntimeScene(true);
        }

        [MenuItem("Prince Titan/Build Windows x64")]
        public static void BuildWindows64()
        {
            WorldSeed.ValidateOrThrow();
            EnsureRuntimeScene(false);
            ConfigurePlayer();

            var output = CommandLineValue("-customBuildPath");
            if (string.IsNullOrWhiteSpace(output)) output = Path.Combine("build", "StandaloneWindows64", "PrinceTitan.exe");
            if (!output.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) output = Path.Combine(output, "PrinceTitan.exe");
            var directory = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { RuntimeScene },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Prince Titan build failed: " + report.summary.result + " / " + report.summary.totalErrors + " errors.");

            Debug.Log("Prince Titan Windows build complete: " + Path.GetFullPath(output));
        }

        private static void EnsureRuntimeScene(bool openAfterSave)
        {
            var directory = Path.GetDirectoryName(RuntimeScene);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, RuntimeScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!openAfterSave) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Khidam";
            PlayerSettings.productName = "Prince Titan";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, "com.khidam.princetitan");
        }

        private static string CommandLineValue(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return string.Empty;
        }
    }
}
