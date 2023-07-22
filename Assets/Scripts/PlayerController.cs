using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Maze
{
    public class PlayerController : MonoBehaviour
    {
        [Inject] private MazeGenerator _maze;
        [Inject] private HealthController _healthController;
        [SerializeField] private Camera _camera;

        public MazeGenerator.Cell CurrentCell;
        public Player p1;
        public Player p2;


        private Player activeTurnPlayer;

        public void Initialize()
        {
            _healthController.Initialize();

            CurrentCell = _maze.StartCell;
            this.transform.position = CurrentCell.cellObject.transform.position;

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

        public void ShakeCamera()
        {
            // float cameraX = _camera.transform.position.x;
            // float cameraY = _camera.transform.position.y;
            // float cameraZ = _camera.transform.position.z;
            // _camera.transform.position = new Vector3(cameraX + 5, cameraY + 5, cameraZ);
            var defaultPosition = _camera.transform.position;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_camera.DOShakePosition(.2f, 0.2f, 1));
            sequence.Append(_camera.transform.DOMove(defaultPosition, 0.3f));
            sequence.Play();
        }

        public void MoveCell(Vector2 direction)
        {
            bool keyExist = _maze.allCells.ContainsKey(CurrentCell.gridPos + direction);
            if (!keyExist)
            {
                var column = CurrentCell.cScript.GetColumnWithDirection(direction);
                column.ShowWallInteractionEffect(direction, null);
                _healthController.TakeDamage();
                ShakeCamera();
                TurnChange();
                return;
            }

            var nextCell = _maze.allCells[CurrentCell.gridPos + direction];

            var currentColumn = CurrentCell.cScript.GetColumnWithDirection(direction);
            if (currentColumn.IsActive)
            {
                var nextCellColumn = nextCell.cScript.GetColumnWithDirection(-direction);
                nextCellColumn.SetActive(false);
                currentColumn.ShowWallInteractionEffect(direction,
                                                        () =>
                                                        {
                                                            nextCellColumn.SetActive(true);
                                                        });

                _healthController.TakeDamage();
                ShakeCamera();
                TurnChange();
                return;
            }

            TurnChange();
            SetCell(nextCell);
        }

        public void SetCell(MazeGenerator.Cell cell)
        {
            CurrentCell = cell;
            // this.transform.position = CurrentCell.cellObject.transform.position;

            this.transform.DOMove(CurrentCell.cellObject.transform.position, .1f);
        }
    }
}