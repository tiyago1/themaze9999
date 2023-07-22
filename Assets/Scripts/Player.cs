using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Maze.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Maze
{
    public class Player : MonoBehaviour
    {
        [Inject] private PlayerController _playerController;
        [Inject] private GameOverPanel _gameOverPanel;
        [Inject] private SignalBus _signalBus;

        public List<Sprite> ArrowSprites;

        public int Index;
        public bool IsTurn;
        public float TimerDuration;
        public KeyCode Up;
        public KeyCode Down;
        public KeyCode Left;
        public KeyCode Right;
        public Image timerImage;
        public Image arrowImage;
        private Tweener _timerTween;
        private bool _isInputBlocked;

        public void Initialize()
        {
            _isInputBlocked = false;
            _signalBus.Subscribe<GameOverPanel>(() => { _isInputBlocked = true; });
        }

        public void SetTurn()
        {
            IsTurn = true;
            StartTimer();

            // TimersManager.SetTimer(this, TimerDuration, TimesUp, OnProgressing);
        }

        private void Update()
        {
            if (!IsTurn || _isInputBlocked)
            {
                return;
            }

            if (Input.GetKeyDown(Up))
            {
                arrowImage.gameObject.SetActive(true);
                arrowImage.sprite = ArrowSprites[0];
                _playerController.MoveCell(new Vector2(0, 1));
            }

            if (Input.GetKeyDown(Left))
            {
                arrowImage.gameObject.SetActive(true);
                arrowImage.sprite = ArrowSprites[1];
                _playerController.MoveCell(new Vector2(-1, 0));
            }

            if (Input.GetKeyDown(Down))
            {
                arrowImage.gameObject.SetActive(true);
                arrowImage.sprite = ArrowSprites[2];
                _playerController.MoveCell(new Vector2(0, -1));
            }

            if (Input.GetKeyDown(Right))
            {
                arrowImage.gameObject.SetActive(true);
                arrowImage.sprite = ArrowSprites[3];
                _playerController.MoveCell(new Vector2(1, 0));
            }
        }

        public void TurnEnd()
        {
            IsTurn = false;
            if (_timerTween != null)
            {
                _timerTween.Kill();
                _timerTween = null;
            }

            timerImage.fillAmount = 0;
        }

        private void StartTimer()
        {
            if (_timerTween != null && !_timerTween.IsComplete())
            {
                _timerTween.TogglePause();
            }
            else
            {
                _timerTween = DOVirtual.Float(0, 1, TimerDuration, (second) => { timerImage.fillAmount = second; })
                    .SetEase(Ease.Linear)
                    .OnComplete(() => { _gameOverPanel.Show(); });
            }
        }
    }
}