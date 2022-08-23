using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UnityEngine.Networking;
using System.Collections;
using System.IO;


namespace Nova
{

    [ExportCustomType]
    public enum AssetCacheType
    {
        Image,
        StandingLayer,
        Prefab,
        Audio
    }

    public enum ABLoadStatus
    {
        LoadedSuccessfully,
        LoadFailed,
        Loading
    }

    /// <summary>
    /// Load assets at runtime and manage preloaded assets
    /// </summary>
    /// <remarks>
    /// All assets should be stored in a Resources folder, or a subfolder in it
    /// </remarks>
    [ExportCustomType]
    public class AssetLoader : MonoBehaviour, IPrioritizedRestorable
    {
        private static AssetLoader Current;

        private static readonly HashSet<string> LocalizedResourcePaths = new HashSet<string>();

        // The reference of the asset is stored in request
        private class CachedAssetEntry
        {
            public int count;
            public ResourceRequest request;
        }

        private Dictionary<AssetCacheType, LRUCache<string, CachedAssetEntry>> cachedAssets;
        private GameState gameState;

        //private AssetBundle ab;
        // All the ABs
        private readonly Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

        private ABLoadStatus abStatus;

        private readonly Dictionary<string, UnityObject> allLoadedTextures = new Dictionary<string, UnityObject>();
        private readonly Dictionary<string, bool> isLocked = new Dictionary<string, bool>();

        //private const int MaxHoldNumber = 50;

        private void Awake()
        {
            abStatus = ABLoadStatus.Loading;
            Current = this;

            var text = Resources.Load<TextAsset>("LocalizedResourcePaths");
            if (text)
            {
                LocalizedResourcePaths.UnionWith(text.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            }

            cachedAssets = new Dictionary<AssetCacheType, LRUCache<string, CachedAssetEntry>>
            {
                [AssetCacheType.Image] = new LRUCache<string, CachedAssetEntry>(20, true),
                [AssetCacheType.StandingLayer] = new LRUCache<string, CachedAssetEntry>(20, true),
                [AssetCacheType.Prefab] = new LRUCache<string, CachedAssetEntry>(1, true),
                [AssetCacheType.Audio] = new LRUCache<string, CachedAssetEntry>(4, true)
            };

            gameState = Utils.FindNovaGameController().GameState;
            gameState.AddRestorable(this);

            Application.lowMemory += UnloadUnusedAndCachedAssets;
            I18n.LocaleChanged.AddListener(OnLocaleChanged);

            // Download all the ABs
            downloadABs();
            LuaRuntime.Instance.BindObject("assetLoader", this);
        }

        #region List Operation

        private void l_add(string name, UnityObject o)
        {
            isLocked[name] = false;
            allLoadedTextures[name] = o;
        }

        private UnityObject l_get(string name)
        {
            if (allLoadedTextures.ContainsKey(name))
            {
                return allLoadedTextures[name];
            }
            return null;
        }

        private void l_remove(string name)
        {
            isLocked.Remove(name);

            foreach (var e in allLoadedTextures)
            {
                if (e.Key == name)
                {
                    Debug.Log($"Removing {name}");
                    Resources.UnloadAsset(allLoadedTextures[name]);
                    Resources.UnloadUnusedAssets();
                    return;
                }
            }
        }

        #endregion

        #region AB Management

        private List<string> getAllABName()
        {
            // The names of all the ABs
            return (new List<string>
            {
                "wuhui",
                "data"
            });
        }

        public bool isABLoaded()
        {
            return abStatus != ABLoadStatus.Loading;
        }

        public void downloadABs()
        {
            StartCoroutine(c_DownloadAllAB());
        }

        private T loadABSprite<T>(string abName, string rname) where T : UnityObject
        {
            if (allLoadedTextures.ContainsKey($"{abName}/{rname}") && allLoadedTextures[$"{abName}/{rname}"] != null)
            {
                // Has cached data, use it!
                return (T)allLoadedTextures[$"{abName}/{rname}"];
            }
            var ab = assetBundles[abName];
            var lld = ab.LoadAsset<T>(rname);

            if (rname.Contains(".__snapshot"))
            {
                Debug.Log("Snapshot file found, skip caching");
                return lld;
            }
            var fname = $"{abName}/{rname}";
            l_add(fname, lld);
            return lld;
        }

        public T getABSprite<T>(string path) where T : UnityObject
        {
            Resources.UnloadUnusedAssets();


            //var lockednum = getLockedNumber();
            //Debug.Log($"Getting sprite {rname}");

            //if (cachedTextures.Count >= MaxHoldNumber + lockednum)
            //{
            //    // Remove First
            //    foreach (var ele in cachedTextures)
            //    {
            //        if (!isLocked[ele])
            //        {
            //            // Delete it!
            //            l_remove(ele);
            //            break;
            //        }
            //    }
            //}

            if (abStatus == ABLoadStatus.LoadedSuccessfully)
            {
                //Debug.Log($"Origin: {path}");


                var index = path.IndexOf('/', 0);
                var rname = Path.GetFileNameWithoutExtension(path);

                if (index == -1)
                {
                    // Search Mode
                    var abNames = getAllABName();
                    foreach (var abName in abNames)
                    {
                        var ab = assetBundles[abName];
                        //Debug.Log($"Finding: {abName}/{rname}");
                        if (ab.Contains(rname))
                        {
                            // Found
                            Debug.Log($"Found Asset: {abName}/{rname}");
                            return loadABSprite<T>(abName, rname);
                        }
                    }
                }
                else
                {
                    // Normal Mode
                    var dname = path.Substring(0, index);
                    Debug.Log($"Loading AB {dname}/{rname}");
                    return loadABSprite<T>(dname, rname);
                }


            }
            return null;
        }

        private IEnumerator c_DownloadAllAB()
        {
            var names = getAllABName();
            foreach (var name in names)
            {
                yield return StartCoroutine(c_DownloadAB(name));
            }
            if (abStatus == ABLoadStatus.Loading)
            {
                Debug.Log("All ABs loaded!");
                abStatus = ABLoadStatus.LoadedSuccessfully;
                preloadAssets();
            }
        }

        private IEnumerator c_DownloadAB(string name)
        {
#if UNITY_EDITOR
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(Path.Combine(Application.streamingAssetsPath, $"{name}.bundle"));
#else
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle($"https://res.yydbxx.cn/res/player/StreamingAssets/{name}.bundle");
#endif       
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("ERROR/" + request.error);
                abStatus = ABLoadStatus.LoadFailed;

            }
            else
            {
                var ab = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                assetBundles.Add(name, ab);
                Debug.Log($"Load AB {name} successfully!");
            }
            request.Dispose();
        }

