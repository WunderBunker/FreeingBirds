using UnityEngine;
using UnityEngine.UI;

public class NoiseScreen : MonoBehaviour
{
    [SerializeField] float _speed = 0.3f;

    //true - Raising ; false - lowering ;
    public bool _isAppearing;

    void OnEnable()
    {
        Color vNewColor = gameObject.GetComponent<Image>().color;
        vNewColor = new Vector4(vNewColor.r, vNewColor.g, vNewColor.b, 0);

        gameObject.GetComponent<Image>().color = vNewColor;
    }

    void Update()
    {
        if (_isAppearing)
        {
            if (gameObject.GetComponent<Image>().color.a >= 1) _isAppearing = false;
            else
            {
                float vNewAlpha = gameObject.GetComponent<Image>().color.a;
                vNewAlpha += _speed * Time.deltaTime;

                Color vNewColor = gameObject.GetComponent<Image>().color;
                vNewColor = new Vector4(vNewColor.r, vNewColor.g, vNewColor.b, vNewAlpha);

                gameObject.GetComponent<Image>().color = vNewColor;
                gameObject.GetComponent<Image>().material.SetFloat("_Alpha", vNewAlpha);
            }
        }
    }
}
