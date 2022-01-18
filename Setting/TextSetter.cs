using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSetter : MonoBehaviour
{
    public Text text;
    void Start()
    {
        text.font = UISetting.Instance.defaultFont;
        text.fontSize = UISetting.Instance.defaultFontsize;
        text.color = UISetting.Instance.defaultFontColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