        #endregion

        #region Lua exposed

        private void preloadAssets()
        {
            // Preload some assets at the beginning
            var spriteList = new List<string>
            {
                //"data/test1",
                "wuhui/entry"
            };
            foreach (var sprite in spriteList)
            {
                preloadAsset(sprite);
            }
        }

        public void preloadAsset(string name)
        {
            getABSprite<Sprite>(name);
            l_remove(name);
            Resources.UnloadUnusedAssets();
        }

        public void m_release_all()
        {
            // Unhold all textures
            foreach (var txt in allLoadedTextures)
            {
                Debug.Log($"Releasing {txt.Key}");
                Resources.UnloadAsset(allLoadedTextures[txt.Key]);
            }
            Resources.UnloadUnusedAssets();
            isLocked.Clear();
        }

        public void m_release_all_without_locked()
        {
            // Unhold all textures
            foreach (var txt in allLoadedTextures)
            {

                if (isLocked.ContainsKey(txt.Key) && isLocked[txt.Key])
                {
                    // Locked
                    continue;
                }
                Debug.Log($"Releasing {txt.Key}");
                Resources.UnloadAsset(allLoadedTextures[txt.Key]);
            }
            Resources.UnloadUnusedAssets();
        }

        public void m_hold(string name)
        {
            // Hold
            isLocked[name] = true;
        }

        public void m_unhold(string name)
        {
            // unHold
            isLocked[name] = false;
        }

        public void m_release(string name)
        {
            l_remove(name);
            Resources.UnloadUnusedAssets();
        }

        #endregion

        //private int getLockedNumber()
        //{
        //    int total = 0;
        //    foreach(var status in isLocked)
        //    {
        //        if(status.Value == true)
        //        {
        //            total++;
        //        }
        //    }
        //    return total;
        //}

        private void OnDestroy()
        {
            Current = null;
            gameState.RemoveRestorable(this);

            // Unload all ABs
            foreach (var ab in assetBundles)
            {
                ab.Value.Unload(true);
            }
            Application.lowMemory -= UnloadUnusedAndCachedAssets;
            I18n.LocaleChanged.RemoveListener(OnLocaleChanged);
        }

        #region Methods independent of current and cache

        private static string TryGetLocalizedPath(string path)
        {
            if (I18n.CurrentLocale == I18n.DefaultLocale)
            {
                return path;
            }

            var localizedPath = I18n.LocalizedResourcesPath + I18n.CurrentLocale + "/" + path;

            if (!LocalizedResourcePaths.Contains(localizedPath))
            {
                return path;
            }

            return localizedPath;
        }

        public static T LoadOrNull<T>(string path) where T : UnityObject
        {
            path = Utils.ConvertPathSeparator(path);
            path = TryGetLocalizedPath(path);
            return Resources.Load<T>(path);
        }

