using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class BirdMenuItem : MonoBehaviour
{
    [SerializeField] Sprite _openImage;
    [SerializeField] Sprite _closeImage;
    [SerializeField] float _flottingSpeed = 1;
    public Color CadenasColor;

    public Bird Bird;

    Toggle _toggle;
    RectTransform _rect;

    float _decalage;

    private void Awake()
    {
        _toggle = GetComponentInChildren<Toggle>();

        //Pour dissocier les propriétés de matériaux entre les différents items
        Material vMat = Instantiate(transform.Find("BirdImage").GetComponent<Image>().material);
        transform.Find("BirdImage").GetComponent<Image>().material = vMat;
        vMat.SetInt("_Chosen", 0);

        transform.Find("BirdImage").transform.SetSiblingIndex(2);

        _rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        transform.Find("BirdImage").GetComponent<Image>().material.SetVector("_OLSpriteCenter", Bird.ImageOutlineOffset);
        _decalage = transform.GetSiblingIndex();
    }

    void Update()
    {
        if (gameObject.activeSelf && !_toggle.isOn)
        {
            float vSin = Mathf.Sin(Time.fixedTime + _decalage);
            _rect.anchoredPosition += Mathf.Sign(vSin) * Vector2.up * Time.deltaTime * math.lerp(_flottingSpeed / 10, _flottingSpeed, Math.Abs(vSin));
        }
    }

    public void MakeAccessible()
    {
        transform.Find("CageFrontImage").GetComponent<Image>().sprite = _openImage;
        transform.Find("BirdImage").GetComponent<Image>().material.SetInt("_Trapped", 0);
        _toggle.interactable = true;
        transform.Find("Cadenas").gameObject.SetActive(false);
    }

    public void MakeUnAccessible()
    {
        transform.Find("CageFrontImage").GetComponent<Image>().sprite = _closeImage;
        transform.Find("BirdImage").GetComponent<Image>().material.SetInt("_Trapped", 1);
        _toggle.interactable = false;
        transform.Find("Cadenas").gameObject.SetActive(true);
        transform.Find("Cadenas").GetComponentInChildren<TextMeshProUGUI>().text = Bird.ScoreToBeAccessible.ToString();
        transform.Find("Cadenas").GetComponentInChildren<TextMeshProUGUI>().color = Bird.Color;
    }

    public void ChooseBird()
    {
        //Safeguard de l'initialisation du togglegroup qui va sélection un toggle avant le chargement de quoi que soit 
        if (PartieManager.Instance._partieState == PartieState.MustLoadSave
        || PartieManager.Instance._partieState == PartieState.LoadingSave) return;


        //On ne prend pas gToggle car la méthode peut être appelée avant le awake
        if (GetComponentInChildren<Toggle>().isOn)
        {
            transform.Find("BirdImage").transform.SetSiblingIndex(3);
            transform.Find("BirdImage").GetComponent<Image>().material.SetInt("_Chosen", 1);
            transform.Find("BirdImage").localScale = 1.5f * Vector3.one;

            GetComponentInParent<MenuManager>().ChooseBird(Bird.Id);
        }
        else
        {
            transform.Find("BirdImage").transform.SetSiblingIndex(1);
            transform.Find("BirdImage").GetComponent<Image>().material.SetInt("_Chosen", 0);
            transform.Find("BirdImage").localScale = Vector3.one;
        }
    }
}
