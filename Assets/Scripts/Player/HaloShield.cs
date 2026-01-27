using System.Collections;
using UnityEngine;

public class HaloShield : MonoBehaviour
{
    [SerializeField] float _shieldHaloAlphaMin = 0.2f;
    [SerializeField] float _shieldHaloAlphaPace = 0.3f;
    float _shieldHaloAlpha;

    void Start()
    {
        NoMoreShield();
    }

    public void HaloLoseShield()
    {
        gameObject.GetComponent<Animator>().SetTrigger("Lose");

        _shieldHaloAlpha -= _shieldHaloAlphaPace;
        _shieldHaloAlpha = Mathf.Max(_shieldHaloAlpha, _shieldHaloAlphaMin);

        UpdateHaloAlpha();
    }

    public void HaloGainShield()
    {
        gameObject.GetComponent<Animator>().SetTrigger("Gain");

        if (gameObject.GetComponent<SpriteRenderer>().color.a > 0) //si l'alpha est encore à 0 alors on laisse notre variable à la valeur min (première valeur pour un halo actif)
            _shieldHaloAlpha += _shieldHaloAlphaPace;

        UpdateHaloAlpha();
    }

    public void NoMoreShield()
    {
        StartCoroutine(WaitHitFinishForInvisibleize());
        _shieldHaloAlpha = 0;
        UpdateHaloAlpha();
        _shieldHaloAlpha = _shieldHaloAlphaMin;
    }

    void UpdateHaloAlpha()
    {
        Color vNewColor = gameObject.GetComponent<SpriteRenderer>().color;
        vNewColor.a = _shieldHaloAlpha;
        gameObject.GetComponent<SpriteRenderer>().color = vNewColor;
    }

    IEnumerator WaitHitFinishForInvisibleize()
    {
        //To-Do : faire un wait for seconds plutot non ?
        yield return gameObject.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length;
    }

    

}
