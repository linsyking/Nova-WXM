using UnityEngine;
using UnityEngine.UI;
using WeChatWASM;
using UnityEngine.EventSystems;

namespace Nova
{
    public class InputMinigameController : MonoBehaviour
    {
        public Text text;
        public InputField inputField;
        public Button closeButton;

        private GameState gameState;
        private Variables variables;


#if !UNITY_EDITOR
        private void CloseKeyBoard(OnKeyboardInputCallbackResult x)
        {
            // Input String
            inputField.text = x.value;
        }
        private void CloseKeyBoardNull(OnKeyboardInputCallbackResult x)
        {
            Debug.Log("Close Keyboard");
        }
#endif

        private void Awake()
        {
            Debug.Log("Satrted running minigame");
#if !UNITY_EDITOR
            WX.OnKeyboardConfirm(CloseKeyBoard);
            WX.OnKeyboardComplete(CloseKeyBoard);
#endif

            var controller = Utils.FindNovaGameController();
            gameState = controller.GameState;
            variables = gameState.variables;
            //checkpointHelper = controller.CheckpointHelper;

            //var choice = variables.Get<string>("v_choice");
            //var count = checkpointHelper.GetGlobalVariable<int>("gv_minigame_count", 0);
            //count += 1;
            //checkpointHelper.SetGlobalVariable("gv_minigame_count", count);
            //text.text = string.Format(text.text, choice, count);
            text.text = variables.Get<string>("v_inputminigame_text");

            closeButton.onClick.AddListener(OnClick);
            //inputField.OnPointerClick(SelectInput);

            //var test_bool = variables.Get<bool>("v_test_bool");
            //var test_int = variables.Get<int>("v_test_int");
            //var test_float = variables.Get<float>("v_test_float");
            //var test_double = variables.Get<double>("v_test_double");
            //Debug.Log($"{test_bool} {test_int} {test_float} {test_double}");
            //variables.Set("v_test_bool", false);
            //variables.Set("v_test_int", 321);
            //variables.Set("v_test_float", 6.54f);
            //variables.Set("v_test_double", 9.87);
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            variables.Set("v_inputminigame_result", inputField.text);
            gameState.SignalFence(true);
#if !UNITY_EDITOR
            WX.OffKeyboardComplete(CloseKeyBoardNull);
            WX.OffKeyboardConfirm(CloseKeyBoardNull);
#endif
        }
    }
}
