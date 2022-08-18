using WeChatWASM;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace WXDM
{
    public class WXDataManager
    {
        private static string getLocalPath()
        {
            var fpath = WX.env.USER_DATA_PATH;
            return $"{fpath}/data.json";
        }

        private static void updateKeyValue(string key, string value)
        {
            var fs = WX.GetFileSystemManager();
            if (fs.AccessSync(getLocalPath()) != "access:ok")
            {
                // Initialize
                Debug.Log($"WXDM:First Time Save! Initializing... Storing {key}:{value}");
                var initData = new Dictionary<string, string>();
                initData.Add(key, value);
                var serialized_I = JsonConvert.SerializeObject(initData);
                var res = fs.WriteFileSync(getLocalPath(), serialized_I);
                Debug.Log($"Writing to {getLocalPath()}, the result is {res}");
                return;
            }
            var fileContents = fs.ReadFileSync(getLocalPath(), "utf8");
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);
            values[key] = value;
            var serialized = JsonConvert.SerializeObject(values);
            fs.WriteFileSync(getLocalPath(), serialized);
        }

        public static bool hasKey(string key)
        {
            var fs = WX.GetFileSystemManager();
            if (fs.AccessSync(getLocalPath()) != "access:ok")
            {
                // Failed
                Debug.Log($"WXDM:Failed to load data when has key {key}");
                return false;
            }
            var fileContents = fs.ReadFileSync(getLocalPath(), "utf8");
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);
            return values.ContainsKey(key);

            //return false;
        }

        public static void delKey(string key)
        {
            var fs = WX.GetFileSystemManager();
            if (fs.AccessSync(getLocalPath()) != "access:ok")
            {
                // Failed
                Debug.Log($"WXDM:Failed to load data when del key {key}");
                return;
            }
            var fileContents = fs.ReadFileSync(getLocalPath(), "utf8");
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);
            values.Remove(key);
            var serialized = JsonConvert.SerializeObject(values);
            fs.WriteFileSync(getLocalPath(), serialized);
        }

        private static string getKey(string key)
        {
            var fs = WX.GetFileSystemManager();
            Debug.Log($"WXDM: get {key}, status: {fs.AccessSync(getLocalPath())}");
            if (fs.AccessSync(getLocalPath()) != "access:ok")
            {
                // Failed
                Debug.Log($"WXDM:Failed to load data when get key {key}");
                return "";
            }
            var fileContents = fs.ReadFileSync(getLocalPath(), "utf8");

            Debug.Log($"Read data {fileContents}");

            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);
            if (values.ContainsKey(key))
            {
                return values[key];
            }
            else
            {
                return "";
            }
            //return "";
        }

        public static void setString(string key, string value)
        {
            updateKeyValue(key, value);
        }

        public static void setInt(string key, int value)
        {
            updateKeyValue(key, value.ToString());
        }

        public static int getInt(string key)
        {
            return int.Parse(getKey(key));
        }

        public static string getString(string key)
        {
            return getKey(key);
        }

        public static void delAllKey()
        {
            var fs = WX.GetFileSystemManager();
            fs.WriteFileSync(getLocalPath(), "");
        }
    }
}
