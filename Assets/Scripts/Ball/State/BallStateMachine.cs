using System;
using Game;
using UnityEngine;

namespace Ball.State
{
    public class BallStateMachine : MonoBehaviour
    {

        [SerializeField] private SpriteRenderer sr;
        [SerializeField] public float minSpeed = 15f, maxSpeed = 25f;
        [SerializeField] public Vector2 draggingRange = new (0.25f, 1f);

        public BallPhysics Physics { get; private set; }
        private BallState _state = new (null);
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
            // _state = newState switch
            // {
            //     BallStateEnum.Waiting => new BallWaitingState(this),
            //     BallStateEnum.Moving => new BallMovingState(this),
            //     BallStateEnum.PlayerBall => new BallController(this),
            //     BallStateEnum.Flying => new BallFlyingState(this),
            //     BallStateEnum.ChargedFlying => new BallChargedFlyingState(this),
            //     BallStateEnum.Static => new BallStationaryState(this),
            //     BallStateEnum.Falling => new BallFallingState(this),
            //     BallStateEnum.Popped => new BallPoppedState(this),
            //     _ => _state
            // };

            _state = newState.CreateState(this);
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
        public BallStateEnum NextState = BallStateEnum.Waiting;

        public int[] IntArgs = Array.Empty<int>();
        public float[] FloatArgs = Array.Empty<float>();
        public Vector2[] Vector2Args = Array.Empty<Vector2>();
    }
}


