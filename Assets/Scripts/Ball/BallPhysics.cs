using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BallPhysics : MonoBehaviour
{
    [SerializeField] private float xLimit;
    [SerializeField] private Vector2 gravity = new (0, -9.8f);
    private Vector2 _currentVelocity;
    private float _radius = 0.5f;

    public bool collisionSimulation = true;

    private void OnEnable()
    {
        Init();
    } 
    
    public void Init()
    {
        _radius = transform.lossyScale.x / 2;
    }

    private void FixedUpdate()
    {
        if (collisionSimulation) SimulateCollision();
        
        Vector2 position = transform.position;
        
        _currentVelocity += gravity * Time.fixedDeltaTime;

        if (position.x + _radius > xLimit || position.x - _radius < -xLimit) _currentVelocity.x *= -1;

        
        position += _currentVelocity * Time.fixedDeltaTime;
        transform.position = position;
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (!enabled) return;
        _currentVelocity = velocity;
    }

    public void AddVelocity(Vector2 velocity)
    {
        if (!enabled) return;
        _currentVelocity += velocity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(xLimit, -10), new Vector3(xLimit, 10));
        Gizmos.DrawLine(new Vector3(-xLimit, -10), new Vector3(-xLimit, 10));
    }

    public List<Vector2> PredictTrajectory(Vector2 velocity)
    {
        List<Vector2> points = new List<Vector2>();
        float time = 0.5f;
        float deltaTime = 0.05f;
        Vector2 position = transform.position;

        points.Add(position);
        for (float i = 0; i < time; i += deltaTime)
        {
            velocity += gravity * deltaTime;
            
            if (position.x + _radius > xLimit || position.x - _radius < -xLimit) velocity.x *= -1;

            position += velocity * deltaTime;

            points.Add(position);
        }

        return points;
    }

    public List<BallCollisionInfo> CheckCollisions(Func<BallStateMachine, bool> condition = null)
    {
        condition ??= _ => true;
        var balls = GameField.I.BallSet;
        List<BallCollisionInfo> collisions = new List<BallCollisionInfo>();

        foreach (var ball in balls)
        {
            if (ball.Physics == this) continue;
            if (!condition.Invoke(ball)) continue;

            if (!AreCirclesColliding(this, ball.Physics, out var distance)) continue;

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
            ballPhysics.AddVelocity(impulseVector);
        }
    }

    private static bool AreCirclesColliding(BallPhysics a, BallPhysics b, out float distance)
    {
        var minDist = a._radius + b._radius;
        distance = Vector2.Distance(a.transform.position, b.transform.position);
        return minDist > distance;
    }
    
    public struct BallCollisionInfo
    {
        public float Distance;
        public BallPhysics HitBallPhysics;
    }
    
}
