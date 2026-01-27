using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildScript
{
    public static void BuildAndroid()
    {
        Debug.Log("Build android started");

        EditorUserBuildSettings.SwitchActiveBuildTarget(
        BuildTargetGroup.Android,
        BuildTarget.Android
        );


        // === CONFIG SIGNATURE ===
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = "user.keystore";
        PlayerSettings.Android.keystorePass = System.Environment.GetEnvironmentVariable("FREEINGBIRDS_KEYSTORE_PASS");
        PlayerSettings.Android.keyaliasName = "key1";
        PlayerSettings.Android.keyaliasPass = System.Environment.GetEnvironmentVariable("FREEINGBIRDS_KEYALIAS_PASS");

        // === FORMAT GOOGLE PLAY ===
        EditorUserBuildSettings.buildAppBundle = true; // AAB
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        string[] vScenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

        if (HasCommandLineArg("-incrementVersion"))
        {
            PlayerSettings.Android.bundleVersionCode += 1;
            Debug.Log($"Android bundleVersionCode incrémenté : {PlayerSettings.Android.bundleVersionCode}");
        }

        // === BUILD ===
        BuildReport vReport = BuildPipeline.BuildPlayer(
            vScenes,
            "../Builds/FreeingBirds/AsBundle/FreeingBirds.aab",
            BuildTarget.Android,
            BuildOptions.CompressWithLz4HC
        );

        if (vReport.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("Build échoué");
            EditorApplication.Exit(1);
        }

        Debug.Log("Build Android réussi");
        EditorApplication.Exit(0);
    }

    private static bool HasCommandLineArg(string pArg)
    {
        string[] vArgs = System.Environment.GetCommandLineArgs();
        foreach (string lArg in vArgs)
        {
            if (lArg.Equals(pArg, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
