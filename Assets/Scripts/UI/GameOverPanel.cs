using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Maze.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        
        public void Show()
        {
            this.gameObject.SetActive(true);
            _signalBus.Fire<GameOver>();
        }

        public void Retry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
    }
}