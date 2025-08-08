// using System.IO;
// using System.Linq;
// using UnityEditor;
// using UnityEditor.Build.Reporting;
// using UnityEngine;

// public static class MultiPlatformBuild
// {
//     private const string ApkPath = "~/Downloads/WebGL/Android/crazy-coral.apk"; // change name/path
//     private const string WebGLPath = "~/Downloads/WebGL"; // folder

//     // Menu item you can click
//     [MenuItem("Build/Build APK then WebGL")]
//     public static void BuildApkThenWebGL()
//     {
//         // Save original target so we can restore later
//         var originalGroup  = EditorUserBuildSettings.selectedBuildTargetGroup;
//         var originalTarget = EditorUserBuildSettings.activeBuildTarget;
//         try
//         {
//             EnsureOutputDirs();

//             // 1) ANDROID
//             SwitchTarget(BuildTargetGroup.Android, BuildTarget.Android);
//             // Toggle AAB if you prefer a bundle:
//             // EditorUserBuildSettings.buildAppBundle = true;
//             BuildOrThrow(
//                 new BuildPlayerOptions {
//                     scenes = GetEnabledScenes(),
//                     locationPathName = ApkPath,
//                     target = BuildTarget.Android,
//                     options = BuildOptions.None // add Development, AllowDebugging, etc. if needed
//                 }
//             );

//             // 2) WEBGL
//             SwitchTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
//             BuildOrThrow(
//                 new BuildPlayerOptions {
//                     scenes = GetEnabledScenes(),
//                     locationPathName = WebGLPath,
//                     target = BuildTarget.WebGL,
//                     options = BuildOptions.None
//                 }
//             );

//             Debug.Log("<color=green>✅ APK and WebGL builds completed.</color>");
//         }
//         finally
//         {
//             // Restore original target no matter what happened
//             EditorUserBuildSettings.SwitchActiveBuildTarget(originalGroup, originalTarget);
//         }
//     }

//     // ----- helpers -----

//     private static string[] GetEnabledScenes() =>
//         EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

//     private static void SwitchTarget(BuildTargetGroup group, BuildTarget target)
//     {
//         if (EditorUserBuildSettings.activeBuildTarget == target) return;
//         Debug.Log($"Switching to {target}…");
//         EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
//     }

//     private static void EnsureOutputDirs()
//     {
//         var apkDir = Path.GetDirectoryName(ApkPath);
//         if (!string.IsNullOrEmpty(apkDir) && !Directory.Exists(apkDir))
//             Directory.CreateDirectory(apkDir);

//         if (!Directory.Exists(WebGLPath))
//             Directory.CreateDirectory(WebGLPath);
//     }

//     private static void BuildOrThrow(BuildPlayerOptions opts)
//     {
//         var report = BuildPipeline.BuildPlayer(opts);
//         var summary = report.summary;

//         if (summary.result == BuildResult.Succeeded)
//         {
//             Debug.Log($"✅ {summary.platform} build OK: size {summary.totalSize/ (1024f*1024f):0.0} MB");
//         }
//         else
//         {
//             var msg = $"❌ {summary.platform} build failed: {summary.result}";
//             Debug.LogError(msg);
//             throw new System.Exception(msg);
//         }
//     }
// }
