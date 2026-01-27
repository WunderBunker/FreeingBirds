using UnityEngine;
using System.Collections.Generic;

public class PlayerShield : MonoBehaviour
{
    [SerializeField] List<AudioClip> _hurtNoisesList;
    [SerializeField] List<AudioClip> _moneyNoisesList = new List<AudioClip>();
    [SerializeField] int _dollarValue = 50;

    [SerializeField] float _invulnerabilityTime = 1.2f;

    HaloShield _shieldHalo;

    System.Random _ranNoise = new System.Random();

    public int _shieldsCount { get; private set; } = 0;

    float _invulnerabilityStartTime;

    void Start()
    {
        _shieldHalo = transform.Find("ShieldHalo").GetComponent<HaloShield>();
    }

    void Update()
    {
        if (Time.time - _invulnerabilityStartTime > _invulnerabilityTime)
        {
            _invulnerabilityStartTime = 0;
            gameObject.GetComponent<Animator>().SetBool("Invulnerable", false);
        }
    }

    public void GainShield()
    {
        if (_shieldsCount < 3)
        {
            _shieldHalo.HaloGainShield();
            _shieldsCount += 1;
            gameObject.GetComponentInChildren<ShieldIndicator>().GainDollar();
            AudioManager.Instance.PlaySound(_moneyNoisesList[0], 1f, transform.position);
            PartieManager.Instance.GetComponent<PartieManager>().AddPonctualAvancement(_dollarValue);
        }
        else
        {
            AudioManager.Instance.PlaySound(_moneyNoisesList[1], 1f, transform.position);
            PartieManager.Instance.GetComponent<PartieManager>().AddPonctualAvancement(_dollarValue);
        }
    }

    public void LooseShield()
    {
        if (_invulnerabilityStartTime != 0) return;

        _invulnerabilityStartTime = Time.time;
        gameObject.GetComponent<Animator>().SetBool("Invulnerable", true);

        _shieldHalo.HaloLoseShield();

        if (_shieldsCount > 0)
        {
            _shieldsCount -= 1;
            if (_shieldsCount == 0) _shieldHalo.NoMoreShield();

            gameObject.GetComponentInChildren<ShieldIndicator>().LooseDollar();
            AudioManager.Instance.PlaySound(_hurtNoisesList[_ranNoise.Next(0, _hurtNoisesList.Count - 1)], 1f, transform.position);
            PartieManager.Instance.GetComponent<PartieManager>().RemovePonctualAvancement(_dollarValue);

        }
    }


}
