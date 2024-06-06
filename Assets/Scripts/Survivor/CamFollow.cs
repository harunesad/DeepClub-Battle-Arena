using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [SerializeField] Vector3 pos;
    [SerializeField] GameManager gameManager;
    void Start()
    {
        
    }
    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, gameManager.player.position + pos, Time.deltaTime * 10);
    }
}
