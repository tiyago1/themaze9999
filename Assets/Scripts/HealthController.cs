using System.Collections.Generic;
using DG.Tweening;
using Maze.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Maze
{
    public class HealthController : MonoBehaviour
    {
        public List<Image> HearthsImages;
        public int MaxHealth;
        public int Health;

        [Inject] private GameOverPanel _gameOverPanel;
        
        public void Initialize()
        {
            Health = MaxHealth;
        }

        public void TakeDamage()
        {
            Health--;
            var hearth = HearthsImages[Health - 1];

            hearth.transform.DOScale(Vector3.zero, 1);

            if (Health <= 0)
            {
                _gameOverPanel.Show();
            }
        }
    }
}