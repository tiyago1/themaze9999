using System.Collections;
using Timers;
using TMPro;
using UnityEngine;
using Zenject;

namespace Maze
{
    public class Player : MonoBehaviour
    {
        [Inject] private PlayerController _playerController;
        public int Index;
        public bool IsTurn;
        public float TimerDuration;
        public KeyCode Up;
        public KeyCode Down;
        public KeyCode Left;
        public KeyCode Right;
        public TextMeshProUGUI timerText;
        private Coroutine _timerCoroutine;
        
        public void SetTurn()
        {
            IsTurn = true;

            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine); 
                _timerCoroutine = null;
            }
            
            _timerCoroutine = StartCoroutine(Timer());

            // TimersManager.SetTimer(this, TimerDuration, TimesUp, OnProgressing);
        }

        private void Update()
        {
            if (!IsTurn)
            {
                return;
            }
            
            if (Input.GetKeyDown(Up))
            {
                _playerController.MoveCell(new Vector2(0, 1));
            }

            if (Input.GetKeyDown(Left))
            {
                _playerController.MoveCell(new Vector2(-1, 0));
            }

            if (Input.GetKeyDown(Down))
            {
                _playerController.MoveCell(new Vector2(0, -1));
            }

            if (Input.GetKeyDown(Right))
            {
                _playerController.MoveCell(new Vector2(1, 0));
            }
        }
        
        public void TurnEnd()
        {
            IsTurn = false;
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine); 
                _timerCoroutine = null;
            }
            
            timerText.gameObject.SetActive(false);
        }
        
        private void OnProgressing(int time)
        {
            timerText.text = time.ToString();
        }

        private IEnumerator Timer()
        {
            timerText.gameObject.SetActive(true);

            for (int i = (int)TimerDuration; i >= 0; i--)
            {
                OnProgressing(i);
                yield return new WaitForSecondsRealtime(1);
            }

            _playerController.TurnChange();
        }
        
    }
}