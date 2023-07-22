using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Maze.UI
{
    public class GameResultPanel : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        public Transform GameOverObject;
        public Transform WinPanelObject;
        
        public void Show(bool isWin)
        {
            this.gameObject.SetActive(true);
            if (isWin)
            {
                WinPanelObject.gameObject.SetActive(true);
                WinPanelObject.transform.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
            }
            else
            {
                GameOverObject.gameObject.SetActive(true);
                GameOverObject.transform.DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
            }
            
            _signalBus.Fire<GameOver>();
        }

        public void Retry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
    }
}