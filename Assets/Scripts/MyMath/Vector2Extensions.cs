using UnityEngine;

namespace DMath
{
    public static class Vector2Extensions
    {
        public static Vector2 Rotate(this Vector2 v2, float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            float cosAngle = Mathf.Cos(angleRadians);
            float sinAngle = Mathf.Sin(angleRadians);
        
            float newX = v2.x * cosAngle - v2.y * sinAngle;
            float newY = v2.x * sinAngle + v2.y * cosAngle;

            v2.x = newX;
            v2.y = newY;
            return v2;
        }
    }
}