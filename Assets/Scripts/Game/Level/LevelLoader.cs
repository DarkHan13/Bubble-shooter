using System;
using System.IO;
using Ball.State;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Game.Level
{
    public static class LevelLoader
    {
        private const string folderName = "Levels";

        public static void Save(string name, BallStateMachine[] balls, GameManager gameManager)
        {
            LevelData.BallInfo[] ballInfoArray = new LevelData.BallInfo[balls.Length];
                
            for (int i = 0; i < balls.Length; i++)
            {
                var coord = gameManager.Field.GetCoordByGlobalPos(balls[i].transform.position);
                ballInfoArray[i] = new LevelData.BallInfo(coord, balls[i].Type);
            }

            LevelData levelData = new LevelData(name, gameManager.radius, gameManager.offset, 
                gameManager.playerSpawnPos, gameManager.playerBallsData, ballInfoArray);
            Save(levelData);
        }
        
        public static void Save(LevelData levelData)
        {
            string jsonData = JsonUtility.ToJson(levelData, false);

            string filePath = Path.Combine(Application.dataPath, "Resources/Levels", $"{levelData.name}.json");
            
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"Save: {levelData.balls.Length} balls {levelData.playerBallsData.ballCount}");
            Debug.Log($"Level saved to: {filePath}");
        }

        public static LevelData GetRandomLevel()
        {
            var files = GetAllFileLevels();
            if (files.Length == 0)
            {
                Debug.LogError($"There are no files in Resources/{folderName}");
                return null;
            }
            var file = files[Random.Range(0, files.Length)];
            return FileToLevel(file);
        }

        public static LevelData FileToLevel(TextAsset file)
        {
            LevelData levelData = JsonUtility.FromJson<LevelData>(file.text);
            levelData.name = file.name;
            Debug.Log($"{levelData.offset} {levelData.radius} {levelData.balls.Length}");
            return levelData;
        }
        public static TextAsset[] GetAllFileLevels()
        {
            TextAsset[] files = Resources.LoadAll<TextAsset>(folderName);
            return files;
        }
    }

    [Serializable]
    public class LevelData
    {
        public string name;
        public float radius;
        public float offset;
        public Vector2 playerSpawnPos = new (0, -4f);

        public BallInfo[] balls;
        public PlayerBallsData playerBallsData;

        public LevelData(string name)
        {
            this.name = name;
            radius = 0.2f;
            offset = 0.05f;
            balls = new BallInfo[] {};
            playerBallsData = new PlayerBallsData(15);
        }
        

        public LevelData(string name, float radius, float offset, Vector2 playerSpawnPos, PlayerBallsData playerBallsData, BallInfo[] balls)
        {
            this.name = name;
            this.radius = radius;
            this.offset = offset;
            this.playerSpawnPos = playerSpawnPos;
            this.playerBallsData = playerBallsData;
            this.balls = balls;
        }
        
        [Serializable]
        public class BallInfo
        {
            public Vector2Int pos;
            public BallType type;

            public BallInfo(Vector2Int pos, BallType type)
            {
                this.pos = pos;
                this.type = type;
            }
        }
        
        [Serializable]
        public class PlayerBallsData
        {
            public BallType[] balls;
            public bool isRandom;
            public int ballCount = 1;

            public PlayerBallsData(BallType[] balls)
            {
                this.balls = balls;
                isRandom = false;
            }

            public PlayerBallsData(int ballCount)
            {
                this.ballCount = ballCount;
                isRandom = true;
            }
        }

    }
}


