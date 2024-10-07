using DMath;
using UnityEngine;

namespace Ball.Physics
{
    public class BallXLimitAddition : BallPhysicsAddition
    {
        public float xLimit;
        
        public BallXLimitAddition(BallPhysics physics) : base(physics)
        {
        }

        public BallXLimitAddition(BallPhysics physics, float xLimit) : base(physics)
        {
            this.xLimit = xLimit;
        }

        public override void Simulate(float deltaTime, ref Vector2 position, ref Vector2 velocity)
        {
            var r = Physics.Radius;
            var predictPosition = position + velocity * Time.fixedDeltaTime;
            if (predictPosition.x + r > xLimit || predictPosition.x - r < -xLimit)
            {
                var xLine = predictPosition.x + r > xLimit ? xLimit - r : -xLimit + r;
                Line wallLineWithOffset = new Line(new Vector2(xLine, -100), new Vector2(xLine, 100));
                position = Physics.GetIntersectionWithLine(predictPosition, velocity, wallLineWithOffset);
                velocity.x *= -1;
            }
        }
    }
}