using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ModalWindow : MonoBehaviour
    {
        private static string _defaultWindowPathResources = "UI/DefaultModalWindow";
        private RectTransform _rectT;
        [SerializeField] private TextMeshProUGUI tmp;
        [SerializeField, Tooltip("Must be inactive")] private Button btnPrefab;
        [SerializeField] private HorizontalLayoutGroup buttonsGroup;

        public static ModalWindow CreateWindow(ModalWindowOptions args)
        {
            args.prefab ??= Resources.Load<ModalWindow>(_defaultWindowPathResources);
            if (args.prefab == null)
            {
                Debug.LogError($"No window was found in {_defaultWindowPathResources}");
                return null;
            }

            var window = Instantiate(args.prefab, args.canvas.transform, false);
            window._rectT.sizeDelta = new Vector2(args.width, args.height);
            window._rectT.anchorMin = new Vector2(0.5f, 0.5f);
            window._rectT.anchorMax = new Vector2(0.5f, 0.5f);
            window._rectT.anchoredPosition = Vector2.zero;

            window.tmp.text = args.message;
            
            foreach (var btnInfo in args.buttons)
            {
                var btn = Instantiate(window.btnPrefab, window.buttonsGroup.transform);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = btnInfo.text;
                btn.onClick.AddListener(() =>
                {
                    btnInfo.action?.Invoke();
                    window.Close();
                });
                btn.gameObject.SetActive(true);
            }


            return window;
        }
        
        private void OnEnable()
        {
            _rectT = transform.GetChild(0).GetComponent<RectTransform>();
        }

        public void Close()
        {
            Destroy(gameObject);
        }
        
        public class ModalWindowOptions
        {
            public Canvas canvas;
            public string message;
            public List<ModalWindowBtn> buttons;
            public ModalWindow prefab;
            public int width = 450;
            public int height = 300;
        }
        
        public class ModalWindowBtn
        {
            public Action action;
            public string text;

            public ModalWindowBtn(string text, Action action)
            {
                this.text = text;
                this.action = action;
            }
        }
    }
}

