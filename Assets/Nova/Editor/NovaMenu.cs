using System.IO;
using UnityEditor;
using UnityEngine;
using WeChatWASM;
using WXDM;

namespace Nova.Editor
{
    public static class NovaMenu
    {
        [MenuItem("Nova/Clear Save Data", false, 1)]
        public static void ClearSaveData()
        {
            var saveDir = new DirectoryInfo(Application.persistentDataPath + "/Save/");
            foreach (var file in saveDir.GetFiles())
            {
                file.Delete();
            }
        }

        [MenuItem("Nova/Clear Config Data", false, 1)]
        public static void ClearConfigData()
        {
#if true
            PlayerPrefs.DeleteAll();
#else
            WXDataManager.delAllKey();
#endif
        }

        [MenuItem("Nova/Reset Input Mapping", false, 1)]
        public static void ResetInputMapping()
        {
            if (Directory.Exists(InputMapper.InputFilesDirectory))
            {
                Directory.Delete(InputMapper.InputFilesDirectory, true);
            }
        }
    }
}
