﻿using UnityEngine;

namespace Ball.State
{
    public class BallMovingState : BallState
    {
        private BallPhysics _physics;
        private float _moveTimer;
        private readonly float _moveDuration = 0.5f;
        private Vector3 _originalPosition, _targetPosition;

        public BallMovingState(BallStateMachine stateMachine, float moveDuration) : base(stateMachine)
        {
            _moveDuration = moveDuration;
            _physics = stateMachine.Physics;
            _originalPosition = stateMachine.transform.position;
            _targetPosition = stateMachine.Context.TargetPosition;
            StateMachine.gameObject.name = "Moving";
        }

        public override void Enter()
        {
            _physics.enabled = false;
            _physics.ClearAdditions();
            _physics.SetVelocity(Vector2.zero);
        }

        public override void Update()
        {
            _moveTimer += Time.deltaTime;

            float progress = _moveTimer / _moveDuration;
            StateMachine.transform.position = Vector3.Lerp(_originalPosition, _targetPosition, progress);

            if (_moveTimer >= _moveDuration)
            {
                StateMachine.SetState(StateMachine.Context.NextState);
            }
        }
    }
}