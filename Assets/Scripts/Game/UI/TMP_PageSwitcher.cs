using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMP_PageSwitcher : MonoBehaviour
{
    public TextMeshProUGUI tmp;

    public int CurrentPage { get; private set; }
    [field: SerializeField] public int TotalPages { get; private set; }

    public event Action<int> OnPageChanged; 

    void OnEnable()
    {
        // Инициализация количества страниц
        TotalPages = tmp.textInfo.pageInfo.Length;
    }

    public void SetPage(int newPageNumber)
    {
        CurrentPage = Math.Clamp(newPageNumber, 1, TotalPages);
        tmp.pageToDisplay = CurrentPage;
        OnPageChanged?.Invoke(CurrentPage);
    }
    
    public void GoToNextPage()
    {
        SetPage(CurrentPage + 1);
    }

    public void GoToPreviousPage()
    {
        SetPage(CurrentPage - 1);
    }
}
