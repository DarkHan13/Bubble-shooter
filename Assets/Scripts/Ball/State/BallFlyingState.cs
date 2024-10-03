using System;
using System.Collections;
using System.Collections.Generic;
using Ball;
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
    }

    public override void Update()
    {
        // var coord = GameField.I.GetCoordByGlobalPos(_physics.transform.position);
        // if (GameField.I.GetBallByCoord(coord) == null && 
        //     GameField.I.GetBallPositionByCoord(coord, out var position))
        // {
        //     _lastValidPosition = position;
        // }
        _collisions = _physics.CheckCollisions();
        
        Debug.Log(_collisions.Count);
        if (_collisions.Count == 0) return;
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
        float minDistance = Single.PositiveInfinity;
        foreach (var nCoord in neighboursCoords)
        {
            GameField.I.GetBallPositionByCoord(nCoord, out var neighbourPos);
            var d = Vector2.Distance(_lastValidPosition, neighbourPos);
            if (d < minDistance)
            {
                minDistance = d;
                _lastValidPosition = neighbourPos;
            }
        }
        
        _physics.transform.position = _lastValidPosition;
        
        StateMachine.SetState(BallStateMachine.BallStateEnum.Static);
        GameField.I.TriggerChainReactionFor(GameField.I.GetCoordByGlobalPos(_lastValidPosition));
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_lastValidPosition, 0.1f);
    }
    
    
}
