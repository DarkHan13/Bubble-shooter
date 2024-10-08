using System;
using System.Collections;
using System.Collections.Generic;
using Ball.Physics;
using DMath;
using Game;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ball
{
    public class BallPhysics : MonoBehaviour
    {
        private float _xLimit;
        private Vector2 _currentVelocity;
        public float Radius { get; private set; } = 0.5f;

        public bool collisionSimulation = true, gravitySimulation = true;

        private readonly List<BallPhysicsAddition> _additions = new ();
        private BallXLimitAddition _xLimitAddition;

        private void OnEnable()
        {
            Init();
        } 
        
        public void Init()
        {
            Radius = transform.lossyScale.x / 2;
            _xLimit = GameField.I.limitX;
            _xLimitAddition = new BallXLimitAddition(this, GameField.I.limitX);
        }

        private void FixedUpdate()
        {
            if (collisionSimulation) SimulateCollision();
            
            Vector2 position = transform.position;
            
            foreach (var addition in _additions)
            {
                addition.Simulate(Time.fixedDeltaTime, ref position, ref _currentVelocity);
            }

            _xLimitAddition.Simulate(Time.fixedDeltaTime, ref position, ref _currentVelocity);
            
            // var predictPosition = position + _currentVelocity * Time.fixedDeltaTime;
            // if (predictPosition.x + Radius > _xLimit || predictPosition.x - Radius < -_xLimit)
            // {
            //     var xLine = predictPosition.x + Radius > _xLimit ? _xLimit - Radius : -_xLimit + Radius;
            //     Line wallLineWithOffset = new Line(new Vector2(xLine, -100), new Vector2(xLine, 100));
            //     position = GetIntersectionWithLine(predictPosition, _currentVelocity, wallLineWithOffset);
            //     _currentVelocity.x *= -1;
            // }

            
            position += _currentVelocity * Time.fixedDeltaTime;
            transform.position = position;
        }

        public Vector2 GetIntersectionWithLine(Vector2 position, Vector2 velocity, Line l2)
        {
            Vector2 from = position - velocity * 10;
            Vector2 to = position + velocity * 10;
            Line l1 = new Line(from, to);

            // l1.DebugDraw(Color.black, 0.5f);            
            // l2.DebugDraw(Color.black, 0.5f);            
            if (Line.TryGetIntersection(l1, l2, out var intersection))
            {
                // Debug.DrawLine(position, intersection, Color.black, 1f);
                position = intersection;
            }

            return position;
        }
        

        public void SetVelocity(Vector2 velocity)
        {
            if (!enabled) return;
            _currentVelocity = velocity;
        }

        public Vector2 GetVelocity() => _currentVelocity;

        public void AddVelocity(Vector2 velocity)
        {
            if (!enabled) return;
            _currentVelocity += velocity;
        }

        public void AddGravity()
        {
            _additions.Add(new BallGravityAddition(this));
        }

        public void AddSpringJoint(Vector2 joint, float springStrength, float damping)
        {
            _additions.Add(new BallSpringJointAddition(this, joint, springStrength, damping));
        }

        public void ClearAdditions()
        {
            _additions.Clear();
        }
        
        public List<Vector2> PredictTrajectory(Vector2 velocity, float time = 0.5f)
        {
            List<Vector2> points = new List<Vector2>();
            float deltaTime = 0.02f;
            Vector2 position = transform.position;

            points.Add(position);
            for (float i = 0; i < time; i += deltaTime)
            {
                foreach (var addition in _additions)
                {
                    addition.Simulate(deltaTime, ref position, ref velocity);
                }
            
                _xLimitAddition.Simulate(deltaTime, ref position, ref velocity);
                
                position += velocity * deltaTime;

                points.Add(position);
            }

            return points;
        }

        public List<BallCollisionInfo> CheckCollisions(Func<State.BallStateMachine, bool> condition = null)
        {
            condition ??= _ => true;
            var balls = GameField.I.BallSet;
            List<BallCollisionInfo> collisions = new List<BallCollisionInfo>();

            foreach (var ball in balls)
            {
                if (ball.Physics == this) continue;
                if (!AreCirclesColliding(this, ball.Physics, out var distance)) continue;
                if (!condition.Invoke(ball)) continue;
                
                var collision = new BallCollisionInfo();
                collision.Distance = distance;
                collision.HitBallPhysics = ball.Physics;
                collisions.Add(collision);
            }

            return collisions;
        }
        
        private void SimulateCollision()
        {
            var collisions = CheckCollisions(b => b.Physics.enabled);
            
            foreach (var collision in collisions)
            {
                var ballPhysics = collision.HitBallPhysics;
                
                var velocityA = _currentVelocity;
                var velocityB = ballPhysics._currentVelocity;
                Vector2 normal = (transform.position - ballPhysics.transform.position).normalized;
                var relVelocity = Vector2.Dot(velocityB - velocityA, normal);
                
                if (relVelocity < 0) continue;

                float e = 1f;
                float impulse = -(1 + e) * relVelocity / 2;
                Vector2 impulseVector = impulse * normal;
                
                AddVelocity(-impulseVector);;
            }
        }

        private static bool AreCirclesColliding(BallPhysics a, BallPhysics b, out float distance)
        {
            var minDist = a.Radius + b.Radius;
            distance = Vector2.Distance(a.transform.position, b.transform.position);
            return minDist > distance;
        }
        
        public struct BallCollisionInfo
        {
            public float Distance;
            public BallPhysics HitBallPhysics;
        }
        
    }

}
