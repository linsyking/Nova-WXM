using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoSize : MonoBehaviour
{

    public RectTransform textRect;
    public int oneHeight;

    void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
        var old = GetComponent<RectTransform>().sizeDelta;
        GetComponent<RectTransform>().sizeDelta = new Vector2(old.x,old.y + textRect.sizeDelta.y + oneHeight);
    }

}
