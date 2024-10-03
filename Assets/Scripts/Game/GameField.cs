using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameField
{
    public static GameField I;
    public readonly Dictionary<Vector2, BallStateMachine> BallDict;
    public readonly HashSet<BallStateMachine> BallSet;
    
    public float limitX, upperY, radius = 0.5f, offset = 0.1f;

    public int Columns
    {
        get
        {
            float diameter = (radius) * 2f + offset;
            return (int)Math.Round(limitX * 2 / diameter);
        }
    }

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
    

    public GameField(float limitX, float upperY, float radius, float offset)
    {
        I = this;
        BallSet = new HashSet<BallStateMachine>();
        BallDict = new Dictionary<Vector2, BallStateMachine>();
        
        this.limitX = limitX;
        this.upperY = upperY;
        this.radius = radius;
        this.offset = offset;
    }

    public List<Vector2> GenerateField()
    {
        if (radius == 0) return null;
        

        List<Vector2> positions = new ();
        float diameter = radius * 2f;

        bool isEvenRow = true;
        for (float y = upperY; y >= 0; y -= diameter + offset)
        {
            isEvenRow = !isEvenRow;
            for (float x = -limitX; x <= limitX - radius; x += diameter + offset)
            {
                var xOffset = isEvenRow ? radius : 0;
                positions.Add(new Vector3(x + xOffset, y, 0));
            }
        }
        
        return positions;
    }

    public BallStateMachine GetBallByCoord(Vector2 coord)
    {
        if (BallDict.TryGetValue(coord, out var result)) return result;
        return null;
    }
    
    public Vector2Int GetCoordByGlobalPos(Vector2 globalPos)
    {
        var upperLeftCorner = new Vector2(-limitX, upperY);
        var relativePos = globalPos - upperLeftCorner;
        float diameterOffset = radius * 2f + offset;
        int y = Mathf.RoundToInt(relativePos.y / diameterOffset);
        bool isEvenRow = y % 2 == 0;
        float xOffset = !isEvenRow ? radius : 0;
        var t = relativePos.x / diameterOffset;
        int x = isEvenRow ? Mathf.RoundToInt(t) : Mathf.FloorToInt(t);
        Debug.Log($"{x} {t}");
        return new Vector2Int(x, y); 
    }
    public bool GetBallPositionByGlobalPos(Vector2 globalPos, out Vector2 resultPos)
    {
        var i = GetCoordByGlobalPos(globalPos);

        return GetBallPositionByCoord(i, out resultPos);

    }

    public bool GetBallPositionByCoord(Vector2 coord, out Vector2 resultPos)
    {
        var upperLeftCorner = new Vector2(-limitX, upperY);
        float diameterOffset = radius * 2f + offset;
        bool isEvenRow = coord.y % 2 == 0;
        float xOffset = !isEvenRow ? radius : 0;
        resultPos = upperLeftCorner + coord * diameterOffset + new Vector2(xOffset, 0);

        if (!IsValidCoord(coord)) return false;

        return true;
    }

    private bool IsValidCoord(Vector2 coord) => IsValidCoord(Vector2Int.RoundToInt(coord));
    private bool IsValidCoord(Vector2Int coord) => !(coord.y > 0 || coord.x < 0 || coord.x >= Columns);

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
        
        Debug.Log($"{targetBalls.Count} {targetType.ToString()}");
        if (targetBalls.Count < 3) return;
        
        foreach (var targetBall in targetBalls)
        {
            targetBall.SetState(BallStateMachine.BallStateEnum.Popped);
        }

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
}
