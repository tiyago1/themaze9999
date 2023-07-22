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

        [Inject] private GameResultPanel _gameResultPanel;
        
        public void Initialize()
        {
            Health = MaxHealth;
        }

        public void TakeDamage()
        {
            Health--;
            var hearth = HearthsImages[Health];

            hearth.transform.DOScale(Vector3.zero, 1);

            if (Health <= 0)
            {
                _gameResultPanel.Show(false);
            }
        }
    }
}