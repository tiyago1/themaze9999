﻿using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Maze
{
    public class PlayerController : MonoBehaviour
    {
        [Inject] private MazeGenerator _maze;
        [Inject] private HealthController _healthController;
        [Inject] private GameManager _gameManager;
        [SerializeField] private Camera _camera;

        public MazeGenerator.Cell CurrentCell;
        public Player p1;
        public Player p2;

        private Player activeTurnPlayer;

        public void Initialize()
        {
            p1.Initialize();
            p2.Initialize();

            CurrentCell = _maze.StartCell;
            this.transform.position = CurrentCell.cellObject.transform.position;

            // TurnChange();
        }

        public void DisableAllIndicators()
        { 
            p1.KeyIndicator.gameObject.SetActive(false);
            p2.KeyIndicator.gameObject.SetActive(false);
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
            var defaultPosition = _camera.transform.position;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_camera.DOShakePosition(.2f, .5f, 1));
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
                _gameManager.SoundManager.PlayHitWall();
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
                currentColumn.ShowWallInteractionEffect(direction, () => { nextCellColumn.SetActive(true); });
                _gameManager.SoundManager.PlayHitWall();
                ShakeCamera();
                _healthController.TakeDamage();
                TurnChange();
                return;
            }

            TurnChange();
            SetCell(nextCell);
        }

        public void SetCell(MazeGenerator.Cell cell)
        {
            CurrentCell = cell;
            var renderer = this.GetComponent<SpriteRenderer>();
            var cellSortingGroup = CurrentCell.cScript.GrayArea.GetComponent<SortingGroup>();
            if (CurrentCell.gridPos == _maze.StartCell.gridPos || CurrentCell.gridPos == _maze.EndCell.gridPos)
            {
                cellSortingGroup.sortingLayerName = "Player";
                cellSortingGroup.sortingOrder = 0;
                
                renderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            }
            else
            {
                cellSortingGroup.sortingLayerName = "Maze";
                cellSortingGroup.sortingOrder = 1;
                renderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            }

            if (CurrentCell.cScript.Potion.gameObject.activeSelf)
            {
                _healthController.GetHealth();
            }
            
            SortingGroup.UpdateAllSortingGroups();

            // this.transform.position = CurrentCell.cellObject.transform.position;

            this.transform.DOMove(CurrentCell.cellObject.transform.position, .1f).OnComplete(() => { _gameManager.CheckGameEnd(CurrentCell); });
        }
    }
}