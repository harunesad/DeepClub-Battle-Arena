using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasDestroying : MonoBehaviour
{
    [SerializeField] GameObject parent;
    void Update()
    {
        CanvasDestroy();
    }
    private void LateUpdate()
    {
        CanvasFollow();
    }
    void CanvasDestroy()
    {
        if (parent == null)
        {
            Destroy(gameObject);
        }
    }
    void CanvasFollow()
    {
        if (parent != null)
        {
            transform.position = Vector3.Lerp(transform.position, parent.transform.position + Vector3.up * 2.05f, 100);
        }
    }
}
