using UnityEngine;

public class TubeSharpnel : MonoBehaviour
{

    float _speed;

    // Start is called before the first frame update
    void OnEnable()
    {
        _speed = GetComponentInParent<MeanTubeFlying>()._speed;
        transform.SetParent(transform.parent.parent);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Quaternion.AngleAxis(transform.localEulerAngles.z, new Vector3(0, 0, 1)) * Vector3.up * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PolyA") || collision.CompareTag("PolyB"))
            Destroy(gameObject);
    }
}
