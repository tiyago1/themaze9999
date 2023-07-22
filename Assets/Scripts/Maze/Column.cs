using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Column : MonoBehaviour
{
    public Vector2 Direction;
    public bool IsActive;

    public List<Color> Colors;
    public SpriteRenderer CellRenderer;
    private Color _activeColor;
    private Sequence _sequence;
    private Vector3 _defaultPosition;

    private void Awake()
    {
        SetActive(true);
        _defaultPosition = this.transform.localPosition;
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

    public void ShowWallInteractionEffect(Vector3 direction)
    {
        CellRenderer.color = _activeColor;
        CellRenderer.transform.localPosition = _defaultPosition;
        // CellRenderer.DOFade(0, .6f);
        if (_sequence != null && !_sequence.IsComplete())
        {
            _sequence.Kill();
        }

        _sequence = DOTween.Sequence();
        // _sequence.Join(CellRenderer.transform.DOLocalMove(_defaultPosition + (direction * 0.3f), 0.5f).SetEase(Ease.InOutBack));
        _sequence.Join(CellRenderer.DOColor(Color.red, .3f));
        // _sequence.Append(CellRenderer.DOColor(_activeColor, .3f));
        _sequence.OnComplete(() =>
            {
                CellRenderer.color = _activeColor;
                CellRenderer.transform.localPosition = _defaultPosition;
            }
        );
        _sequence.Play();
        // CellRenderer.transform.DOPunchScale(Vector3.one, .5f, 1, 0);
    }
}