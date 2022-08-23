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

1. 检测到您正在使用苹果系统，请升级微信至最新版，
否则可能出现音频播放异常等问题
2. 部分苹果设备可能会比较卡顿

运营方：上海交通大学密西根学院学生会
技术负责人：段令博
联络负责人：段令博、彭靖嘉、宋万里
内容审核负责人：彭靖嘉
技术组：向一铭、何亦农、段令博、尹奕航、李云琪
文案与审核组：熊子扬、杨博然、许蓝
设计与审核组：王大墉、韩宇翔、蒋子宸、倪家棱、梁而道
文案提供方：学生会各下属职能部门";
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
