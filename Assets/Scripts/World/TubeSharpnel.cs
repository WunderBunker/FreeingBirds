using UnityEngine;

public class TubeSharpnel : MonoBehaviour
{

    float _speed;

    // Start is called before the first frame update
    void OnEnable()
    {
        _speed = GetComponentInParent<MeanTubeFlying>()._speed * 1.2f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.rotation * Vector3.up * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PolyA") || collision.CompareTag("PolyB"))
            Destroy(gameObject);
    }
}
