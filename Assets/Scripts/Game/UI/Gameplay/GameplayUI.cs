using System;
using System.Collections.Generic;
using Game.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI.Gameplay
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameManager gameManager;

        private int _levelIndex;

        private void OnEnable()
        {
            gameManager.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            gameManager.OnGameOver -= OnGameOver;
        }

        public void BackToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void OnGameOver(bool isWin)
        {
            ModalWindow.ModalWindowOptions modalWindowOptions;
            if (isWin)
            {
                List<ModalWindow.ModalWindowBtn> choices = new()
                {
                    new ModalWindow.ModalWindowBtn("Выйти", BackToMainMenu),
                    new ModalWindow.ModalWindowBtn("Продолжить", NextLevel),
                };
                modalWindowOptions = new()
                {
                    canvas = canvas,
                    message = $"Победа!\nВаш счет:{gameManager.CurrentScore}",
                    buttons = choices,
                };
            }
            else
            {
                List<ModalWindow.ModalWindowBtn> choices = new()
                {
                    new ModalWindow.ModalWindowBtn("Выйти", BackToMainMenu),
                    new ModalWindow.ModalWindowBtn("Еще раз", Restartlevel),
                };
                modalWindowOptions = new()
                {
                    canvas = canvas,
                    message = $"Ты проиграл\nВаш счет:{gameManager.CurrentScore}",
                    buttons = choices,
                };
            }
            
            ModalWindow.CreateWindow(modalWindowOptions);
        }

        private void Restartlevel()
        {
            gameManager.LoadLevel(_levelIndex);
        }

        private void NextLevel()
        {
            _levelIndex++;
            var levels = LevelLoader.GetAllFileLevels();
            if (_levelIndex >= levels.Length) _levelIndex = 0;
            var level = LevelLoader.FileToLevel(levels[_levelIndex]);
            gameManager.LoadLevel(level);
        }
    }
}

