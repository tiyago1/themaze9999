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

        public Camera camera;

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
            IsGameOver = false;
            
            _mazeGenerator.Initialize();
            _healthController.Initialize();
            _playerController.Initialize();
            SetUpCamera(_mazeGenerator.mazeRows);
        }
    }
}