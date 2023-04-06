using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spinner : MonoBehaviour
{
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddTorque(Vector3.up * 100000);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
