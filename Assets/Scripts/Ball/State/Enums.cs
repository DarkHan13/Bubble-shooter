using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ball.State
{
    public enum BallStateEnum
    {
        Waiting,
        Moving,
        PlayerBall,
        Flying,
        Static,
        Falling,
        Popped,
    }
        
    [Serializable]
    public enum BallType
    {
        None = 0,
        Red = 1,
        Yellow = 2,
        Blue = 3,
        Purple = 4,
        Green = 5
    }
    
    public static class BallTypeExtensions
    {
        private static Dictionary<Color, BallType> ColorBallDict = new ()
        {
            {Color.gray, BallType.None},
            {Color.red, BallType.Red},
            {Color.yellow, BallType.Yellow},
            {Color.blue, BallType.Blue},
            {new Color(139 / 255f, 0, 255), BallType.Purple},
            {Color.green, BallType.Green},
        };

        public static BallType GetBallType(this Color color)
        {
            if (ColorBallDict.TryGetValue(color, out var type)) return type;
            return BallType.None;
        }

        public static Color GetColor(this BallType type)
        {
            foreach (var (key, value) in ColorBallDict)
            {
                if (type == value) return key;
            }
            return Color.gray;
        }
    }

}