using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EncryptedAsset.Runtime;
using RemoteBuild.Client;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

public class GameBundleEditor : Editor, IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private const int M_REMOTE_BUILD_PORT = 9933;
    private static string M_BUNDLE_BUILDER = "Assets/Game/EncryptedAsset/Data/BuildScriptEncryptedAsset.asset";
    private static string M_BUNDLE_SETTINGS = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
    private static string M_GROUP_ISOLATION = "Assets/AddressableAssetsData/AssetGroups/Duplicate Asset Isolation.asset";

    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        Application.logMessageReceived -= OnBuildError;
        Application.logMessageReceived += OnBuildError;
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        Application.logMessageReceived -= OnBuildError;
    }

    void OnBuildError(string condition, string stacktrace, LogType type)
    {
        if (type == LogType.Error)
        {
            Application.logMessageReceived -= OnBuildError;
            BuildComplete("", stacktrace);
        }
    }

    [MenuItem("Mgif/Build Android APK")]
    static void BuildAndroidAPK()
    {
        string[] args = Environment.GetCommandLineArgs();
        string[] parameters = args.Where(arg => arg.StartsWith("id_")).ToArray();
        //
        EditorUserBuildSettings.buildAppBundle = false;
        Setting(BuildTarget.Android); 
        // 
        BuildGame((parameters == null || parameters.Length < 1) ? -1 : int.Parse(parameters[0].Substring(3)));
    }

    [MenuItem("Mgif/Build Android AAB")]
    static void BuildAndroidAab()
    {
        string[] args = Environment.GetCommandLineArgs();
        string[] parameters = args.Where(arg => arg.StartsWith("id_")).ToArray();
        //
        EditorUserBuildSettings.buildAppBundle = true;
        Setting(BuildTarget.Android);         
        // 
        BuildGame((parameters == null || parameters.Length < 1) ? -1 : int.Parse(parameters[0].Substring(3)));
    }

    static void BuildGame(int buidId)
    {
        //save build id
        PlayerPrefs.SetInt("build-id", buidId);
        PlayerPrefs.Save();
        //build bundle
        BuildBundle();
        //build player
        string path = Path.Combine(Application.dataPath, "../../Build");
        string name = PlayerSettings.productName + "-v." + GetExtension(EditorUserBuildSettings.activeBuildTarget);
        //
        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.locationPathName = Path.Combine(path, name);
        buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
 
        buildPlayerOptions.scenes = new string[]{EditorBuildSettings.scenes[0].path};
        buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;

        BuildPipeline.BuildPlayer(buildPlayerOptions);
        //UnityEditor.EditorUtility.RevealInFinder(buildPlayerOptions.locationPathName);
        //notify to server build
        BuildComplete(buildPlayerOptions.locationPathName, "");
    }


    [MenuItem("Mgif/Build Bundle")]
    static void BuildBundle()
    {
        //refresh bundle
        SettingBundle(true);
        //build
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
    }

    [MenuItem("Mgif/Refresh Bundle")]
    static void RefreshBundle()
    {
        SettingBundle(false);
    }

    static void SettingBundle(bool forceFix)
    {
        //load setting
        AddressableAssetSettings settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(M_BUNDLE_SETTINGS) as AddressableAssetSettings;
        //set builder
        IDataBuilder builder = AssetDatabase.LoadAssetAtPath<ScriptableObject>(M_BUNDLE_BUILDER) as IDataBuilder;
        int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);
        if (index > 0)
            settings.ActivePlayerDataBuilderIndex = index;
        //clear label
        List<string> labels = settings.GetLabels();
        foreach (string label in labels)
        {
            settings.RemoveLabel(label, true);
        }
        
        //remove Duplicate Asset Isolation group
        AddressableAssetGroup isolationGroup =
            AssetDatabase.LoadAssetAtPath<ScriptableObject>(M_GROUP_ISOLATION) as AddressableAssetGroup;
        if (isolationGroup != null)
            settings.RemoveGroup(isolationGroup);

        //EditorUtility.SetDirty(settings);
        AssetDatabase.Refresh();
        if(settings == null)
            settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(M_BUNDLE_SETTINGS) as AddressableAssetSettings;
        
        //set EncryptedBundleProvider
        foreach (AddressableAssetGroup group in settings.groups)
        {
            var groupSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (groupSchema != null)
            {
                groupSchema.UseAssetBundleCache = false;
                groupSchema.UseAssetBundleCrc = false;
                groupSchema.UseUnityWebRequestForLocalBundles = true;
                FieldInfo field = groupSchema.GetType().GetField("m_AssetBundleProviderType",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                SerializedType t = new SerializedType();
                t.Value = typeof(EncryptedBundleProvider);
                field.SetValue(groupSchema, t);
                //set label
                if (group.IsDefaultGroup())
                {
                    foreach (AddressableAssetEntry entry in group.entries)
                    {
                        string name = Path.GetFileNameWithoutExtension(entry.AssetPath);
                        settings.AddLabel(name);
                        entry.SetLabel(name, true);
                        entry.SetAddress(entry.AssetPath);
                    }
                }
            }
        }
        
        //EditorUtility.SetDirty(settings);
        AssetDatabase.Refresh();
        if(settings == null)
            settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(M_BUNDLE_SETTINGS) as AddressableAssetSettings;
        
        //fix bundle issue
        if (forceFix)
        {
            CheckBundleDupeDependencies rule = new CheckBundleDupeDependencies();
            rule.FixIssues(settings);
        }
        
        //refresh EncryptedBundleProvider
        foreach (AddressableAssetGroup group in settings.groups)
        {
            var groupSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (groupSchema != null)
            {
                groupSchema.UseAssetBundleCache = false;
                groupSchema.UseAssetBundleCrc = false;
                groupSchema.UseUnityWebRequestForLocalBundles = true;
                FieldInfo field = groupSchema.GetType().GetField("m_AssetBundleProviderType",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                SerializedType t = new SerializedType();
                t.Value = typeof(EncryptedBundleProvider);
                field.SetValue(groupSchema, t);
            }
        }

        //EditorUtility.SetDirty(settings);
        AssetDatabase.Refresh();
    }

    static void Setting(BuildTarget target)
    {
        BuildTargetGroup targetGroup = ConvertBuildTarget(target);
        EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.SetManagedStrippingLevel(targetGroup, ManagedStrippingLevel.Disabled);
        switch (target)
        {
            case BuildTarget.Android:
                if (EditorUserBuildSettings.buildAppBundle)
                    EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
                else
                {
                    EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Disabled;
                    PlayerSettings.Android.bundleVersionCode = PlayerPrefs.GetInt("bundle-vesion-code",
                        PlayerSettings.Android.bundleVersionCode) + 1;
                    PlayerPrefs.SetInt("bundle-vesion-code", PlayerSettings.Android.bundleVersionCode);
                    PlayerPrefs.Save();
                }

                //
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = Application.dataPath + "/../../Working/deathadventure.keystore";
                PlayerSettings.keystorePass = "1234567890";
                PlayerSettings.Android.keyaliasName = "deathadventure";
                PlayerSettings.Android.keyaliasPass = "1234567890";
                //
                EditorUserBuildSettings.il2CppCodeGeneration = Il2CppCodeGeneration.OptimizeSpeed;
                break;
            case BuildTarget.iOS:
                break;
        }
    }

    static void BuildComplete(string pathFile, string errorMessage)
    {
        int buildId = PlayerPrefs.GetInt("build-id");
        PlayerPrefs.DeleteKey("build-id");
        PlayerPrefs.Save();
        if(buildId < 0)
            return;
        BuildClient client = new BuildClient("127.0.0.1", M_REMOTE_BUILD_PORT);
        if(EditorUserBuildSettings.buildAppBundle)
        {
            string pathSymbols = $"{pathFile.Replace(".aab", "")}-{PlayerSettings.bundleVersion}-v{PlayerSettings.Android.bundleVersionCode}.symbols.zip";
            client.BuildAabComplete(buildId, pathFile, pathSymbols, errorMessage);
        }else
            client.BuildApkComplete(buildId, pathFile, errorMessage);
    }

    //////////////////////////////////////ULTILITIES/////////////////////////////////
    static string GetExtension(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.Android:
                return EditorUserBuildSettings.buildAppBundle ? $"{PlayerSettings.bundleVersion}.aab" : $"{PlayerSettings.Android.bundleVersionCode}.apk";
            default:
                break;
        }
 
        return ".unknown";
    }

    static BuildTargetGroup ConvertBuildTarget(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.iOS:
                return BuildTargetGroup.iOS;
            case BuildTarget.Android:
                return BuildTargetGroup.Android;
            default:
                return BuildTargetGroup.Standalone;
        }
    }
}
