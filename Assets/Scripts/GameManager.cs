using UnityEngine;
using Zenject;

namespace Maze
{
    public class GameManager : MonoBehaviour,IInitializable
    {
        [Inject] private MazeGenerator _mazeGenerator;
        [Inject] private HealthController _healthController;
        [Inject] private PlayerController _playerController;
        
        public bool IsGameOver;

        public void Initialize()
        {
            IsGameOver = false;
            
            _mazeGenerator.Initialize();
            _healthController.Initialize();
            _playerController.Initialize();
        }
    }
}