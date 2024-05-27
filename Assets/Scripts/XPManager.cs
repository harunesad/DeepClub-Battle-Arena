using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class XPManager : MonoBehaviour
{
    public static XPManager xp;
    public float xpLevel = 1;
    public float xpAmount;
    private void Awake()
    {
        xp = this;
    }
    void Start()
    {
        UIManager.uIManager.xpLevel.text = "Level " + xpLevel;
    }
    void Update()
    {
        
    }
    public void XPIncrease(float inc)
    {
        xpAmount += inc;
        UIManager.uIManager.xpBar.DOFillAmount(xpAmount, .5f).SetEase(Ease.Linear);
        if (xpAmount >= 1)
        {
            xpLevel++;
            UIManager.uIManager.xpLevel.text = "Level " + xpLevel;
            xpAmount = 0;
            UIManager.uIManager.xpBar.DOFillAmount(xpAmount, .5f).SetEase(Ease.Linear);
        }
    }
}
