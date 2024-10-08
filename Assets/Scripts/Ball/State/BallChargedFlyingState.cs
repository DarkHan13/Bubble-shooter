using System;
using System.Collections;
using System.Collections.Generic;
using Ball;
using Ball.State;
using DMath;
using Game;
using UnityEngine;

public class BallChargedFlyingState : BallState
{
    private BallPhysics _physics;
    private List<BallPhysics.BallCollisionInfo> _collisions;
    private Vector2 _lastValidPosition;

    public BallChargedFlyingState(BallStateMachine stateMachine) : base(stateMachine)
    {
        _physics = StateMachine.Physics;
    }

    public override void Enter()
    {
        _physics.gravitySimulation = true;
        _physics.enabled = true;
        _physics.SetVelocity(StateMachine.Context.Velocity);
        _physics.AddGravity();
    }

    public override void Update()
    {
        Vector2 position = _physics.transform.position;

        if (position.y < GameField.I.lowerY)
        {
            StateMachine.SetState(BallStateEnum.Popped);
        }
        if (CheckCollisionWithBalls())
        {
            SetStaticState(_lastValidPosition);
        }
        else
        {
            var yLine = GameField.I.upperY - GameField.I.radius;
            var predictPosition = position + _physics.GetVelocity() * Time.fixedDeltaTime;
            if (predictPosition.y >= yLine)
            {
                var upperLine = new Line(new Vector2(-100, yLine), new Vector2(100, yLine));
                var intersection = _physics.GetIntersectionWithLine(position, _physics.GetVelocity(), upperLine);
                if (intersection != position)
                {
                    SetStaticState(intersection);
                }
            }
        }
    }

    private void SetStaticState(Vector2 position)
    {
        StateMachine.Context.TargetPosition = position;
        StateMachine.Context.InstantMove = false;
        StateMachine.SetState(BallStateEnum.Static);
        GameField.I.TriggerChainReactionFor(GameField.I.GetCoordByGlobalPos(position));
    }

    private bool CheckCollisionWithBalls()
    {
        _collisions = _physics.CheckCollisions();
        
        if (_collisions.Count == 0) return false;
        _physics.enabled = false;

        var cInfo = _collisions[0];
        var collidedBallPos = cInfo.HitBallPhysics.transform.position;
        var coord = GameField.I.GetCoordByGlobalPos(collidedBallPos);
        var collidedBall = GameField.I.GetBallByCoord(coord);

        collidedBall.SetState(BallStateEnum.Popped);
        
        _lastValidPosition = collidedBallPos;
        
        return true;
    }


    
    
}
