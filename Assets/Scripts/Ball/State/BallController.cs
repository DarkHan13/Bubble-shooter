using System;
using System.Collections;
using System.Collections.Generic;
using Ball;
using UnityEngine;

public class BallController : BallState
{
    
    private float _minSpeed = 10f, _maxSpeed = 20f;
    private Vector2 _draggingRange, _startPoint;
    private bool _isDragging;
    private Vector2 _launchDirection;

    private BallPhysics _physics;
    

    public BallController(BallStateMachine stateMachine, float minSpeed, float maxSpeed, Vector2 draggingRange) : base(stateMachine)
    {
        _startPoint = StateMachine.transform.position;
        _physics = StateMachine.Physics;

        _minSpeed = minSpeed;
        _maxSpeed = maxSpeed;
        _draggingRange = draggingRange;
    }

    public override void Enter()
    {
        _physics.enabled = false;
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
            var points = _physics.PredictTrajectory(ResultVelocity());
            BallTrajectoryDrawer.I.Draw(points, 0.5f);
        } else BallTrajectoryDrawer.I.Clear();

    }
    

    public override void OnMouseDown()
    {
        _startPoint = StateMachine.transform.position;
        _isDragging = true;
        _physics.SetVelocity(Vector2.zero);
        _physics.enabled = false;
    }

    public override void OnMouseUp()
    {
        _isDragging = false;
        BallTrajectoryDrawer.I.Clear();
        if (!CanLaunch())
        {
            _launchDirection = Vector2.zero;
            StateMachine.transform.position = _startPoint;
            return;
        }
        Launch();
    }

    private bool CanLaunch() => _launchDirection.magnitude >= _draggingRange.x;
    
    private void Launch()
    {
        _physics.enabled = true;
        _physics.SetVelocity(ResultVelocity());
        
        _launchDirection = Vector2.zero;
        
        StateMachine.SetState(BallStateMachine.BallStateEnum.Flying);
    }

    private Vector2 ResultVelocity()
    {
        // var percent = Math.Clamp(_launchDirection.magnitude, 0, draggingRange.y);
        // percent /= draggingRange.y;
        // return _launchDirection.normalized * (percent * maxSpeed);
        
        var length = Math.Clamp(_launchDirection.magnitude, _draggingRange.x, _draggingRange.y);
        var value = Mathf.InverseLerp(_draggingRange.x, _draggingRange.y, length);
        return _launchDirection.normalized * (_minSpeed + value * (_maxSpeed - _minSpeed));
    }
}
