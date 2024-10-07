using System.Collections.Generic;
using Ball.State;
using Game.Level;
using TMPro;
using UnityEngine;
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

        public GameField Field { get; private set; }

        private BallStateMachine _playerBall;
        private List<BallStateMachine> _remainingBalls = new ();

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
            
            var sceneBalls = FindObjectsByType<BallStateMachine>(FindObjectsSortMode.None);
            foreach (var sceneBall in sceneBalls)
            {
                Destroy(sceneBall.gameObject);
            }
            
            LevelData levelData = LevelLoader.GetRandomLevel();
            playerSpawnPos = levelData.playerSpawnPos;
            playerBallsData = levelData.playerBallsData;
            radius = levelData.radius;
            offset = levelData.offset;
            UpdateField();
            foreach (var ballInfo in levelData.balls)
            {
                var ball = CreateBall(Field.GetBallPositionByCoord(ballInfo.pos));
                ball.transform.SetParent(transform);
                ball.SetType(ballInfo.type);
                ball.SetState(BallStateEnum.Static);
            }
            
            // CreatePlayerBall();

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
        private void CreatePlayerBall()
        {
            _playerBall = CreateBall(playerSpawnPos);
            _playerBall.SetType((BallType)Random.Range(1, 6));
            _playerBall.SetState(BallStateEnum.PlayerBall);
            _playerBall.OnStateChange += OnPlayerBallStateChanged;
        }

        private void OnPlayerBallStateChanged(BallStateEnum newState)
        {
            if (newState is not (BallStateEnum.Static or BallStateEnum.Popped)) return;
            _playerBall.OnStateChange -= OnPlayerBallStateChanged;
            NextPlayerBall();
        }

        private void NextPlayerBall()
        {
            if (_remainingBalls.Count == 0)
            {
                Debug.Log("You lost");
                remainingBallsTmp.text = ":(";
                return;
            }
            _playerBall = _remainingBalls[0];
            _remainingBalls.RemoveAt(0);
            _playerBall.Context.NextState = BallStateEnum.PlayerBall;
            _playerBall.Context.TargetPosition = playerSpawnPos;
            _playerBall.SetState(BallStateEnum.Moving);
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
