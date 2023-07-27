using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int speed = 100;
    [SerializeField] private Rigidbody2D rigid2d;
    // Start is called before the first frame update
    void Start()
    {
        rigid2d.AddForce(transform.position * speed);
    }

    private void FixedUpdate()
    {
        float angle = Mathf.Atan2(rigid2d.velocity.y, rigid2d.velocity.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
