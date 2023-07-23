using System.Collections;
using DG.Tweening;
using Maze.UI;
using TMPro;
using UnityEngine;
using Zenject;

namespace Maze
{
    public class GameManager : MonoBehaviour, IInitializable
    {
        [SerializeField] private TextMeshProUGUI mazeTimer;

        [Inject] private MazeGenerator _mazeGenerator;
        [Inject] private HealthController _healthController;
        [Inject] private PlayerController _playerController;
        [Inject] private GameResultPanel _gameResultPanel;
        [Inject] private SignalBus _signalBus;

        public bool IsInitialized;
        public bool IsGameOver;

        private Coroutine _mazeCoroutine;
        public Camera camera;

        private void Awake()
        {
            Debug.LogError("Awake");
        }

        private void Start()
        {
            Debug.LogError("Start");
            if (IsInitialized)
            {
                _mazeGenerator.Initialize();
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
            Debug.LogError("Init");
            _signalBus.Subscribe<MazeGenerateFinished>(OnMazeGenerateFinished);
            _signalBus.Subscribe<GameOver>(StopTimer);
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
            StartMazeTimer();
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
    }
}