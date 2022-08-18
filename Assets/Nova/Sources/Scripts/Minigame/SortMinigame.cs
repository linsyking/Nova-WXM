using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Nova
{
    public class SortMinigame : MonoBehaviour
    {
        // Start is called before the first frame update

        public VerticalLayoutGroup content;
        public GameObject button;
        public Text instructionText;
        public Button confirmButton;
        private Variables variables;
        private GameState gameState;

        private void Start()
        {
            var controller = Utils.FindNovaGameController();
            gameState = controller.GameState;
            variables = gameState.variables;

            instructionText.text = variables.Get<string>("v_sortminigame_text");
            var testOptions = variables.Get<string>("v_sortminigame_options");
            var history = variables.Get<string>("v_sortminigame_history_options");
            var list = new List<string>();
            if (history != null)
            {
                list = history.Split(';').ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    Instantiate(button, content.transform).transform.GetChild(0).GetComponent<Text>().text = list[i];
                }
            }
            else
            {
                list = testOptions.Split(';').ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    Instantiate(button, content.transform).transform.GetChild(0).GetComponent<Text>().text = $"{i + 1}. {list[i]}";
                }
            }

            Destroy(button);
            confirmButton.onClick.AddListener(confirmListener);
        }


        private void OnDestroy()
        {
            confirmButton.onClick.RemoveAllListeners();
        }

        private void confirmListener()
        {
            // Get all texts
            List<string> ans_list = new List<string>();
            List<string> new_list = new List<string>();
            foreach (Transform child in content.transform)
            {
                var t_text = child.GetChild(0).GetComponent<Text>().text;
                var dotPosition = t_text.IndexOf('.');
                var to_add = t_text.Substring(0, dotPosition);
                //var new_text = t_text.Substring(dotPosition + 2, t_text.Length - dotPosition - 2);
                ans_list.Add(to_add);
                new_list.Add(t_text);
            }
            var ans = string.Join(";", ans_list);
            var new_t = string.Join(";", new_list);
            variables.Set("v_sortminigame_history_options", new_t);
            variables.Set("v_sortminigame_result", ans);
            gameState.SignalFence(true);
        }
    }

}
