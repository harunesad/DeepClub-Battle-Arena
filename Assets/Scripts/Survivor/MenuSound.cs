using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSound : MonoBehaviour
{
    [SerializeField] AudioClip click;
    public AudioSource characterWindow, loadingWindow;
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void Click()
    {
        AudioSource.PlayClipAtPoint(click, Vector3.zero, DataSave.Instance.mainSound);
    }
}