        // Load with null check
        public static T Load<T>(string path) where T : UnityObject
        {
            UnloadUnusedAndCachedAssets();
            //Debug.Log($"type: {typeof(T)}");
            if (typeof(T) == typeof(Sprite) || typeof(T) == typeof(Texture))
            {
                // Image Resources
                return Utils.FindNovaGameController().AssetLoader.getABSprite<T>(path);

            }
            T ret = LoadOrNull<T>(path);
            if (ret == null)
            {
                Debug.LogError($"Nova: {typeof(T)} {path} not found.");
            }

            return ret;
        }

        // Specified type used for Lua binding
        public static Texture LoadTexture(string path)
        {
            return Load<Texture>(path);
        }

        public static void UnloadUnusedAssets()
        {
            Debug.Log("unloading");
            Resources.UnloadUnusedAssets();
        }

        #endregion

        #region Methods using cache

        public static void UnloadUnusedAndCachedAssets()
        {
            foreach (var cache in Current.cachedAssets.Values)
            {
                cache.Clear();
            }

            Resources.UnloadUnusedAssets();
        }

        public static void Preload(AssetCacheType type, string path)
        {
            if (type == AssetCacheType.Image)
            {
                // Preload Images
                //Utils.FindNovaGameController().AssetLoader.preloadAsset(path);
                return;
            }
            path = Utils.ConvertPathSeparator(path);

            Debug.Log($"Preload {type}:{path}");
            var cache = Current.cachedAssets[type];
            if (cache.ContainsKey(path))
            {
                cache[path].count++;
            }
            else
            {
                // Debug.Log($"Add cache {type}:{path}");
                var localizedPath = TryGetLocalizedPath(path);
                cache[path] = new CachedAssetEntry { count = 1, request = Resources.LoadAsync(localizedPath) };
            }
        }

        public static void Unpreload(AssetCacheType type, string path)
        {
            if (type == AssetCacheType.Image)
            {
                // Preload Images
                //Utils.FindNovaGameController().AssetLoader.m_release(path);
                return;
            }
            path = Utils.ConvertPathSeparator(path);

            // Debug.Log($"Unpreload {type}:{path}");
            var cache = Current.cachedAssets[type];
            if (cache.ContainsKey(path))
            {
                var entry = cache.GetNoTouch(path);
                entry.count--;
                if (entry.count <= 0)
                {
                    // Debug.Log($"Remove cache {type}:{path}");
                    cache.Remove(path);
                }
            }
            else
            {
                //Debug.LogWarning($"Nova: Asset {type}:{path} not cached when unpreloading.");
            }
        }

        // Refresh cache on locale changed
        private void OnLocaleChanged()
        {
            var cache = cachedAssets[AssetCacheType.Image];
            foreach (var path in cache.Keys)
            {
                var localizedPath = I18n.LocalizedResourcesPath + I18n.CurrentLocale + "/" + path;
                if (LocalizedResourcePaths.Contains(localizedPath))
                {
                    cache[path].request = Resources.LoadAsync(localizedPath);
                }
            }
        }

        #endregion

        #region Restoration

        public string restorableName => "AssetLoader";
        public RestorablePriority priority => RestorablePriority.Preload;

        [Serializable]
        private class AssetLoaderRestoreData : IRestoreData
        {
            public readonly Dictionary<AssetCacheType, Dictionary<string, int>> cachedAssetCounts;

            public AssetLoaderRestoreData(Dictionary<AssetCacheType, LRUCache<string, CachedAssetEntry>> cachedAssets)
            {
                cachedAssetCounts = cachedAssets.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToDictionary(
                        pair2 => pair2.Key,
                        pair2 => pair2.Value.count
                    )
                );
            }
        }

        public IRestoreData GetRestoreData()
        {
            return new AssetLoaderRestoreData(cachedAssets);
        }

        public void Restore(IRestoreData restoreData)
        {
            var data = restoreData as AssetLoaderRestoreData;
            // Serialized assets are in MRU order
            foreach (var pair in data.cachedAssetCounts)
            {
                var type = pair.Key;
                var cache = cachedAssets[type];
                cache.Clear();
                foreach (var pathAndCount in pair.Value)
                {
                    var path = pathAndCount.Key;
                    var count = pathAndCount.Value;
                    var localizedPath = TryGetLocalizedPath(path);
                    var request = Resources.LoadAsync(localizedPath);
                    cache[path] = new CachedAssetEntry { count = count, request = request };
                }
            }
        }

        #endregion

        public static void DebugPrint()
        {
            foreach (var pair in Current.cachedAssets)
            {
                var type = pair.Key;
                var cache = pair.Value;
                Debug.Log($"{type} {cache.Count} {cache.HistoryMaxCount}");
            }
        }
    }
}
