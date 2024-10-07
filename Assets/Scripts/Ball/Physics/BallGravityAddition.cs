using UnityEngine;

namespace Ball.Physics
{
    public class BallGravityAddition : BallPhysicsAddition
    {
        private readonly Vector2 _gravity = new Vector2(0, -9.8f);
        
        public BallGravityAddition(BallPhysics physics) : base(physics)
        {
        }
        
        public override void Simulate(float deltaTime, ref Vector2 position, ref Vector2 velocity)
        {
            velocity += _gravity * deltaTime;
        }

    }
}