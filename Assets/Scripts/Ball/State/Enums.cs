using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ball.State
{
    public enum BallStateEnum
    {
        /// <summary>
        /// Waiting ball
        /// </summary>
        Waiting,
        
        /// <summary>
        /// The ball moves before changing its state
        /// Context Parameters:
        /// <param name="TargetPosition">TargetPosition</param>
        /// <param name="Duration">floatArgs[0]: move duration</param>
        /// <param name="NextState">NextState: State after the end of the movement</param>
        /// </summary>
        Moving,
        
        /// <summary>
        /// The ball is under player control.
        /// Context Parameters:
        /// <param name="minSpeed">floatArgs[0]: Minimum ball speed</param>
        /// <param name="maxSpeed">floatArgs[1]: Maximum ball speed</param>
        /// <param name="draggingRange">vector2Args[0]: Allowed range for dragging the ball</param>
        /// </summary>
        PlayerBall,
        
        /// <summary>
        /// The flying ball sticks to stationary balls or an upper boundary.
        /// Context Parameters:
        /// <param name="Velocity">Velocity</param>
        /// </summary>
        Flying,
        
        /// <summary>
        /// The flying ball replaces a stationary ball or sticks to an upper boundary.
        /// Context Parameters:
        /// <param name="Velocity">Velocity</param>
        /// </summary>
        ChargedFlying,
        
        /// <summary>
        /// The Ball fixed to a single point.
        /// Context Parameters:
        /// <param name="springJointPosition">TargetPosition: The position at which the ball is fixed with a spring joint</param>
        /// </summary>
        Static,
        
        /// <summary>
        /// The falling ball is destroyed below a certain height.
        /// </summary>
        Falling,
        
        /// <summary>
        /// The ball pops
        /// </summary>
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

    public static class BallStateExtensions
    {
        public static BallState CreateState(this BallStateEnum stateEnum, BallStateMachine machine)
        {
            BallState state = null;
            var context = machine.Context;
            switch (stateEnum)
            {
                case BallStateEnum.Waiting:
                    state = new BallWaitingState(machine);
                    break;
                case BallStateEnum.Moving:
                    if (context.FloatArgs.Length < 1) context.FloatArgs = new[] { 0.5f };
                    state = new BallMovingState(machine);
                    break;
                case BallStateEnum.PlayerBall:
                    if (context.FloatArgs.Length < 2) context.FloatArgs = new[] { 10f, 20f };
                    if (context.Vector2Args.Length < 1) context.Vector2Args = new[] { new Vector2(0, 1) };
                    state = new BallController(machine);
                    break;
                case BallStateEnum.Flying:
                    state = new BallFlyingState(machine);
                    break;
                case BallStateEnum.ChargedFlying:
                    state = new BallChargedFlyingState(machine);
                    break;
                case BallStateEnum.Static:
                    state = new BallStationaryState(machine);
                    break;
                case BallStateEnum.Falling:
                    state = new BallFallingState(machine);
                    break;
                case BallStateEnum.Popped:
                    state = new BallPoppedState(machine);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stateEnum), stateEnum, null);
            }

            return state;
        }
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