using UnityEngine;
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
    }
}