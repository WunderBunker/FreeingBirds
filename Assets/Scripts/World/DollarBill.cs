using UnityEngine;

public class DollarBill : MonoBehaviour
{

    [SerializeField] int _floatingSpeed =5;

    void Awake()
    {
        System.Random vRan = new System.Random();
        _floatingSpeed = vRan.Next(_floatingSpeed/2, _floatingSpeed );

        int vSignedFloatSpeed;
        vSignedFloatSpeed = _floatingSpeed * (int)Mathf.Sign( vRan.Next(-1,1));
        if(vSignedFloatSpeed != 0) _floatingSpeed = vSignedFloatSpeed;
    }

    void Update()
    {
        transform.position += Mathf.Sign(Mathf.Sin(Time.fixedTime*10))*Vector3.up*Time.deltaTime*_floatingSpeed ;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(collision.gameObject.GetComponent<PlayerShield>() != null) collision.gameObject.GetComponent<PlayerShield>().GainShield();
            Destroy(gameObject);
        }
    }
}
