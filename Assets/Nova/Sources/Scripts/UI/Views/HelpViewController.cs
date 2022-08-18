using UnityEngine.UI;
using WeChatWASM;

namespace Nova
{
    public class HelpViewController : ViewControllerBase
    {
        public Button returnButton;
        public Button returnButton2;
        public Text helpText;

        private const string GameFirstShownKey = ConfigManager.FirstShownKeyPrefix + "Game";

        private ConfigManager configManager;

        protected override void Awake()
        {
            base.Awake();

            returnButton.onClick.AddListener(Hide);
            returnButton2.onClick.AddListener(Hide);

            configManager = Utils.FindNovaGameController().ConfigManager;
        }

        protected override void Start()
        {
            base.Start();

#if !UNITY_EDITOR

            if (WX.GetSystemInfoSync().platform == "ios")
            {
                helpText.text = @"<b>小游戏说明</b>

1. 检测到您正在使用苹果系统，请升级微信至最新版，否则可能出现音频播放异常等问题
2. 苹果设备暂不支持存储屏幕截图作为存档缩略图
3. 苹果设备易出异常（如闪退），建议使用安卓系统玩此游戏
4. 祝您游玩愉快";
            }
#endif

            if (configManager.GetInt(GameFirstShownKey) == 0)
            {
                configManager.SetInt(GameFirstShownKey, 1);
                Show();
            }
        }
    }
}
