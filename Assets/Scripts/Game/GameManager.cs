using System;
using System.Collections;
using System.Collections.Generic;
using Ball.State;
using Game.Level;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager I;
    
        [SerializeField] private BallStateMachine ballPrefab;
        [SerializeField] private TextMeshPro remainingBallsTmp;

        [HideInInspector] public LevelData.PlayerBallsData playerBallsData = new (15);
        public Vector2 playerSpawnPos = new (0, -4f);
        public float limitX, upperY, offset = 0.1f;
        [Range(0.1f, 1f)] public float radius = 0.5f;
        // Just to see the result in inspector
        public int OddColumns, EvenColumns;
        
        [Header("UI")]
        [SerializeField] private ScoreUI scoreUI;
        [SerializeField] private CanvasGroupAlphaController gameplayCanvasGroupAlpha;

        public GameField Field { get; private set; }
        
        private int _currentScore;
        public int CurrentScore
        {
            get => _currentScore;
            private set
            {
                if (_currentScore != value) scoreUI.SetScore(value);
                _currentScore = value;
            }
        }

        private BallStateMachine _playerBall;
        private List<BallStateMachine> _remainingBalls;
        private bool _gameIsOver;

        // true - win / false - lost
        public event Action<bool> OnGameOver; 

        private void Awake()
        {
            I = this;

            UpdateField();
        }

        private void OnDestroy()
        {
            GameField.Destroy();
        }

        private void OnValidate()
        {
            UpdateField();
            EvenColumns = Field.CachedEvenColumns;
            OddColumns = Field.CachedOddColumns;
        }
        private void Start()
        {
            // Set 60 fps
            Application.targetFrameRate = 60;
            CurrentScore = 0;

            LoadLevel(0);
        }

        public void LoadLevel(LevelData levelData)
        {
            _gameIsOver = false;
            var sceneBalls = FindObjectsByType<BallStateMachine>(FindObjectsSortMode.None);
            foreach (var sceneBall in sceneBalls)
            {
                Destroy(sceneBall.gameObject);
            }
            
            playerSpawnPos = levelData.playerSpawnPos;
            playerBallsData = levelData.playerBallsData;
            radius = levelData.radius;
            offset = levelData.offset;
            ResetField();
            // field
            foreach (var ballInfo in levelData.balls)
            {
                var ball = CreateBall(Field.GetBallPositionByCoord(ballInfo.pos));
                ball.transform.SetParent(transform);
                ball.SetType(ballInfo.type);
                ball.SetState(BallStateEnum.Static);
                ball.OnStateChange += OnStaticBallStateChanged;
            }
            
            // player balls
            _remainingBalls = new List<BallStateMachine>();
            var remainingBallPos = remainingBallsTmp.transform.position;
            var l = playerBallsData.isRandom ? playerBallsData.ballCount : playerBallsData.balls.Length;
            for (int i = 0; i < l; i++)
            {
                var ball = CreateBall(Vector3.zero);
             
                BallType type = playerBallsData.isRandom ? (BallType)Random.Range(1, 6): playerBallsData.balls[i];
                ball.SetType(type);
                ball.SetState(BallStateEnum.Waiting);
                ball.transform.position = remainingBallPos;
                remainingBallPos.x -= 2 * radius;
                _remainingBalls.Add(ball);
            }

            NextPlayerBall();
            Field.AfterChainReaction += CheckEndGame;
        }

        public void LoadLevel(int levelIndex)
        {
            LevelData levelData = LevelLoader.FileToLevel(LevelLoader.GetAllFileLevels()[levelIndex]);
            LoadLevel(levelData);
        }


        private void Update()
        {
        
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.P))
                UnityEditor.EditorApplication.isPaused = true;
