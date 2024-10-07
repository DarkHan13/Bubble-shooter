using Game;
using UnityEngine;

namespace Ball.State
{
    public class BallStationaryState : BallState
    {
        public Vector2Int Coord { get; private set; }
        public bool IsFixed;
        private BallPhysics _physics;
        
        public BallStationaryState(BallStateMachine stateMachine) : base(stateMachine)
        {
            _physics = stateMachine.Physics;
        }

        public override void Enter()
        {
            var context = StateMachine.Context;
            StateMachine.Physics.enabled = true;
            _physics.ClearAdditions();
            _physics.collisionSimulation = true;

            Coord = GameField.I.GetValidCoordByGlobalPos(context.TargetPosition);
            StateMachine.gameObject.name = $"Ball {Coord.x} {Coord.y}";

            var position = GameField.I.GetBallPositionByCoord(Coord);
            _physics.AddSpringJoint(position, 1000f, 0.5f);
            
            if (StateMachine.Context.InstantMove)
            {
                StateMachine.transform.position = position;
            }
            
            GameField.I.BallDict[Coord] = StateMachine;

        }

        public override void Exit()
        {
            GameField.I.BallDict.Remove(Coord);
        }
    }
}