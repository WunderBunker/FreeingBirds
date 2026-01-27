using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BookImage : MonoBehaviour
{
    [SerializeField] BirdList _birdList;
    [SerializeField] float _openingSpeed = 1;

    private float _opening = 0;
    public Material _bookMat { get; set; }

    void Awake()
    {
        _bookMat = Instantiate(GetComponent<Image>().material);
        GetComponent<Image>().material = _bookMat;
        _bookMat.SetFloat("_Opening", 0);
    }

    void OnEnable()
    {
        if (_birdList)
        {
            Color vCol = Array.Find(_birdList.List, (e) => e.Id == SaveManager.SafeSave.SelectedBirdId).Color;
            vCol *= 2f;
            _bookMat.SetColor("_OLColor", vCol);
        }
        StartCoroutine(OpenBook());
    }

    public void Quit()
    {
        StartCoroutine(CloseBook());
    }

    IEnumerator OpenBook()
    {
        _opening = _bookMat.GetFloat("_Opening");
        while (_opening < 1)
        {
            _opening = Mathf.MoveTowards(_opening, 1, _openingSpeed * Time.unscaledDeltaTime);
            _bookMat.SetFloat("_Opening", _opening);

            yield return null;
        }
        _bookMat.SetFloat("_Opening", 1);

        transform.parent.GetComponent<IChildEnabler>()?.EnableChilds(true, new string[] { "Book" });
    }

    IEnumerator CloseBook()
    {
        transform.parent.GetComponent<IChildEnabler>()?.EnableChilds(false, new string[] { "Book" });
        _opening = _bookMat.GetFloat("_Opening");
        while (_opening > 0f)
        {
            _opening = Mathf.MoveTowards(_opening, 0f, _openingSpeed * Time.unscaledDeltaTime);
            _bookMat.SetFloat("_Opening", _opening);

            yield return null;
        }
        _bookMat.SetFloat("_Opening", 0);

        transform.parent.gameObject.SetActive(false);
    }
}
