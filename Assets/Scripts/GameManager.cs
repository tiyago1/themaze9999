using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Maze.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Random = UnityEngine.Random;

namespace Maze
{
    public class GameManager : MonoBehaviour, IInitializable
    {
        [SerializeField] private TextMeshProUGUI mazeTimer;
        [SerializeField] private List<ThemeData> themesData;
        [SerializeField] private Transform inputWaitTextObject;
        
        public SoundManager SoundManager;
        public int Diff;
        public int PrevLevel
        {
            get { return PlayerPrefs.GetInt("PrevLevel", 0); }
            set { PlayerPrefs.SetInt("PrevLevel", value); }
        }

        public int Level
        {
            get { return PlayerPrefs.GetInt("Level", 0); }
            set { PlayerPrefs.SetInt("Level", value); }
        }

        public int LastThemeChangedLevel
        {
            get { return PlayerPrefs.GetInt("LastThemeChangedLevel", 0); }
            set { PlayerPrefs.SetInt("LastThemeChangedLevel", value); }
        }

        public int ThemeIndex
        {
            get { return PlayerPrefs.GetInt("ThemeIndex", 0); }
            set { PlayerPrefs.SetInt("ThemeIndex", value); }
        }

        public int MazeSize
        {
            get { return PlayerPrefs.GetInt("MazeSize", 4); }
            set { PlayerPrefs.SetInt("MazeSize", value); }
        }

        public bool IsThemesEnded
        {
            get { return PlayerPrefs.GetInt("IsThemesEnded", 0) == 1; }
            set { PlayerPrefs.SetInt("IsThemesEnded", value ? 1 : 0); }
        }
        
        public bool LastLevelCompleted
        {
            get { return PlayerPrefs.GetInt("LastLevelCompleted", 0) == 1; }
            set { PlayerPrefs.SetInt("LastLevelCompleted", value ? 1 : 0); }
        }

        [Inject] private MazeGenerator _mazeGenerator;
        [Inject] private HealthController _healthController;
        [Inject] private PlayerController _playerController;
        [Inject] private GameResultPanel _gameResultPanel;
        [Inject] private SignalBus _signalBus;

        public bool IsInitialized;
        public bool IsGameOver;

        private Coroutine _mazeCoroutine;
        public Camera camera;
        public ThemeData CurrentThemeData;
        private bool _isFirstKeyDetected;

        private void Awake()
        {
            Debug.LogError("Awake");
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.DeleteAll();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            PlayerPrefs.DeleteAll();
        }

        private void InitializeLevelData()
        {
            int diff = Level - LastThemeChangedLevel;
            if (diff == Diff || diff == 0)
            {
                if (LastLevelCompleted && diff == Diff)
                {
                    ThemeIndex++;
                    MazeSize += 2;
                }

                LastThemeChangedLevel = Level;
                if (IsThemesEnded)
                {
                    GetRandomTheme();
                    return;
                }

                if (ThemeIndex >= themesData.Count)
                {
                    IsThemesEnded = true;
                    GetRandomTheme();
                }
                else
                {
                    CurrentThemeData = themesData[ThemeIndex];
                }
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Retry();
            }
            if (_isFirstKeyDetected)
            {
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isFirstKeyDetected = true;
                _playerController.TurnChange();
                _playerController.DisableAllIndicators();
                StartMazeTimer();
                inputWaitTextObject.DOScale(Vector3.zero, .5f).SetEase(Ease.OutBack);
            }
        }

        private void GetRandomTheme()
        {
            int randomThemeIndex = Random.Range(0, themesData.Count);

            ThemeIndex = randomThemeIndex;
            CurrentThemeData = themesData[ThemeIndex];
        }

        private void Start()
        {
            Debug.LogError("Start");

            if (IsInitialized)
            {
                _mazeGenerator.Initialize(CurrentThemeData.Maze, MazeSize);
            }
        }

        private void SetUpCamera(int gridSize)
        {
            float standardZ = -9;
            float standardY = 0.5f;

            float changeZ = -2.2f;
            float changeY = 0.12f;

            float multiplyRate = (gridSize - 8) / 2;

            float newCameraZ = standardZ + changeZ * multiplyRate;
            float newCameraY = standardY + changeY * multiplyRate;

            camera.transform.position = new Vector3(0, newCameraY, newCameraZ);
        }

        public void Initialize()
        {
            IsInitialized = true;
            IsGameOver = false;
            InitializeLevelData();
            _playerController.p1.SetColor(CurrentThemeData.P1);
            _playerController.p2.SetColor(CurrentThemeData.P2);
            _signalBus.Subscribe<MazeGenerateFinished>(OnMazeGenerateFinished);
            _signalBus.Subscribe<GameOver>(StopTimer);
            Debug.LogError("Init");
        }

        private void StopTimer()
        {
            mazeTimer.transform.DOKill();
            StopCoroutine(_mazeCoroutine);
        }

        private void OnMazeGenerateFinished(MazeGenerateFinished obj)
        {
            SetUpCamera(_mazeGenerator.mazeRows);
            _healthController.Initialize();
            _playerController.Initialize();
            // StartMazeTimer();
            SoundManager.PlayIngameMusic(true);
            inputWaitTextObject.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
        }

        public void CheckGameEnd(MazeGenerator.Cell checkCell)
        {
            if (checkCell.gridPos == _mazeGenerator.EndCell.gridPos)
            {
                mazeTimer.transform.DOKill();
                StopCoroutine(_mazeCoroutine);
                _gameResultPanel.Show(true);
            }
        }

        public void StartMazeTimer()
        {
            _mazeCoroutine = StartCoroutine(Timer());
        }

        private IEnumerator Timer()
        {
            int duration = _mazeGenerator.mazeRows * 6 + 10;
            mazeTimer.text = duration.ToString();
            for (int i = duration; i > 0; i--)
            {
                yield return new WaitForSecondsRealtime(1);

                mazeTimer.transform.DOShakeRotation(.5f, 30, 10, 35).SetEase(Ease.Linear).OnStart(() => { mazeTimer.text = i.ToString(); });
            }

            _gameResultPanel.Show(false);
        }

        public void Retry()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            // LastLevelCompleted = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void NextLevel()
        {
            PrevLevel = Level;
            Level++;
            LastLevelCompleted = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}