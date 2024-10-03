using System;
using System.Collections;
using System.Collections.Generic;
using Ball;
using UnityEngine;

public class BallStateMachine : MonoBehaviour
{
    public enum BallStateEnum
    {
        PlayerBall,
        Flying,
        Static,
        Falling,
        Popped,
    }
    
    public enum BallType
    {
        None = 0,
        Red = 1,
        Yellow = 2,
        Blue = 3,
        Purple = 4,
        Green = 5
    }

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float minSpeed = 15f, maxSpeed = 25f;
    [SerializeField] private Vector2 draggingRange = new Vector2(0.25f, 1f);

    public BallPhysics Physics { get; private set; }
    private BallState _state = new BallState(null);
    public BallType Type { get; private set; }

    public event Action<BallStateEnum> OnStateChange; 


    public void Init()
    {
        sr = GetComponent<SpriteRenderer>();
        Physics = GetComponent<BallPhysics>();
        Physics.Init();
        GameField.I.BallSet.Add(this);

    }

    private void OnDestroy()
    {
        if (GameField.I == null) return;
        GameField.I.OnBallDestroy(this);
    }

    private void Update()
    {
        _state.Update();
    }

    public void SetType(BallType ballType)
    {
        Type = ballType;
        Color color = Color.gray;
        switch (Type)
        {
            case BallType.Red:
                color = Color.red;
                break;
            case BallType.Yellow:
                color = Color.yellow;
                break;
            case BallType.Blue:
                color = Color.blue;
                break;
            case BallType.Purple:
                color = new Color(139 / 255f, 0, 255);
                break;
            case BallType.Green:
                color = Color.green;
                break;
        }

        sr.color = color;
    }

    public void SetState(BallStateEnum newState)
    {
        _state.Exit();
        switch (newState)
        {
            case BallStateEnum.PlayerBall:
                _state = new BallController(this, minSpeed, maxSpeed, draggingRange);
                break;
            case BallStateEnum.Flying:
                _state = new BallFlyingState(this);
                break;
            case BallStateEnum.Static:
                _state = new BallStationaryState(this);
                break;
            case BallStateEnum.Popped:
                _state = new BallPoppedState(this);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        _state.Enter();
        OnStateChange?.Invoke(newState);
    }

    private void OnMouseDown()
    {
        _state.OnMouseDown();
    }

    private void OnMouseUp()
    {
        _state.OnMouseUp();
    }

    private void OnDrawGizmos()
    {
        if (_state is BallFlyingState f) f.OnDrawGizmos(); 
    }
}
