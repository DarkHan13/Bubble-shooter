using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    
    [SerializeField] private BallStateMachine ballPrefab;

    [SerializeField] private Vector2 playerSpawnPos = new Vector2(0, -4f);
    [SerializeField] private float limitX, upperY, lowerY, offset = 0.1f;
    [SerializeField, Range(0.1f, 1f)] float radius = 0.5f;
    public int OddColumns, EvenColumns;

    public GameField Field { get; private set; }

    private BallStateMachine _playerBall;

    private void Awake()
    {
        I = this;

        GameField.Destroy();
        Field = new GameField(limitX, upperY, radius, offset);
    }

    private void OnDestroy()
    {
        GameField.Destroy();
    }

    private void OnValidate()
    {
        Field = new GameField(limitX, upperY, radius, offset);
        EvenColumns = Field.EvenColumns;
        OddColumns = Field.OddColumns;
    }
    private void Start()
    {
        var positions = GameField.I.GenerateField();

        foreach (var pos in positions)
        {
            var ball = CreateBall(pos);
            ball.transform.SetParent(transform);
            ball.SetType((BallStateMachine.BallType)Random.Range(1, 6));
            ball.SetState(BallStateMachine.BallStateEnum.Static);
        }
        
        CreatePlayerBall();
    }


    private void Update()
    {
        
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
            UnityEditor.EditorApplication.isPaused = true;
#endif
    }

    private BallStateMachine CreateBall(Vector3 position)
    {
        var ball = Instantiate(ballPrefab, position, Quaternion.identity);
        ball.transform.localScale = Vector3.one * (radius * 2);
        ball.Init();
        return ball;
    }
    private void CreatePlayerBall()
    {
        _playerBall = CreateBall(playerSpawnPos);
        _playerBall.SetType((BallStateMachine.BallType)Random.Range(1, 6));
        _playerBall.SetState(BallStateMachine.BallStateEnum.PlayerBall);
        _playerBall.OnStateChange += OnPlayerBallStateChanged;
    }

    private void OnPlayerBallStateChanged(BallStateMachine.BallStateEnum newState)
    {
        if (newState != BallStateMachine.BallStateEnum.Static) return;
        
        _playerBall.OnStateChange -= OnPlayerBallStateChanged;
        CreatePlayerBall();
    }
    
    private void OnDrawGizmos()
    {
        if (Field == null) Field = new GameField(limitX, upperY, radius, offset); 
        
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
        Gizmos.DrawLine(new Vector2(-limitX, -10f), new Vector2(-limitX, 10f));
        Gizmos.DrawLine(new Vector2(limitX, -10f), new Vector2(limitX, 10f));
        Gizmos.DrawLine(new Vector2(-10, upperY), new Vector2(10, upperY));
        Gizmos.DrawLine(new Vector2(-10, lowerY), new Vector2(10, lowerY));

        var positions = Field.GenerateField();
        EvenColumns = Field.EvenColumns;
        OddColumns = Field.OddColumns;
        if (positions == null) return;
        foreach (var position in positions)
        {
            Gizmos.DrawWireSphere(position, radius);
        }

    }
}
