using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DMath
{
    public struct Line
    {
        public Vector2 P1, P2;
        
        public Line(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;        
        }

        public Vector2 GetDirectionP1ToP2() => P2 - P1;

        public static bool TryGetIntersection(Line l1, Line l2, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            // Вычисление направляющих векторов для линий
            Vector2 r = l1.GetDirectionP1ToP2();
            Vector2 s = l2.GetDirectionP1ToP2();

            float denominator = r.x * s.y - r.y * s.x;

            if (Mathf.Approximately(denominator, 0)) return false;

            Vector2 p3ToP1 = l2.P1 - l1.P1;
            float t = (p3ToP1.x * s.y - p3ToP1.y * s.x) / denominator;
            float u = (p3ToP1.x * r.y - p3ToP1.y * r.x) / denominator;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                intersection = l1.P1 + t * r;
                return true;
            }

            return false;
        }

        public void DebugDraw(Color color, float duration)
        {
            Debug.DrawLine(P1, P2, color, duration);
        }
    }
}

