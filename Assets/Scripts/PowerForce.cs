using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerForce : MonoBehaviour
{
    Rigidbody rb;
    BoxCollider bc;
    [SerializeField] float pos;
    public bool collectable;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
    }
    void Start()
    {
        rb.velocity = transform.forward * pos;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            rb.constraints = RigidbodyConstraints.FreezePosition;
            collectable = true;
        }
    }
}
