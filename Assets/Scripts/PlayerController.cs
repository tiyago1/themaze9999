using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Maze
{
    public class PlayerController : MonoBehaviour
    {
        [Inject] private MazeGenerator _maze;
        public MazeGenerator.Cell CurrentCell;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                MoveCell(new Vector2(0, 1));
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                MoveCell(new Vector2(-1, 0));
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                MoveCell(new Vector2(0, -1));
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                MoveCell(new Vector2(1, 0));
            }
        }

        public void MoveCell(Vector2 direction)
        {
            var nextCell = _maze.allCells[CurrentCell.gridPos + direction];
            if (nextCell.cScript.GetColumnWithDirection(-direction).IsActive)
            {
                return;
            }
            
            SetCell(nextCell);
        }

        public void SetCell(MazeGenerator.Cell cell)
        {
            CurrentCell = cell;
            // this.transform.position = CurrentCell.cellObject.transform.position;
            
            this.transform.DOMove(CurrentCell.cellObject.transform.position,.2f);
        }
    }
}