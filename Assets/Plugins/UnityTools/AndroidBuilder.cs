using System;
using System.IO;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Icarealot.UnityTools
{
    /// <summary>
    /// Builds the Android player from outside the Editor. Driven by tools/android-build.sh through
    /// one of two entry points: the `android_build` Pipeline command when an Editor is already open,
    /// or <see cref="BuildFromCommandLine"/> via `unity build --execute-method` when none is.
    ///
    /// Every setting it applies is left in ProjectSettings.asset on purpose — a build is a real edit
    /// to the project, and the resulting git diff is the record of what was built.
    /// </summary>
    public static class AndroidBuilder
    {
        private const string ENV_OUTPUT_PATH = "ANDROID_OUTPUT_PATH";
        private const string ENV_VERSION = "ANDROID_VERSION";
        private const string ENV_BUILD_NUMBER = "ANDROID_BUILD_NUMBER";
        private const string ENV_DEFINES = "ANDROID_DEFINES";
        private const string ENV_DEVELOPMENT = "ANDROID_DEVELOPMENT";
        private const string ENV_KEYSTORE_PATH = "ANDROID_KEYSTORE_PATH";
        private const string ENV_KEYSTORE_PASSWORD = "ANDROID_KEYSTORE_PASSWORD";
        private const string ENV_KEY_ALIAS = "ANDROID_KEY_ALIAS";
        private const string ENV_KEY_ALIAS_PASSWORD = "ANDROID_KEY_ALIAS_PASSWORD";

        private const string AAB_EXTENSION = ".aab";

        /// <summary>
        /// Entry point for an Editor that is already open, reached with
        /// `unity command --detach android_build`. Detaching matters: a MainThreadRequired command
        /// otherwise gets a 60 second budget, which an IL2CPP build will not fit inside.
        /// </summary>
        [CliCommand("android_build",
            "Build the Android player with an explicit version, build number, defines and signing. " +
            "Run it detached — the dispatcher's default budget is far shorter than the build.",
            MainThreadRequired = true,
            Tags = new[] { "build" })]
        public static AndroidBuildOutcome BuildForPipeline(
            [CliArg("outputPath", "Output file, absolute or relative to the project root. A .aab extension builds an App Bundle; anything else builds an APK.", Required = true)] string outputPath,
            [CliArg("version", "Value for PlayerSettings.bundleVersion, e.g. 0.1.0.", Required = true)] string version,
            [CliArg("buildNumber", "Value for PlayerSettings.Android.bundleVersionCode.", Required = true)] int buildNumber,
            [CliArg("defines", "Semicolon-separated Scripting Define Symbols for Android. Empty clears them.")] string defines = "",
            [CliArg("development", "Build a development player (profiler, script debugging, on-device console).")] bool development = false,
            [CliArg("keystorePath", "Custom keystore file. Empty falls back to Unity's debug keystore.")] string keystorePath = "",
            [CliArg("keystorePassword", "Keystore password. Required when keystorePath is given.")] string keystorePassword = "",
            [CliArg("keyAlias", "Key alias inside the keystore. Required when keystorePath is given.")] string keyAlias = "",
            [CliArg("keyAliasPassword", "Key alias password. Defaults to the keystore password.")] string keyAliasPassword = "")
        {
            AndroidBuildRequest request = new()
            {
                OutputPath = outputPath,
                Version = version,
                BuildNumber = buildNumber,
                Defines = defines,
                Development = development,
                KeystorePath = keystorePath,
                KeystorePassword = keystorePassword,
                KeyAlias = keyAlias,
                KeyAliasPassword = keyAliasPassword
            };

            return Build(request);
        }

        /// <summary>
        /// Entry point for a batch Editor, reached with `unity build --execute-method`. Reads its
        /// inputs from the environment, which a freshly spawned Editor inherits from the script.
        /// Exits the Editor itself so the CLI reports a failed build as a failed build.
        /// </summary>
        public static void BuildFromCommandLine()
        {
            AndroidBuildOutcome outcome;

            try
            {
                AndroidBuildRequest request = new()
                {
                    OutputPath = ReadEnvironment(ENV_OUTPUT_PATH),
                    Version = ReadEnvironment(ENV_VERSION),
                    BuildNumber = ReadBuildNumberFromEnvironment(),
                    Defines = ReadEnvironment(ENV_DEFINES),
                    Development = ReadEnvironment(ENV_DEVELOPMENT) == "1",
                    KeystorePath = ReadEnvironment(ENV_KEYSTORE_PATH),
                    KeystorePassword = ReadEnvironment(ENV_KEYSTORE_PASSWORD),
                    KeyAlias = ReadEnvironment(ENV_KEY_ALIAS),
                    KeyAliasPassword = ReadEnvironment(ENV_KEY_ALIAS_PASSWORD)
                };

                outcome = Build(request);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Android build failed: {exception}");
                EditorApplication.Exit(1);
                return;
            }

            if (!outcome.Success)
            {
                Debug.LogError($"Android build failed: {outcome.Message}");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"Android build succeeded: {outcome.Message}");
            EditorApplication.Exit(0);
        }

        private static AndroidBuildOutcome Build(AndroidBuildRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OutputPath))
            {
                throw new ArgumentException("No output path was given.");
            }

            if (string.IsNullOrWhiteSpace(request.Version))
            {
                throw new ArgumentException("No version was given.");
            }

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes in EditorBuildSettings — there is nothing to build.");
            }

            string absoluteOutputPath = ResolveOutputPath(request.OutputPath);
            string outputDirectory = Path.GetDirectoryName(absoluteOutputPath);

            if (!string.IsNullOrEmpty(outputDirectory))
            {
                _ = Directory.CreateDirectory(outputDirectory);
            }

            ApplySettings(request, absoluteOutputPath);

            BuildPlayerOptions options = new()
            {
                scenes = scenes,
                locationPathName = absoluteOutputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = request.Development ? BuildOptions.Development : BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            AndroidBuildOutcome outcome = new()
            {
                Success = summary.result == BuildResult.Succeeded,
                OutputPath = absoluteOutputPath,
                SizeBytes = (long)summary.totalSize,
                DurationSeconds = summary.totalTime.TotalSeconds,
                Errors = summary.totalErrors
            };

            outcome.Message = outcome.Success
                ? $"{absoluteOutputPath} ({summary.totalSize / 1024 / 1024} MB in {summary.totalTime.TotalSeconds:F0}s)"
                : $"{summary.result} with {summary.totalErrors} error(s) — see the Editor log for the reason.";

            return outcome;
        }

        private static void ApplySettings(AndroidBuildRequest request, string absoluteOutputPath)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                _ = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            PlayerSettings.bundleVersion = request.Version;
            PlayerSettings.Android.bundleVersionCode = request.BuildNumber;
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, request.Defines ?? string.Empty);

            EditorUserBuildSettings.buildAppBundle =
                absoluteOutputPath.EndsWith(AAB_EXTENSION, StringComparison.OrdinalIgnoreCase);
            EditorUserBuildSettings.development = request.Development;

            ApplySigning(request);

            AssetDatabase.SaveAssets();
        }

        private static void ApplySigning(AndroidBuildRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.KeystorePath))
            {
                PlayerSettings.Android.useCustomKeystore = false;
                return;
            }

            if (!File.Exists(request.KeystorePath))
            {
                throw new FileNotFoundException($"Keystore not found: {request.KeystorePath}");
            }

            if (string.IsNullOrEmpty(request.KeystorePassword))
            {
                throw new ArgumentException("A keystore was given without a keystore password.");
            }

            if (string.IsNullOrWhiteSpace(request.KeyAlias))
            {
                throw new ArgumentException("A keystore was given without a key alias.");
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = request.KeystorePath;
            PlayerSettings.Android.keystorePass = request.KeystorePassword;
            PlayerSettings.Android.keyaliasName = request.KeyAlias;
            PlayerSettings.Android.keyaliasPass = string.IsNullOrEmpty(request.KeyAliasPassword)
                ? request.KeystorePassword
                : request.KeyAliasPassword;
        }

        private static string ResolveOutputPath(string outputPath)
        {
            if (Path.IsPathRooted(outputPath))
            {
                return outputPath.Replace('\\', '/');
            }

            string projectRoot = Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(projectRoot, outputPath)).Replace('\\', '/');
        }

        private static int ReadBuildNumberFromEnvironment()
        {
            string raw = ReadEnvironment(ENV_BUILD_NUMBER);

            if (!int.TryParse(raw, out int buildNumber))
            {
                throw new ArgumentException($"{ENV_BUILD_NUMBER} is not an integer: '{raw}'");
            }

            return buildNumber;
        }

        private static string ReadEnvironment(string name)
        {
            return Environment.GetEnvironmentVariable(name) ?? string.Empty;
        }
    }

    /// <summary>
    /// The inputs one Android build is made from, however they were supplied.
    /// </summary>
    internal sealed class AndroidBuildRequest
    {
        public string OutputPath { get; set; }
        public string Version { get; set; }
        public int BuildNumber { get; set; }
        public string Defines { get; set; }
        public bool Development { get; set; }
        public string KeystorePath { get; set; }
        public string KeystorePassword { get; set; }
        public string KeyAlias { get; set; }
        public string KeyAliasPassword { get; set; }
    }

    /// <summary>
    /// What one Android build produced, serialized back to the caller by the Pipeline command.
    /// </summary>
    public sealed class AndroidBuildOutcome
    {
        public bool Success { get; set; }
        public string OutputPath { get; set; }
        public string Message { get; set; }
        public long SizeBytes { get; set; }
        public double DurationSeconds { get; set; }
        public int Errors { get; set; }
    }
}
