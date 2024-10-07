using UnityEngine;

namespace Ball.Physics
{
    public abstract class BallPhysicsAddition
    {
        public BallPhysics Physics;

        protected BallPhysicsAddition(BallPhysics physics)
        {
            Physics = physics;
        }

        public abstract void Simulate(float deltaTime, ref Vector2 position, ref Vector2 velocity);
    }
}