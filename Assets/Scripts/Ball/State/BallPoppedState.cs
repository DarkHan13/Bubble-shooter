﻿using UnityEngine;

namespace Ball
{
    public class BallPoppedState : BallState
    {
        private float popTimer, popDuration = 0.5f;
        private bool _isPopping;
        private Vector3 _originalScale;
        
        public BallPoppedState(BallStateMachine stateMachine) : base(stateMachine)
        {
            _originalScale = stateMachine.transform.localScale;
        }

        public override void Enter()
        {
            StateMachine.Physics.enabled = false;
            _isPopping = true;
        }

        public override void Update()
        {
            if (_isPopping)
            {
                popTimer += Time.deltaTime;

                float progress = popTimer / popDuration;
                StateMachine.transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, progress);

                if (popTimer >= popDuration)
                {
                    _isPopping = false;
                    GameObject.Destroy(StateMachine.gameObject);
                }
            }
        }
    }
}