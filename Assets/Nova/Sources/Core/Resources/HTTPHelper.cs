using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using WeChatWASM;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Nova
{

    public class VersionInfo
    {
        public string imgVersion;
        public string scriptVersion;
        public string[] imgPreloadList;
        public string[] bundleList;
        public string customInfo;           // Prompt
        public string imgBaseUrl;
        public string scriptBaseUrl;
    }

    public class HTTPHelper : MonoBehaviour
    {
        private bool status;
        private VersionInfo versionInfo;
        private List<string> scriptList;

        private static string sha1(string pwd)
        {
            SHA1 sha1 = SHA1.Create();
            byte[] originalPwd = Encoding.UTF8.GetBytes(pwd);
            //加密
            byte[] newPwd = sha1.ComputeHash(originalPwd);
            return string.Join("", newPwd.Select(o => string.Format("{0:x2}", o)).ToArray()).ToUpper();
        }


        #region Used For External

        public static HTTPHelper getInstance()
        {
            return Utils.FindNovaGameController().httpHelper;
        }
        public string getScriptJSON()
        {
            // Get script JSON Index File Content
#if UNITY_EDITOR
            return File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "script.json"), Encoding.UTF8);
#else
            return WX.GetFileSystemManager().ReadFileSync($"{WX.env.USER_DATA_PATH}/script.json", "utf-8");
#endif
        }

        public string getFileContent(string path)
        {
            var realid = sha1(path);
#if UNITY_EDITOR
            return File.ReadAllText(Path.Combine(Application.streamingAssetsPath, $"{realid}.txt"), Encoding.UTF8);
#else
            return WX.GetFileSystemManager().ReadFileSync($"{WX.env.USER_DATA_PATH}/{realid}.txt", "utf-8");
#endif
        }

        #endregion

        public void initChecking()
        {
            StartCoroutine(check());
        }

        public VersionInfo getCurrentVersionInfo()
        {
            return versionInfo;
        }

        private IEnumerator check()
        {
            // Check Update
            status = false;
            yield return getUpdateInfo();
            if (!status) yield break;
            status = false;
            // Check If need update
            if (PlayerPrefs.HasKey("versioninfo"))
            {
                var oldv = JsonConvert.DeserializeObject<VersionInfo>(PlayerPrefs.GetString("versioninfo"));
                if(oldv.imgVersion == versionInfo.imgVersion && oldv.scriptVersion == versionInfo.scriptVersion)
                {
                    // Same, no need to update
                    // Do something here
                    Debug.Log("No Updates Found.");
                    GameObject.FindGameObjectWithTag("UpdateInfo").SetActive(false);
                    Utils.FindNovaGameController().GameState.ReloadScripts();
                    Utils.FindNovaGameController().AssetLoader.downloadABs();
                    yield break;
                }
                if(oldv.scriptVersion == versionInfo.scriptVersion)
                {
                    // Only change imgVersion
                    saveChange();
                    Debug.Log("Only Update the imgs.");
                    downloadAB();
                    Utils.FindNovaGameController().GameState.ReloadScripts();
                    GameObject.FindGameObjectWithTag("UpdateInfo").SetActive(false);
                    Alert.Show("更新提示", $"提示：已更新图片至v{versionInfo.imgVersion}\n{versionInfo.customInfo}");
                    yield break;
                }
            }
            // Need to update the script

            Debug.Log("New Version Detected, Updating...");
            GameObject.FindGameObjectWithTag("UpdateInfo").GetComponent<UpdateInfo>().txt.text = "检测到更新，更新中...";
            yield return getScriptJ();
            if (!status) yield break;
            status = false;
            foreach(var spath in scriptList)
            {
                yield return getScriptSingle(spath);
            }
            if (!status) yield break;
            status = false;

            // Finally, save the changes
            saveChange();
            downloadAB();
            Debug.Log("Finishing Update");
            Utils.FindNovaGameController().GameState.ReloadScripts();

            GameObject.FindGameObjectWithTag("UpdateInfo").SetActive(false);
            Alert.Show("更新提示", $"提示：已更新剧本至v{versionInfo.scriptVersion},更新图片至v{versionInfo.imgVersion}\n{versionInfo.customInfo}");

        }

        private void saveChange()
        {
            string output = JsonConvert.SerializeObject(versionInfo);
            PlayerPrefs.SetString("versioninfo", output);
        }

        private IEnumerator getUpdateInfo()
        {
            UnityWebRequest webRequest = UnityWebRequest.Get("https://yydbxx.cn/test/st/nseditor/playerinfo/version.json");

            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                status = true;

                versionInfo = JsonConvert.DeserializeObject<VersionInfo>(webRequest.downloadHandler.text);

            }
        }

        private IEnumerator getScriptJ()
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(Path.Combine(versionInfo.scriptBaseUrl, "script.json"));

            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
#if UNITY_EDITOR
                File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "script.json"), webRequest.downloadHandler.text, Encoding.UTF8);
#else
                Debug.Log($"Getting script j, {Encoding.UTF8.GetBytes(webRequest.downloadHandler.text)}");
                WX.GetFileSystemManager().WriteFileSync($"{WX.env.USER_DATA_PATH}/script.json", Encoding.UTF8.GetBytes(webRequest.downloadHandler.text), "utf-8");
#endif
                status = true;
                var rawObjs = JArray.Parse(webRequest.downloadHandler.text);
                scriptList = rawObjs.ToObject<List<string>>();
                //Utils.FindNovaGameController().GameState.ReloadScripts();
            }
        }

        private IEnumerator getScriptSingle(string path)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(Path.Combine(versionInfo.scriptBaseUrl, path));

            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var rpath = sha1(path);
#if UNITY_EDITOR
                File.WriteAllText(Path.Combine(Application.streamingAssetsPath, $"{rpath}.txt"), webRequest.downloadHandler.text);
#else
                WX.GetFileSystemManager().WriteFileSync($"{WX.env.USER_DATA_PATH}/{rpath}.txt", Encoding.UTF8.GetBytes(webRequest.downloadHandler.text), "utf-8");
#endif
                status = true;
                //Utils.FindNovaGameController().GameState.ReloadScripts();
            }
        }

        private void downloadAB()
        {
            // Download New ABs
            Utils.FindNovaGameController().AssetLoader.downloadABs(new List<string>(versionInfo.bundleList));
        }
    }
}
