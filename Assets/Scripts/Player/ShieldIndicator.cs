using UnityEngine;

public class ShieldIndicator : MonoBehaviour
{
    [SerializeField] float _speed = 100;
    [SerializeField] float _radius = 4.8f;

    Transform _haloCenter;

    void Start()
    {
        _haloCenter = transform.parent.Find("ShieldHalo").transform;

        for (int lCptDollar = 0; lCptDollar < transform.childCount; lCptDollar++)
        {
            Transform lDollar = transform.GetChild(lCptDollar);

            lDollar.position = _haloCenter.position + Quaternion.AngleAxis(lCptDollar * 120, new Vector3(0, 0, 1)) * Vector3.up * _radius; ;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int lCptDollar = 0; lCptDollar < transform.childCount; lCptDollar++)
        {
            GameObject lDollar = transform.GetChild(lCptDollar).gameObject;
            lDollar.transform.RotateAround(_haloCenter.position, Vector3.forward, _speed * Time.deltaTime);
        }
    }

    public void LooseDollar()
    {
        //On trouve le premier dollar désactivé et on l'active
        for (int lCptDollar = 0; lCptDollar < transform.childCount; lCptDollar++)
        {
            GameObject lDollar = transform.GetChild(lCptDollar).gameObject;
            if (lDollar.activeSelf)
            {
                lDollar.SetActive(false);
                break;
            }
        }
    }

    public void GainDollar()
    {
        //On trouve le premier dollar activé et on le désactive
        for (int lCptDollar = 0; lCptDollar < transform.childCount; lCptDollar++)
        {
            GameObject lDollar = transform.GetChild(lCptDollar).gameObject;
            if (!lDollar.activeSelf)
            {
                lDollar.SetActive(true);
                break;
            }
        }
    }
}
