using Game;
using UnityEngine;

namespace Ball.State
{
    public class BallFallingState : BallState
    {
        private BallPhysics _physics;
        
        public BallFallingState(BallStateMachine stateMachine) : base(stateMachine)
        {
            _physics = stateMachine.Physics;
        }

        public override void Enter()
        {
            _physics.collisionSimulation = false;
            _physics.ClearAdditions();
            _physics.AddGravity();
            _physics.AddVelocity(new Vector2(Random.Range(-1f, 1f), 0));
            _physics.enabled = true;
        }

        public override void Update()
        {
            if (_physics.transform.position.y < GameField.I.lowerY)
            {
                var v = _physics.GetVelocity();
                v.y *= -0.8f;
                _physics.SetVelocity(v);
                StateMachine.SetState(BallStateEnum.Popped);
            }
        }
    }
}