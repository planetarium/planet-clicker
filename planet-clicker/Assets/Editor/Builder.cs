using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor
{
    public class Builder {

        public static string PlayerName = "PlanetClicker";

        [MenuItem("Build/Windows + Mac OSX")]
        public static void BuildAll()
        {
            BuildMacOS();
            BuildWindows();
        }

        [MenuItem("Build/MacOS")]
        public static void BuildMacOS()
        {
            Debug.Log("Build MacOS");
            Build(BuildTarget.StandaloneOSX, targetDirName: "MacOS");
        }

        [MenuItem("Build/Windows")]
        public static void BuildWindows()
        {
            Debug.Log("Build Windows");
            Build(BuildTarget.StandaloneWindows64, targetDirName: "Windows");
        }

        [MenuItem("Build/Linux Headless")]
        public static void BuildLinuxHeadless()
        {
            Debug.Log("Build Linux Headless");
            Build(BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode, "LinuxHeadless");
        }

        public static void Build(
            BuildTarget buildTarget,
            BuildOptions options = BuildOptions.None,
            string targetDirName = null)
        {
            string[] scenes = { "Assets/Scenes/Game.unity" };
            const string basePath = "Build";
            string locationPathName = Path.Combine(
                basePath,
                targetDirName ?? buildTarget.ToString(),
                buildTarget.HasFlag(BuildTarget.StandaloneWindows64) ? $"{PlayerName}.exe" : PlayerName);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = locationPathName,
                target = buildTarget,
                options = options | BuildOptions.Development,
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed");
            }
        }
    }

}
