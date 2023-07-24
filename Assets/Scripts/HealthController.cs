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

        public int Health
        {
            get { return PlayerPrefs.GetInt("Health", 3); }
            set { PlayerPrefs.SetInt("Health", value); }
        }

        [Inject] private GameResultPanel _gameResultPanel;

        public void Initialize()
        {
            for (int i = 0; i < Health; i++)
            {
                var hearth = HearthsImages[i];

                hearth.transform.DOScale(Vector3.one, 1);
            }
        }

        public void TakeDamage()
        {
            if (Health <= 0)
            {
                return;
            }
            
            Debug.Log("TAKE DAMAGE " + Health);
            var hearth = HearthsImages[Health - 1];
            Health--;

            hearth.transform.DOScale(Vector3.zero, 1);

            if (Health <= 0)
            {
                _gameResultPanel.Show(false);
            }
        }

        public void GetHealth()
        {
            Health++;
            var hearth = HearthsImages[Health - 1];

            hearth.transform.DOScale(Vector3.one, 1);
        }
    }
}