using System;
using System.Collections.Generic;
using Ball.State;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
    public class GameField
    {
        public static GameField I;
        public readonly Dictionary<Vector2Int, BallStateMachine> BallDict;
        public readonly HashSet<BallStateMachine> BallSet;
    
        public float limitX, upperY, lowerY, radius = 0.5f, offset = 0.1f;

        public int EvenColumns
        {
            get
            {
                // float diameter = (radius) * 2f + offset;
                // return (int)Math.Ceiling((limitX * 2 - diameter) / diameter);
                
                int n = 0;
                float diameterOffset = radius * 2f + offset;
                for (float x = -limitX + radius; x <= limitX - radius; x += diameterOffset)
                {
                    n++;
                }
            
                return n;
            }
        }
    
        public int OddColumns
        {
            get
            {
                int n = 0;
                float diameterOffset = radius * 2f + offset;
                for (float x = -limitX + radius; x <= limitX - radius; x += diameterOffset)
                {
                    if (x + radius > limitX - radius) continue;
                    n++;
                }
            
                return n;
            }
        }

        public int CachedEvenColumns {get; private set;}
        public int CachedOddColumns {get; private set;}

        private static Vector2Int[] NeighboursPositionsOddRow =
        {
            new (-1, 0), new (0, 1), new (1, 1),
            new (1, 0), new (1, -1), new (0, -1) 
        };
    
        private static Vector2Int[] NeighboursPositionsEvenRow =
        {
            new (-1, 0), new (-1, 1), new (0, 1),
            new (1, 0), new (0, -1), new (-1, -1) 
        };
    

        public GameField(float limitX, float upperY, float lowerY, float radius, float offset)
        {
            I = this;
            BallSet = new HashSet<BallStateMachine>();
            BallDict = new Dictionary<Vector2Int, BallStateMachine>();
        
            UpdateData(limitX, upperY, lowerY, radius, offset);
        }

        public void UpdateData(float limitX, float upperY, float lowerY, float radius, float offset)
        {
            this.limitX = limitX;
            this.upperY = upperY;
            this.lowerY = lowerY;
            this.radius = radius;
            this.offset = offset;

            RecalculateColumns();
        }

        private void RecalculateColumns()
        {
            CachedEvenColumns = EvenColumns;
            CachedOddColumns = OddColumns;
        }

        public List<Vector2> GenerateField()
        {
            if (radius == 0) return null;
        

            List<Vector2> positions = new ();
            float diameter = radius * 2f;
            float diameterOffset = diameter + offset;
            // first row is 0, so it's even
            bool isEvenRow = false;
            for (float y = upperY - radius; y >= radius; y -= diameterOffset)
            {
                isEvenRow = !isEvenRow;
                for (float x = -limitX + radius; x <= limitX - radius; x += diameterOffset)
                {
                    var xOffset = isEvenRow ? 0 : radius;
                    if (!isEvenRow && x + xOffset > limitX - radius) continue;
                
                    positions.Add(new Vector3(x + xOffset, y, 0));
                }
            }
        
            return positions;
        }

        public BallStateMachine GetBallByCoord(Vector2Int coord)
        {
            if (BallDict.TryGetValue(coord, out var result)) return result;
            return null;
        }
        public bool GetBallPositionByGlobalPos(Vector2 globalPos, out Vector2 resultPos)
        {
            var i = GetCoordByGlobalPos(globalPos);

            return TryGetBallPositionByCoord(i, out resultPos);

        }
    
        public Vector2Int GetCoordByGlobalPos(Vector2 globalPos)
        {
            var upperLeftCorner = new Vector2(-limitX + radius, upperY - radius);
            var relativePos = globalPos - upperLeftCorner;
            float diameterOffset = radius * 2f + offset;
            int y = Mathf.RoundToInt(relativePos.y / diameterOffset);
            if (y % 2 != 0) relativePos.x -= radius;
            var t = relativePos.x / diameterOffset;
            int x =  Mathf.RoundToInt(t);
            return new Vector2Int(x, y); 
        }

        public Vector2Int GetValidCoordByGlobalPos(Vector2 globalPos)
        {
            var currentCoord = GetCoordByGlobalPos(globalPos);
            if (IsValidCoord(currentCoord)) return currentCoord;
            var columns = currentCoord.y % 2 == 0 ? CachedEvenColumns : CachedOddColumns;
            if (currentCoord.x < 0) currentCoord.x = 0;
            else if (currentCoord.x >= columns) currentCoord.x = columns - 1;
            
            if (currentCoord.y > 0) currentCoord.y = 0;
            return currentCoord;
        }

        public Vector2 GetBallPositionByCoord(Vector2 coord)
        {
            TryGetBallPositionByCoord(coord, out var resultPos);
            return resultPos;
        }
        public bool TryGetBallPositionByCoord(Vector2 coord, out Vector2 resultPos)
        {
            var upperLeftCorner = new Vector2(-limitX + radius, upperY - radius);
            float diameterOffset = radius * 2f + offset;
            bool isEvenRow = coord.y % 2 == 0;
            float xOffset = !isEvenRow ? radius : 0;
            resultPos = upperLeftCorner + coord * diameterOffset + new Vector2(xOffset, 0);

            if (!IsValidCoord(coord)) return false;

            return true;
        }

        private bool IsValidCoord(Vector2 coord) => IsValidCoord(Vector2Int.RoundToInt(coord));
        private bool IsValidCoord(Vector2Int coord)
        {
            var columns = coord.y % 2 == 0 ? CachedEvenColumns : CachedOddColumns;
            return !(coord.y > 0 || coord.x < 0 || coord.x >= columns);
        }

        public void TriggerChainReactionFor(Vector2Int coord)
        {
            var ball = GetBallByCoord(coord);
            if (ball == null) return;

            var targetType = ball.Type;

            HashSet<Vector2Int> visited = new ();
            HashSet<BallStateMachine> targetBalls = new ();
            Queue<Vector2Int> queue = new();
            queue.Enqueue(coord);

            while (queue.Count > 0)
            {
                coord = queue.Dequeue();
                visited.Add(coord);
                ball = GetBallByCoord(coord);
                if (ball.Type == targetType) targetBalls.Add(ball);
                var neighbours = GetNeighbours(coord, delegate(Vector2Int nCoord)
                {
                    if (visited.Contains(nCoord)) return false;
                    var b = GetBallByCoord(nCoord);
                    if (b == null) return false;
                    return b.Type == targetType;
                });
                foreach (var neighbour in neighbours)
                {
                    queue.Enqueue(neighbour);
                }
            }
        
            if (targetBalls.Count < 3) return;

            foreach (var targetBall in targetBalls)
            {
                targetBall.SetState(BallStateEnum.Popped);
            }

            for (int x = 0; x < CachedEvenColumns; x++)
            {
                var pos = new Vector2Int(x, 0);
                if (BallDict.ContainsKey(pos)) queue.Enqueue(pos);
            }

            var row = GetFirstRow();
            if (row.Count < CachedEvenColumns * 0.3f)
            {
                foreach (var ballKp in BallDict)
                {
                    if (ballKp.Value.State is not BallStationaryState stationaryState) continue;
                    stationaryState.IsFixed = false;
                }
            }
            else
            {
                UpdateStaticBalls();
            }
            
            TriggerFalling();
        }

        public void UpdateStaticBalls()
        {
            Queue<Vector2Int> queue = new();
            HashSet<Vector2Int> visited = new();

            foreach (var pos in GetFirstRow())
            {
                queue.Enqueue(pos);
            }

            while (queue.Count > 0)
            {
                var coord = queue.Dequeue();
                if (visited.Contains(coord)) continue;
                visited.Add(coord);
                var neighbours = GetNeighbours(coord, delegate(Vector2Int nCoord)
                {
                    if (visited.Contains(nCoord)) return false;
                    return GetBallByCoord(nCoord) != null;
                });
                
                foreach (var nCoord in neighbours)
                {
                    queue.Enqueue(nCoord);
                }
            }

            foreach (var (coord, ball) in BallDict)
            {
                if (ball.State is not BallStationaryState stationaryBall) continue;
                stationaryBall.IsFixed = visited.Contains(coord);
            }
        }

        public void TriggerFalling()
        {
            List<BallStateMachine> fallingBalls = new();
            foreach (var (coord, ball) in BallDict)
            {
                if (ball.State is not BallStationaryState stationaryBall) continue;
                if (!stationaryBall.IsFixed) fallingBalls.Add(ball);
            }
            foreach (var ball in fallingBalls)
            {
                ball.SetState(BallStateEnum.Falling);
            }
        }

        private List<Vector2Int> GetFirstRow()
        {
            RecalculateColumns();
            List<Vector2Int> list = new();
            for (int x = 0; x < CachedEvenColumns; x++)
            {
                var pos = new Vector2Int(x, 0);
                if (BallDict.ContainsKey(pos)) list.Add(pos);
            }

            return list;
        }
        public List<Vector2Int> GetNeighbours(Vector2Int coord, Func<Vector2Int, bool> filter = null)
        {
            filter ??= _ => true;
            List<Vector2Int> neighbours = new(6);
            var neighbourPattern = coord.y % 2 == 0 ? NeighboursPositionsEvenRow : NeighboursPositionsOddRow;
            foreach (var relativePos in neighbourPattern)
            {
                var pos = relativePos + coord;
                if (IsValidCoord(pos) && filter.Invoke(pos)) neighbours.Add(pos);
            }

            return neighbours;
        }

        public void OnBallDestroy(BallStateMachine ball)
        {
            BallSet.Remove(ball);
        }
        public static void Destroy()
        {
            I = null;
        }

        public void UpdateBalls()
        {
            List<Vector2Int> ballsToDestroy = new();
            List<BallStateMachine> modifiedBalls = new();
            Debug.Log(radius);
            foreach (var (coord, ball) in BallDict)
            {
                if (IsValidCoord(coord) && ball != null)
                {
                    ball.transform.localScale = Vector3.one * (radius * 2);
                    ball.Context.TargetPosition = GetBallPositionByCoord(coord);
                    modifiedBalls.Add(ball);
                    continue;
                }
                ballsToDestroy.Add(coord);
            }
            foreach (var coord in ballsToDestroy)
            {
                var ball = BallDict[coord];
                BallDict.Remove(coord);
                if (ball != null) Object.DestroyImmediate(ball.gameObject);
            }
            BallDict.Clear();

            foreach (var ball in modifiedBalls)
            {
                ball.SetState(BallStateEnum.Static);
            }
        }
    }
}
