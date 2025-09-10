using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager> //½Ì±ÛÅæ ÆÐÅÏ ±¸Çö
{
    private Canvas _canvas;
    private void Awake()
    {
        base.Awake();
        _canvas = FindAnyObjectByType<Canvas>();
    }

    public void ShowUI(string uiName)
    {
        _canvas.transform.Find(uiName).gameObject.SetActive(true);
    }

    public void HideUI(string uiName)
    {
        _canvas.transform.Find(uiName).gameObject.SetActive(false);
    }

    public bool UpdateText(string uiName, string newText)
    {
        GameObject textUI = _canvas.transform.Find(uiName).gameObject;
        if (textUI.activeSelf == false) { textUI.SetActive(true); }

        TextMeshProUGUI TMP;
         if(textUI.TryGetComponent(out TMP))
        {
            TMP.text = newText;
            return true;
        }
        else
        {
            return false;
        }
    }
}
