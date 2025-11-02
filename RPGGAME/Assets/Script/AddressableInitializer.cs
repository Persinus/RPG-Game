#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class AddressableAutoCopy
{
    private const string SourceFolder = "ServerData/Android";
    private const string TargetFolder = "Assets/StreamingAssets/aa/Android";

    [MenuItem("Addressables/📦 Copy ServerData → StreamingAssets")]
    public static void CopyAddressablesToStreamingAssets()
    {
        if (!Directory.Exists(SourceFolder))
        {
            Debug.LogError($"❌ Source folder not found: {SourceFolder}");
            return;
        }

        if (!Directory.Exists(TargetFolder))
        {
            Directory.CreateDirectory(TargetFolder);
        }

        foreach (var file in Directory.GetFiles(SourceFolder))
        {
            var fileName = Path.GetFileName(file);
            var dest = Path.Combine(TargetFolder, fileName);
            File.Copy(file, dest, true);
        }

        AssetDatabase.Refresh();
        Debug.Log($"✅ Copied all files from '{SourceFolder}' → '{TargetFolder}'");
    }

    // 🔹 Auto-run right after Addressables build
    [UnityEditor.Callbacks.PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        CopyAddressablesToStreamingAssets();
    }
}
#endif
