using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] private RawImage _img;
    [SerializeField] private float _speed = 0.1f;
    [SerializeField] private float _amplitude = 0.05f;

    private void Update()
    {
        float x = Mathf.Sin(Time.time * _speed) * _amplitude;
        float y = Mathf.Cos(Time.time * _speed) * _amplitude;

        _img.uvRect = new Rect(new Vector2(x, y), _img.uvRect.size);
    }
}
