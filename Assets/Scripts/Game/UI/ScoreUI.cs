using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tmp;
        [SerializeField] private float changeScoreAnimDuration = 0.5f;
        
        private bool _isAnimating;
        private float _timer;
        private int _currentScore, _plusScore;
        
        private void OnEnable()
        {
            if (tmp == null) tmp = GetComponent<TextMeshProUGUI>();
        }

        public void SetScore(int score)
        {
            _timer = 0;
            _isAnimating = true;
            _plusScore = score - _currentScore;
            StringBuilder sb = new StringBuilder();
            sb.Append($"Score: {score}");
            if (_plusScore >= 0) sb.Append('+');
            sb.Append(_plusScore.ToString());
            tmp.text = sb.ToString();
        }

        private void Update()
        {
            if (_isAnimating)
            {
                _timer += Time.deltaTime;
                if (_timer >= changeScoreAnimDuration)
                {
                    _isAnimating = false;
                    _currentScore += _plusScore;
                    _plusScore = 0;
                    tmp.text = $"Score: {_currentScore}";
                }
            }
        }


    }
}

