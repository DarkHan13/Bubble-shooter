using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    // Update is called once per frame
    void Update()
    {
        text.text = $"{(1 / Time.deltaTime).ToString("00")} {(1 / Time.fixedDeltaTime).ToString("00")}";
    }

    private bool _fpsChanged;
    private int prevFps;
    public void ChangeFps()
    {
        if (!_fpsChanged)
        {
            prevFps = Application.targetFrameRate;
            _fpsChanged = true;
            Application.targetFrameRate = 60;
        }
        else
        {
            Application.targetFrameRate = prevFps;
            _fpsChanged = false;
        }
    }
}
