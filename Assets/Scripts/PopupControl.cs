using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PopupControl : MonoBehaviour
{
    void Start()
    {
        transform.DOMove(transform.position + transform.parent.forward, 1).SetEase(Ease.Linear);
        transform.DOScale(new Vector3(.5f, .5f, .5f), 1).SetEase(Ease.Linear).OnComplete(() =>
        {
            Destroy(transform.parent.gameObject);
        });
    }
}