#endif
        }

        public void ResetField()
        {
            GameField.Destroy();
            Field = new GameField(limitX, upperY, GetCameraBottomBoundary(), radius, offset);
        }

        public void UpdateField()
        {
            if (Field == null)
            {
                ResetField();
                return;
            }

            Field.UpdateData(limitX, upperY, GetCameraBottomBoundary(), radius, offset);
        }

        // Create ball and set Context
        private BallStateMachine CreateBall(Vector3 position)
        {
            var ball = Instantiate(ballPrefab);
            ball.Context.TargetPosition = position;
            ball.Context.InstantMove = true;
            ball.transform.localScale = Vector3.one * (radius * 2);
            ball.Init();
            return ball;
        }


        private void OnPlayerBallStateChanged(BallStateEnum newState)
        {
            if (newState is BallStateEnum.PlayerBall)
            {
                var ballController = (BallController)_playerBall.State;
                ballController.OnDragging += delegate(bool b)
                {
                    gameplayCanvasGroupAlpha.Show(!b);
                };
                return;
            }
            if (newState is not (BallStateEnum.Static or BallStateEnum.Popped)) return;
            if (newState is BallStateEnum.Static)
            {
                _playerBall.OnStateChange += OnStaticBallStateChanged;
            }
            _playerBall.OnStateChange -= OnPlayerBallStateChanged;
            NextPlayerBall();
        }
        
        private void OnStaticBallStateChanged(BallStateEnum newState)
        {
            if (newState is not BallStateEnum.Popped) return;
            CurrentScore += 20;
            scoreUI.SetScore(CurrentScore);
        }

        private void CheckEndGame()
        {
            if (_gameIsOver) return;
            var numberBalls = Field.BallDict.Count;
            if (numberBalls != 0 && _playerBall != null) return;
            CurrentScore += _remainingBalls.Count * 50;
            _gameIsOver = true;
            StartCoroutine(EndGameCo());
        }

        IEnumerator EndGameCo()
        {
            var balls = Field.BallSet;
            yield return new WaitForSeconds(0.5f);
            bool allBallsDestroyed = false;
            while (!allBallsDestroyed)
            {
                allBallsDestroyed = true;
                foreach (var ball in balls)
                {
                    if (ball.State is (BallFallingState))
                    {
                        allBallsDestroyed = false;
                        break;
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            var numberBalls = Field.BallDict.Count;
            if (numberBalls > 0)
            {
                // Lost
                CurrentScore = Math.Max(0, CurrentScore - 100);
                OnGameOver?.Invoke(false);
                yield break;
            }

            OnGameOver?.Invoke(true);

        }

        private void NextPlayerBall()
        {
            if (_remainingBalls.Count == 0)
            {
                remainingBallsTmp.text = ":(";
                _playerBall = null;
                CheckEndGame();
                return;
            }
            _playerBall = _remainingBalls[0];
            _remainingBalls.RemoveAt(0);
            _playerBall.Context.NextState = BallStateEnum.PlayerBall;
            _playerBall.Context.FloatArgs = new[] { 0.5f };
            _playerBall.Context.TargetPosition = playerSpawnPos;
            _playerBall.SetState(BallStateEnum.Moving);
            _playerBall.Context.FloatArgs = new[] { _playerBall.minSpeed, _playerBall.maxSpeed };
            _playerBall.Context.Vector2Args = new[] { _playerBall.draggingRange };
            _playerBall.OnStateChange += OnPlayerBallStateChanged;
            
            remainingBallsTmp.text = _remainingBalls.Count.ToString();
            
            foreach (var ball in _remainingBalls)
            {
                ball.Context.NextState = BallStateEnum.Waiting;
                ball.Context.TargetPosition = ball.transform.position;
                ball.Context.TargetPosition.x += 2*radius;
                ball.SetState(BallStateEnum.Moving);
            }
        }
        
        public static float GetCameraBottomBoundary()
        {
            Camera cam = Camera.main;
            Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            return bottomLeft.y;
        }
    
        private void OnDrawGizmos()
        {
            if (Field == null) UpdateField(); 
        
            Gizmos.color = Color.red;

            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0f;
            Gizmos.DrawSphere(worldPosition, 0.05f);
            if (Field.GetBallPositionByGlobalPos(worldPosition, out var ballPos))
            {
                Gizmos.DrawSphere(ballPos, radius);
                // var neighbours = Field.GetNeighbours(Field.GetCoordByGlobalPos(worldPosition), null);
                // foreach (var pCoord in neighbours)
                // {
                //     if (Field.GetBallPositionByCoord(pCoord, out ballPos))
                //         Gizmos.DrawSphere(ballPos, radius);
                // }
            }
        
            Gizmos.color = Color.green;
            // Gizmos.DrawLine(new Vector2(-limitX, -10f), new Vector2(-limitX, 10f));
            // Gizmos.DrawLine(new Vector2(limitX, -10f), new Vector2(limitX, 10f));
            // Gizmos.DrawLine(new Vector2(-10, upperY), new Vector2(10, upperY));
            // Gizmos.DrawLine(new Vector2(-10, lowerY), new Vector2(10, lowerY));

            var positions = Field.GenerateField();
            EvenColumns = Field.CachedEvenColumns;
            OddColumns = Field.CachedOddColumns;
            if (positions == null) return;
            foreach (var position in positions)
            {
                Gizmos.DrawWireSphere(position, radius);
            }
            
            Gizmos.DrawWireSphere(playerSpawnPos, radius);

        }
    }
}
