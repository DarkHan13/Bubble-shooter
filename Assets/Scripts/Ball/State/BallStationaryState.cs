using UnityEngine;

namespace Ball
{
    public class BallStationaryState : BallState
    {
        public Vector2 Coord { get; private set; }
        
        public BallStationaryState(BallStateMachine stateMachine) : base(stateMachine)
        {
            Coord = GameField.I.GetCoordByGlobalPos(stateMachine.transform.position);
            GameField.I.BallDict.TryAdd(Coord, stateMachine);
        }

        public override void Enter()
        {
            StateMachine.Physics.SetVelocity(Vector2.zero);
            StateMachine.Physics.enabled = false;
        }

        public override void Exit()
        {
            GameField.I.BallDict.Remove(Coord);
        }
    }
}