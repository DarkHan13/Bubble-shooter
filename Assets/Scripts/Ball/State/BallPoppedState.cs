using UnityEngine;

namespace Ball.State
{
    public class BallPoppedState : BallState
    {
        private float _popTimer;
        private readonly float popDuration = 0.5f;
        private bool _isPopping;
        private Vector3 _originalScale;
        
        public BallPoppedState(BallStateMachine stateMachine) : base(stateMachine)
        {
            _originalScale = stateMachine.transform.localScale;
        }

        public override void Enter()
        {
            // StateMachine.Physics.enabled = false;
            StateMachine.Physics.collisionSimulation = false;
            _isPopping = true;
        }

        public override void Update()
        {
            if (_isPopping)
            {
                _popTimer += Time.deltaTime;

                float progress = _popTimer / popDuration;
                StateMachine.transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, progress);

                if (_popTimer >= popDuration)
                {
                    _isPopping = false;
                    GameObject.Destroy(StateMachine.gameObject);
                }
            }
        }
    }
}