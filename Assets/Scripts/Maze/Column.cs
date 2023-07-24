using System;
using System.Collections.Generic;
using DG.Tweening;
using Maze;
using UnityEngine;
using Zenject;

public class Column : MonoBehaviour
{
    public Vector2 Direction;
    public bool IsActive;

    [Inject] private GameManager _gameManager;
    
    public List<Color> Colors;
    public SpriteRenderer CellRenderer;
    private Color _activeColor;
    private Sequence _sequence;
    private Vector3 _defaultPosition;
    private Vector3 _defaultScale;

    private void Awake()
    {
        _defaultPosition = this.transform.localPosition;
        _defaultScale = this.transform.localScale;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        var type = isActive ? CellType.Empty : CellType.Fog;
        _activeColor = Colors[(int) type];
        CellRenderer.color = Colors[(int) type];
        CellRenderer.sortingOrder = isActive ? 5 : -1;
        // this.gameObject.SetActive(IsActive);
    }

    public void SetWallColor(Color color)
    {
        Colors[(int) CellType.Empty] = color;

        SetActive(true);
    }

    public void ShowWallInteractionEffect(Vector3 direction, Action onComplete)
    {
        CellRenderer.color = _activeColor;
        CellRenderer.transform.localPosition = _defaultPosition;
        CellRenderer.transform.localScale = _defaultScale;
        // CellRenderer.DOFade(0, .6f);
        if (_sequence != null && !_sequence.IsComplete())
        {
            _sequence.Kill();
        }
        // _gameManager.SoundManager.PlayHitWall();

        _sequence = DOTween.Sequence();
        _sequence.Join(CellRenderer.transform.DOLocalMove(_defaultPosition + (direction * 0.3f), 0.3f).SetEase(Ease.InOutBack));
        _sequence.Join(CellRenderer.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBack));
        _sequence.Join(CellRenderer.DOColor(Color.red, .3f));
        _sequence.OnComplete(() =>
            {
                CellRenderer.color = _activeColor;
                Sequence growUp = DOTween.Sequence();
                growUp.Join(CellRenderer.transform.DOLocalMove(_defaultPosition, .2f));
                growUp.Join(CellRenderer.transform.DOScale(_defaultScale, .2f));
                growUp.SetEase(Ease.InOutBack);
                growUp.Play();
                onComplete?.Invoke();
            }
        );
        _sequence.Play();
        // CellRenderer.transform.DOPunchScale(Vector3.one, .5f, 1, 0);
    }
}