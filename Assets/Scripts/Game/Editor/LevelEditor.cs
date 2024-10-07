using System;
using System.Collections.Generic;
using Ball.State;
using Game.Level;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class LevelEditor : EditorWindow
    {
        public bool isEditMode;
        [SerializeField] private BallStateMachine _ballPrefab;
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private BallType _ballType;
        [SerializeField] private string _levelName;
        [SerializeField] private BallType[] _playerBallTypes;

        private SerializedObject _serializedObject;
        private SerializedProperty _ballPrefabProp;
        private SerializedProperty _gameManagerProp;
        private SerializedProperty _ballTypeProp;
        private SerializedProperty _levelNameProp;
        private SerializedProperty _playerBallTypesProp;

        private Vector2 _scrollPos;
        private string _originalLevelName;

        [MenuItem("Tools/LevelEditor")]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelEditor>();
            window._gameManager = FindAnyObjectByType<GameManager>();
        }

        private void OnEnable()
        {
            SetEditMode(false);
            _serializedObject = new SerializedObject(this);
            _ballPrefabProp = _serializedObject.FindProperty("_ballPrefab");
            _gameManagerProp = _serializedObject.FindProperty("_gameManager");
            _ballTypeProp = _serializedObject.FindProperty("_ballType");
            _levelNameProp = _serializedObject.FindProperty("_levelName");
            _playerBallTypesProp = _serializedObject.FindProperty("_playerBallTypes");

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            _gameManager = FindAnyObjectByType<GameManager>();
            if (_gameManager != null)
            {
                _gameManager.ResetField();
                var balls = FindObjectsByType<BallStateMachine>(FindObjectsSortMode.None);
                foreach (var ballSM in balls)
                {
                    ballSM.Init();
                    ballSM.Context.TargetPosition = ballSM.transform.position;
                    ballSM.SetState(BallStateEnum.Static);
                }
            }
            
        }

        private void OnDisable()
        {
            SetEditMode(false);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (isEditMode) SaveLevel();
                SetEditMode(false);
            }
        }
        private void OnGUI()
        {
            _serializedObject.Update();
            GUILayout.Label(isEditMode ? "Edit mode" : "Level Editor", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_ballPrefabProp, new GUIContent("Ball prefab"));
            EditorGUILayout.PropertyField(_gameManagerProp, new GUIContent("Game Manager"));
            if (isEditMode)
            {
                EditorGUILayout.PropertyField(_ballTypeProp, new GUIContent("Current Ball Color"));
                EditorGUILayout.PropertyField(_levelNameProp, new GUIContent("Level name"));

                _gameManager.playerBallsData.isRandom = EditorGUILayout.Toggle("Random Color", 
                                                        _gameManager.playerBallsData.isRandom);
                if (_gameManager.playerBallsData.isRandom)
                    _gameManager.playerBallsData.ballCount = EditorGUILayout.IntField("Number of Player Balls", 
                                                            _gameManager.playerBallsData.ballCount);
                else
                {
                    EditorGUILayout.PropertyField(_playerBallTypesProp, new GUIContent("Player Balls"));
                }
            }

            _serializedObject.ApplyModifiedProperties();
            
            if (_ballPrefab == null)
            {
                EditorGUILayout.HelpBox("Please assign a prefab to spawn.", MessageType.Warning);
            }

            if (_gameManager == null)
            {
                EditorGUILayout.HelpBox("No GameManager", MessageType.Error);
            }
            

            if (_gameManager != null)
            {
                EditorGUILayout.Space(10);
                if (!isEditMode)
                {
                    var levelFiles = LevelLoader.GetAllFileLevels();

                    if (GUILayout.Button("Create New Level"))
                    {
                        _levelName = $"Level_{levelFiles.Length + 1}";
                        LevelData data = new LevelData(_levelName);
                        LevelLoader.Save(data);
                        AssetDatabase.Refresh();
                    }

                    if (levelFiles.Length == 0)
                    {
                        EditorGUILayout.HelpBox("No levels", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("Level Files", EditorStyles.boldLabel);
                        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                        foreach (var levelFile in levelFiles)
                        {
                            if (GUILayout.Button(levelFile.name))
                            {
                                SetEditMode(true);
                                LoadLevel(LevelLoader.FileToLevel(levelFile));
                            }
                        }

                        EditorGUILayout.EndScrollView();
                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    var prevRadius = _gameManager.radius;
                    var prevOffset = _gameManager.offset;
                    _gameManager.radius = EditorGUILayout.Slider("radius", _gameManager.radius, 0.1f, 1f);
                    _gameManager.offset = EditorGUILayout.FloatField("offset", _gameManager.offset);

                    if (Math.Abs(prevRadius - _gameManager.radius) > 0.01f 
                        || Math.Abs(prevOffset - _gameManager.offset) > 0.01f)
                    {
                        _gameManager.UpdateField();
                        _gameManager.Field.UpdateBalls();
                    }
                    
                    if (GUILayout.Button("Save Level"))
                    {
                        SaveLevel();
                    }
                    
                    if (GUILayout.Button("Clear"))
                    {
                        ClearBalls();
                    }
                    
                    if (GUILayout.Button("Exit"))
                    {
                        ClearBalls();
                        SetEditMode(false);
                    }
                }
            }
        }
        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            
            if (e.type == EventType.MouseDown)
            {
                Vector2 mousePosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
                if (e.button == 0 && _ballPrefab != null)
                {
                    var field = _gameManager.Field;
                    var coord = field.GetValidCoordByGlobalPos(mousePosition);
                    var prevBall = field.GetBallByCoord(coord);
                    if (prevBall != null)
                    {
                        RemoveBall(prevBall, coord);
                    }

                    var globalPos = field.GetBallPositionByCoord(coord);

                    CreateBall(globalPos, _ballType);

                    Selection.activeObject = null;
                    e.Use(); 
                } else if (e.button == 1)
                {
                    var field = _gameManager.Field;
                    var coord = field.GetCoordByGlobalPos(mousePosition);
                    var ball = field.GetBallByCoord(coord);
                    if (ball != null)
                    {
                        RemoveBall(ball, coord);
                        e.Use(); 
                    }
                }
            }
        }

        private void SetEditMode(bool val)
        {
            isEditMode = val;
            if (isEditMode)
            {
                SceneView.duringSceneGui += OnSceneGUI;
            }
            else
            {
                SceneView.duringSceneGui -= OnSceneGUI;
            }
        }

        private void LoadLevel(LevelData level)
        {
            ClearBalls();
            _levelName = level.name;
            _gameManager.radius = level.radius;
            _gameManager.offset = level.offset;
            _gameManager.UpdateField();
            if (level.playerBallsData != null) _gameManager.playerBallsData = level.playerBallsData;
            else _gameManager.playerBallsData = new LevelData.PlayerBallsData(15);
            var field = _gameManager.Field;
            foreach (var ball in level.balls)
            {
                CreateBall(field.GetBallPositionByCoord(ball.pos), ball.type);
            }
        }

        private void RemoveBall(BallStateMachine ball, Vector2Int coord)
        {
            DestroyImmediate(ball.gameObject);
            _gameManager.Field.BallDict.Remove(coord);
        }
        
        private void CreateBall(Vector3 position, BallType type)
        {
            var ball = (BallStateMachine)PrefabUtility.InstantiatePrefab(_ballPrefab);
            var transform = ball.transform;
            transform.localScale = Vector3.one * (_gameManager.radius * 2);
            ball.Init();
            ball.SetType(type);
            ball.Context.TargetPosition = position;
            ball.SetState(BallStateEnum.Static);
            Undo.RegisterCreatedObjectUndo(ball.gameObject, "Place Ball");
        }

        private void ClearBalls()
        {
            var balls = FindObjectsByType<BallStateMachine>(FindObjectsSortMode.None);
                
            foreach (var ball in balls)
            {
                DestroyImmediate(ball.gameObject);
            }
        }

        private void SaveLevel()
        {
            if (_gameManager == null)
            {
                Debug.LogError("GameManager is null. Failed to save");
                return;
            }
            var balls = FindObjectsByType<BallStateMachine>(FindObjectsSortMode.None);

            if (!_gameManager.playerBallsData.isRandom)
            {
                _gameManager.playerBallsData = new LevelData.PlayerBallsData(_playerBallTypes);
            }
            
            LevelLoader.Save(_levelName, balls, _gameManager);
            AssetDatabase.Refresh();
        }
        
    }
    
    

}
