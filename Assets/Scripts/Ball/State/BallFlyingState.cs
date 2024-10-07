using System;
using System.Collections;
using System.Collections.Generic;
using Ball;
using Ball.State;
using DMath;
using Game;
using UnityEngine;

public class BallFlyingState : BallState
{
    private BallPhysics _physics;
    private List<BallPhysics.BallCollisionInfo> _collisions;
    private Vector2 _lastValidPosition;

    public BallFlyingState(BallStateMachine stateMachine) : base(stateMachine)
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
        HashSet<Vector2Int> neighboursCoords = new ();
        bool Filter(Vector2Int nCoord)
        {
            var b = GameField.I.GetBallByCoord(nCoord);
            return b == null;
        }
        foreach (var cInfo in _collisions)
        {
            var coord = GameField.I.GetCoordByGlobalPos(cInfo.HitBallPhysics.transform.position);
            var neighboursList = GameField.I.GetNeighbours(coord, Filter);
            foreach (var i in neighboursList)
            {
                neighboursCoords.Add(i);
            }
        }

        _lastValidPosition = _physics.transform.position;
        Vector2 position = _lastValidPosition;
        float minDistance = Single.PositiveInfinity;
        foreach (var nCoord in neighboursCoords)
        {
            GameField.I.TryGetBallPositionByCoord(nCoord, out var neighbourPos);
            var d = Vector2.Distance(position, neighbourPos);
            if (d < minDistance)
            {
                minDistance = d;
                _lastValidPosition = neighbourPos;
            }
        }

        return true;
    }


    
    
}
