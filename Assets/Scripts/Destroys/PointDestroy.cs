using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointDestroy : MonoBehaviour
{
    [SerializeField] GameObject parent;
    void Update()
    {
        PointDestroying();
    }
    void PointDestroying()
    {
        if (parent == null)
        {
            Destroy(gameObject);
        }
    }
}
