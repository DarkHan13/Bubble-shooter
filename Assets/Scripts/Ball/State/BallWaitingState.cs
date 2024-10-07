using UnityEngine;

namespace Ball.State
{
    public class BallWaitingState : BallState
    {
        private BallPhysics _physics;
        
        public BallWaitingState(BallStateMachine stateMachine) : base(stateMachine)
        {
            _physics = stateMachine.Physics;
            StateMachine.gameObject.name = "Waiting";
        }

        public override void Enter()
        {
            _physics.enabled = false;
            _physics.ClearAdditions();
            _physics.SetVelocity(Vector2.zero);
        }
    }
}