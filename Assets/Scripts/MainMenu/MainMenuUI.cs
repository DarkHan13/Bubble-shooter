using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    [RequireComponent(typeof(Canvas))]
    public class MainMenuUI : MonoBehaviour
    {
        private Canvas _canvas;

        private void OnEnable()
        {
            _canvas = GetComponent<Canvas>();
        }

        public void NewGameScene()
        {
            SceneManager.LoadScene("Gameplay");
        }

        public void GuideScene()
        {
            SceneManager.LoadScene("Guide");
        }

        public void ShowExitWindow()
        {
            List<ModalWindow.ModalWindowBtn> choices = new()
            {
                new ModalWindow.ModalWindowBtn("Остаться", null),
                new ModalWindow.ModalWindowBtn("Выйти", Quit),
            };
            ModalWindow.ModalWindowOptions modalWindowOptions = new ()
            {
                canvas = _canvas,
                message = "Вы действительно хотите выйти из игры?",
                buttons = choices,
            };
            ModalWindow.CreateWindow(modalWindowOptions);
        }
        public void Quit()
        {
            Application.Quit();
        }
    }
}