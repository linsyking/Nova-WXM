using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Nova
{

    public class CheckMinigameController : MonoBehaviour
    {
        public VerticalLayoutGroup content;
        public GameObject toggle;
        public Text instructionText;
        public Button confirmButton;
        private Variables variables;
        private GameState gameState;

        private void Start()
        {
            var controller = Utils.FindNovaGameController();
            gameState = controller.GameState;
            variables = gameState.variables;

            instructionText.text = variables.Get<string>("v_checkminigame_text", "default");
            var testOptions = variables.Get<string>("v_checkminigame_options", "test;test");
            var list = new List<string>();
            list = testOptions.Split(';').ToList();
            for (int i = 0; i < list.Count; i++)
            {
                Instantiate(toggle, content.transform).transform.GetChild(0).GetComponent<Text>().text = list[i];
            }

            Destroy(toggle);
            confirmButton.onClick.AddListener(confirmListener);
        }

        private void OnDestroy()
        {
            confirmButton.onClick.RemoveAllListeners();
        }

        private void confirmListener()
        {
            // Get all choices
            List<string> choose = new List<string>();
            var t_id = 1;
            foreach (Transform child in content.transform)
            {
                var isOn = child.GetComponent<Toggle>().isOn;
                if (isOn)
                {
                    choose.Add(t_id.ToString());
                }
                t_id++;
            }
            var ans = string.Join(";", choose);
            variables.Set("v_checkminigame_result", ans);
            gameState.SignalFence(true);
        }
    }

}
