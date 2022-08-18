using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using WXDM;
using WeChatWASM;

namespace Nova
{
    public class CheckpointSerializer
    {
        private const int Version = 2;

        private static readonly string FileHeader = "NOVASAVE";

        public T SafeRead<T>(string path)
        {
            try
            {
                try
                {
                    var result = Read<T>(path);
                    return result;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Nova: {path} is corrupted.\n{e.Message}\nTry to recover...");
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Nova: Error loading {path}.\n{e.Message}");
                throw; // Nested exception cannot display full message here
            }
        }

        private bool alertOnSafeWriteFail = true;

        public void SafeWrite<T>(T obj, string path)
        {
//#if UNITY_EDITOR
            Debug.Log($"SafeWrite {obj:GetType()} {path}");
//#endif

            try
            {
                Write(obj, path); // May be interrupted
            }
            catch (Exception e)
            {
                // If there is some problem with Alert.Show, we need to avoid infinite recursion
                if (alertOnSafeWriteFail)
                {
                    alertOnSafeWriteFail = false;
                    Alert.Show(I18n.__("bookmark.save.fail"), e.Message);
                }

                throw;
            }
        }

        private T Read<T>(string s)
        {
#if true
            var fileHeader = PlayerPrefs.GetString("FileHeader", "");
            var version = PlayerPrefs.GetInt("Version", -1);
            var serialized_data = PlayerPrefs.GetString(s, "");
#else
            Debug.Log($"Current WX File path: {WX.env.USER_DATA_PATH}");
            var fileHeader = WXDataManager.getString("FileHeader");
            var version = WXDataManager.getInt("Version");
            var serialized_data = WXDataManager.getString(s);
#endif

            Utils.RuntimeAssert(Version >= version, "Save file is incompatible with the current version of Nova.");
            Utils.RuntimeAssert(fileHeader == FileHeader, "Save file is not formatted.");


            //Debug.Log($"Reading from {s}, {serialized_data}");
            // Start Parsing

            Utils.RuntimeAssert(serialized_data != "", "Save file is empty.");

            BinaryFormatter formatter = new BinaryFormatter();
            byte[] bta = Convert.FromBase64String(serialized_data);
            MemoryStream ms = new MemoryStream(bta);
            using (var compressed = new DeflateStream(ms, CompressionMode.Decompress))
            using (var uncompressed = new MemoryStream())
            {
                compressed.CopyTo(uncompressed);
                uncompressed.Position = 0;
                return (T)formatter.Deserialize(uncompressed);
            }
        }
        private void Write<T>(T obj, string s)
        {
#if true

            // Write obj to the s field of playerpref
            PlayerPrefs.SetString("FileHeader", FileHeader);
            PlayerPrefs.SetInt("Version", Version);
#else
            WXDataManager.setString("FileHeader", FileHeader);
            WXDataManager.setInt("Version", Version);
#endif

            BinaryFormatter formatter = new BinaryFormatter();

            var compressStream = new MemoryStream();
            using (var compressed = new DeflateStream(compressStream, CompressionMode.Compress))
            using (var uncompressed = new MemoryStream())
            {
                formatter.Serialize(uncompressed, obj);
                uncompressed.Position = 0;
                uncompressed.CopyTo(compressed);
            }
            var converted = Convert.ToBase64String(compressStream.ToArray());
            //Debug.Log($"Writing to {s}, {converted}");
#if true
            PlayerPrefs.SetString(s, converted);
#else
            WXDataManager.setString(s, converted);
#endif
        }
    }

}
