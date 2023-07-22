using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Maze
{
    public class PlayerController : MonoBehaviour
    {
        [Inject] private MazeGenerator _maze;
        public MazeGenerator.Cell CurrentCell;
        public Player p1;
        public Player p2;

        private Player activeTurnPlayer;

        private void Start()
        {
            TurnChange();
        }

        public void TurnChange()
        {
            if (activeTurnPlayer == null)
            {
                activeTurnPlayer = p1;
            }
            else
            {
                activeTurnPlayer.TurnEnd();
                activeTurnPlayer = activeTurnPlayer == p1 ? p2 : p1;
            }

            activeTurnPlayer.SetTurn();
        }

        public void MoveCell(Vector2 direction)
        {
            bool keyExist = _maze.allCells.ContainsKey(CurrentCell.gridPos + direction);
            if (!keyExist)
            {
                return;
            }

            var nextCell = _maze.allCells[CurrentCell.gridPos + direction];
            if (nextCell.cScript.GetColumnWithDirection(-direction).IsActive)
            {
                return;
            }
            TurnChange();
            SetCell(nextCell);
        }

        public void SetCell(MazeGenerator.Cell cell)
        {
            CurrentCell = cell;
            // this.transform.position = CurrentCell.cellObject.transform.position;

            this.transform.DOMove(CurrentCell.cellObject.transform.position, .2f);
        }
    }
}