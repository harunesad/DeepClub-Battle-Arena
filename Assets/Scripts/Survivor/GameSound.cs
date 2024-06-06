using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSound : MonoBehaviour
{
    [SerializeField] AudioClip click;
    public AudioSource musicWindow;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Click()
    {
        AudioSource.PlayClipAtPoint(click, Vector3.zero, DataSave.Instance.mainSound);
    }
}
