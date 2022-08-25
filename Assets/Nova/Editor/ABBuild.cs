using UnityEditor;
using UnityEngine;
using System.IO;
public class ExportAssets : MonoBehaviour
{

    [@MenuItem("AB Manager/Build Asset Bundles")]
    static void BuildAssetBundles()
    {
        string dst = Application.streamingAssetsPath + "/AssetBundles";
        if (!Directory.Exists(dst))
        {
            Directory.CreateDirectory(dst);
        }
        //BuildPipeline.BuildAssetBundles(dst, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL,);
        //string[] k = AssetDatabase.GetAssetPathsFromAssetBundle("wuhui");
        
        //BuildPipeline.BuildAssetBundleExplicitAssetNames()

    }
}
