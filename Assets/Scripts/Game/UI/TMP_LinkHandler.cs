using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMP_LinkHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI textMeshPro;

    private void Reset()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick (PointerEventData eventData) {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink (textMeshPro, eventData.position, eventData.pressEventCamera);
        if (linkIndex == -1) return;
        TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
        string selectedLink = linkInfo.GetLinkID();
        if (selectedLink != "") {
            Application.OpenURL (selectedLink);        
        }
    }
}
