using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : MonoBehaviour
{
    [SerializeField] float _speed = 20;

    //true - Raising ; false - lowering ;
    public bool _isRaising;
    public bool _stopMoving;

    void Update()
    {
        if (_isRaising)
        {
            if (_stopMoving) _stopMoving = false;
            if (gameObject.GetComponent<RectTransform>().anchoredPosition.y >= gameObject.GetComponent<RectTransform>().rect.height) 
                gameObject.SetActive(false);
            else gameObject.GetComponent<RectTransform>().anchoredPosition += Vector2.up * _speed * Time.deltaTime;
        }
        else if (!_stopMoving)
        {
            if (gameObject.GetComponent<RectTransform>().anchoredPosition.y <= 0.1f) _stopMoving = true;
            else gameObject.GetComponent<RectTransform>().anchoredPosition -= Vector2.up * _speed * Time.deltaTime;
        }
    }
}
