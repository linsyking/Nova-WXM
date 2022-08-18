using UnityEditor;
using UnityEngine;
using System.IO;
public class ExportAssets : MonoBehaviour
{

    [@MenuItem("Test/Build Asset Bundles")]
    static void BuildAssetBundles()
    {
        string dst = Application.streamingAssetsPath + "/AssetBundles";
        if (!Directory.Exists(dst))
        {
            Directory.CreateDirectory(dst);
        }
        BuildPipeline.BuildAssetBundles(dst, BuildAssetBundleOptions.AppendHashToAssetBundleName | BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.None, BuildTarget.WebGL);

    }
}
