using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using WeChatWASM;

public class CallWXKeyboard : MonoBehaviour, IPointerClickHandler
{
    public InputField inputField;
    // Start is called before the first frame update
    public void OnPointerClick(PointerEventData eventData)
    {
#if !UNITY_EDITOR
        ShowKeyboardOption kbopt = new ShowKeyboardOption
        {
            confirmHold = true,
            confirmType = "done",
            defaultValue = "",
            maxLength = 100,
            multiple = false
        };
        WX.ShowKeyboard(kbopt);
#endif
        inputField.Select();
    }
}
