using System;
using System.Collections.Generic;
using Game.Level;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI.Gameplay
{
    public class GuideUI : MonoBehaviour
    {
        [SerializeField] private TMP_PageSwitcher psInfo;
        [SerializeField] private Button nextBtn, prevBtn;
        [SerializeField] private TextMeshProUGUI tmpPageNumber;
        
        private void OnEnable()
        {
            psInfo.OnPageChanged += PsInfoOnOnPageChanged;
            nextBtn.onClick.AddListener(delegate
            {
                psInfo.GoToNextPage();
            });
            
            prevBtn.onClick.AddListener(delegate
            {
                psInfo.GoToPreviousPage();
            });
        }

        private void Start()
        {
            psInfo.SetPage(1);
        }

        private void OnDisable()
        {
            psInfo.OnPageChanged -= PsInfoOnOnPageChanged;
        }

        private void PsInfoOnOnPageChanged(int newPage)
        {
            prevBtn.interactable = newPage > 1;
            nextBtn.interactable = newPage < psInfo.TotalPages;
            tmpPageNumber.text = $"{newPage}/{psInfo.TotalPages}";
        }

        public void BackToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}

