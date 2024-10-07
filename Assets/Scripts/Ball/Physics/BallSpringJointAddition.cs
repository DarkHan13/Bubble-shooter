using UnityEngine;

namespace Ball.Physics
{
    public class BallSpringJointAddition : BallPhysicsAddition
    {
        public Vector2 Joint;
        public float SpringStrength, Damping; 

        
        public BallSpringJointAddition(BallPhysics physics) : base(physics)
        {
        }

        public BallSpringJointAddition(BallPhysics physics, Vector2 joint, float springStrength, float damping) : base(physics)
        {
            Joint = joint;
            SpringStrength = springStrength;
            Damping = damping;
        }
        public override void Simulate(float deltaTime, ref Vector2 position, ref Vector2 velocity)
        {
            var toJointDir = Joint - position;
            var a = toJointDir * (SpringStrength * deltaTime);
            velocity += a;
            velocity *= Damping;
        }
    }
}