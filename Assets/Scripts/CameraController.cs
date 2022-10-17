using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private Transform model;
    void Start() {
        transform.forward = (model.transform.position - transform.position).normalized;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
