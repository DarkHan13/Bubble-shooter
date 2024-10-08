using System;
using System.Collections.Generic;
using Ball;
using Ball.State;
using UnityEngine;
using DMath;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class BallController : BallState
{
    
    private float _minSpeed = 10f, _maxSpeed = 20f;
    private Vector2 _draggingRange, _startPoint;
    private bool _isDragging;
    private Vector2 _launchDirection;

    private BallPhysics _physics;
    // Flying or ChargedFlying
    private BallStateEnum _flyingBallState = BallStateEnum.Flying;
    private BallTrajectoryDrawer[] _drawers;
    public event Action<bool> OnDragging; 


    public BallController(BallStateMachine stateMachine) : base(stateMachine)
    {
        _startPoint = StateMachine.Context.TargetPosition;
        StateMachine.transform.position = _startPoint;
        _physics = StateMachine.Physics;

        _minSpeed = StateMachine.Context.FloatArgs[0];
        _maxSpeed = StateMachine.Context.FloatArgs[1];
        _draggingRange = StateMachine.Context.Vector2Args[0];
    }

    public override void Enter()
    {
        _physics.enabled = false;
        _physics.AddGravity();
        
        _drawers = new BallTrajectoryDrawer[2];
        for (var i = 0; i < _drawers.Length; i++)
        {
            _drawers[i] = BallTrajectoryDrawer.CreateTrajectoryLine();
        }
    }

    public override void Exit()
    {
        _physics.ClearAdditions();
        foreach (var ballTrajectoryDrawer in _drawers)
        {
            Object.Destroy(ballTrajectoryDrawer.gameObject);
        }
    }
    
    
    public override void OnMouseDown()
    {
        _startPoint = StateMachine.transform.position;
        _isDragging = true;
        OnDragging?.Invoke(true);
        _physics.SetVelocity(Vector2.zero);
        _physics.enabled = false;
    }

    public override void OnMouseUp()
    {
        _isDragging = false;
        OnDragging?.Invoke(false);
        ClearTrajectory();
        if (!CanLaunch())
        {
            _launchDirection = Vector2.zero;
            StateMachine.transform.position = _startPoint;
            return;
        }
        Launch();
    }
    
    public override void Update()
    {
        if (!_isDragging) return;
        Vector2 currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _launchDirection = _startPoint - currentPoint;

        var distance = Mathf.Clamp(_launchDirection.magnitude, 0, _draggingRange.y);
        StateMachine.transform.position = _startPoint - _launchDirection.normalized * distance;

        if (CanLaunch())
        {
            UpdateFlyingBallState();
            DrawTrajectory();
        } else ClearTrajectory();

    }
    

    private bool CanLaunch() => _launchDirection.magnitude >= _draggingRange.x;
    
    private void Launch()
    {
        if (_flyingBallState == BallStateEnum.ChargedFlying)
        {
            _launchDirection = _launchDirection.Rotate(Random.Range(-10, 10));
        }
        StateMachine.Context.Velocity = ResultVelocity();
        // Debug.Log(CurrentDragValue());
        StateMachine.SetState(_flyingBallState);
    }

    private void DrawTrajectory()
    {
        if (_flyingBallState == BallStateEnum.Flying)
        {
            var points = _physics.PredictTrajectory(ResultVelocity());
            _drawers[1].Clear();
            _drawers[0].SetColor(Color.black);
            _drawers[0].Draw(points, 0.5f);
        }
        else
        {
            var tLaunchDirection = _launchDirection;
            _launchDirection = tLaunchDirection.Rotate(-10);
            var points1 = _physics.PredictTrajectory(ResultVelocity());
            _launchDirection = tLaunchDirection.Rotate(10);
            var points2 = _physics.PredictTrajectory(ResultVelocity());
            _launchDirection = tLaunchDirection;
            
            foreach (var drawer in _drawers) drawer.SetColor(Color.red);
            _drawers[0].Draw(points1, 0.5f);
            _drawers[1].Draw(points2, 0.5f);
        }
    }
    

    private void ClearTrajectory()
    {
        for (var i = 0; i < _drawers.Length; i++)
        {
            _drawers[i].Clear();
        }
    }

    private void UpdateFlyingBallState()
    {
        _flyingBallState = CurrentDragValue() < 1 ? BallStateEnum.Flying : BallStateEnum.ChargedFlying;
    }

    private Vector2 ResultVelocity()
    {
        var value = CurrentDragValue();
        return _launchDirection.normalized * (_minSpeed + value * (_maxSpeed - _minSpeed));
    }

    private float CurrentDragValue()
    {
        var length = Math.Clamp(_launchDirection.magnitude, _draggingRange.x, _draggingRange.y);
        return Mathf.InverseLerp(_draggingRange.x, _draggingRange.y, length);
    }
}
