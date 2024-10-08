using System;
using Game;
using UnityEngine;

namespace Ball.State
{
    public class BallStateMachine : MonoBehaviour
    {

        [SerializeField] private SpriteRenderer sr;
        [SerializeField] private float minSpeed = 15f, maxSpeed = 25f;
        [SerializeField] private Vector2 draggingRange = new Vector2(0.25f, 1f);

        public BallPhysics Physics { get; private set; }
        private BallState _state = new BallState(null);
        public BallState State => _state;
        private Camera _mainCamera;
        public BallType Type { get; private set; }
        public StateContext Context = new ();

        public event Action<BallStateEnum> OnStateChange;
        public event Action<BallStateMachine> OnMouseDownEvent; 


        public void Init()
        {
            sr = GetComponent<SpriteRenderer>();
            SetType();
            Physics = GetComponent<BallPhysics>();
            Physics.Init();
            GameField.I.BallSet.Add(this);

        }

        private void OnDestroy()
        {
            if (GameField.I == null) return;
            GameField.I.OnBallDestroy(this);
        }

        private void Update()
        {
            _state.Update();
        }

        public void SetType(BallType ballType)
        {
            Type = ballType;
            sr.color = Type.GetColor();
        }

        public void SetType()
        {
            var color = sr.color;
            SetType(color.GetBallType());
        }

        public void SetState(BallStateEnum newState)
        {
            _state.Exit();
            switch (newState)
            {
                case BallStateEnum.Waiting:
                    _state = new BallWaitingState(this);
                    break;
                case BallStateEnum.Moving:
                    _state = new BallMovingState(this, 0.5f);
                    break;
                case BallStateEnum.PlayerBall:
                    _state = new BallController(this, minSpeed, maxSpeed, draggingRange);
                    break;
                case BallStateEnum.Flying:
                    _state = new BallFlyingState(this);
                    break;
                case BallStateEnum.ChargedFlying:
                    _state = new BallChargedFlyingState(this);
                    break;
                case BallStateEnum.Static:
                    _state = new BallStationaryState(this);
                    break;
                case BallStateEnum.Falling:
                    _state = new BallFallingState(this);
                    break;
                case BallStateEnum.Popped:
                    _state = new BallPoppedState(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
            _state.Enter();
            OnStateChange?.Invoke(newState);
        }
        
        private void OnMouseDown()
        {
            OnMouseDownEvent?.Invoke(this);
            _state.OnMouseDown();
        }

        private void OnMouseUp()
        {
            _state.OnMouseUp();
        }
    }

    public class StateContext
    {
        public Vector2 TargetPosition;
        public bool InstantMove = true;
        public Vector2 Velocity;
        public BallStateEnum NextState;
    }
}


