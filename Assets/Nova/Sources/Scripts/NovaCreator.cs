using UnityEngine;
using WeChatWASM;

namespace Nova
{
    /// <summary>
    /// Create and init NovaGameController prefab
    /// </summary>
    public class NovaCreator : MonoBehaviour
    {
        public GameObject novaGameControllerPrefab;

        private void Awake()
        {
            var controllerCount = GameObject.FindGameObjectsWithTag("NovaGameController").Length;
            if (controllerCount > 1)
            {
                Debug.LogWarning("Nova: Multiple NovaGameController found in the scene.");
            }

            if (controllerCount >= 1)
            {
                return;
            }

#if !UNITY_EDITOR
            WX.InitSDK((int code) =>
            {
                Debug.Log($"WX Init Success with exit code {code}");
                var fs = WX.env.USER_DATA_PATH;
                Debug.Log($"Current WXPATH: {fs}");

                var controller = Instantiate(novaGameControllerPrefab);
                controller.tag = "NovaGameController";
                DontDestroyOnLoad(controller);
            });
#else
            var controller = Instantiate(novaGameControllerPrefab);
            controller.tag = "NovaGameController";
            DontDestroyOnLoad(controller);
#endif
        }
    }
}
