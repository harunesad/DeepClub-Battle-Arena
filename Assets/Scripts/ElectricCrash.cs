using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class ElectricCrash : MonoBehaviour
{
    bool poison;
    float poisonTime;
    private void Update()
    {
        PoisonActive();
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            poison = true;
        }
    }
    void PoisonActive()
    {
        if (poison)
        {
            if (poisonTime <= 0)
            {
                ParticleSystem lightning = Instantiate(ServerControl.server.lightning, transform.position + Vector3.up * 3, ServerControl.server.lightning.transform.rotation);
                var mainModule = lightning.main;
                mainModule.startColor = Color.green;
                PowerupAttack();
            }
            poisonTime += Time.deltaTime;
            if (poisonTime >= 3)
            {
                poisonTime = 0;
            }
        }
    }
    void PowerupAttack()
    {
        GameObject canvas = transform.GetChild(0).gameObject;
        TextMeshProUGUI powerHealthText = GetComponentInChildren<TextMeshProUGUI>();
        Image powerReduceBar = canvas.transform.GetChild(0).GetComponent<Image>();
        Image powerBar = canvas.transform.GetChild(1).GetComponent<Image>();
        float powerHealth = float.Parse(powerHealthText.text);
        if (powerHealth <= 0)
        {
            return;
        }
        powerHealth -= 100;
        powerHealth = powerHealth <= 0 ? 0 : powerHealth;
        powerHealthText.text = ((int)powerHealth).ToString();
        powerBar.DOFillAmount(powerHealth / 1000, .1f).
            SetEase(Ease.Linear).OnComplete(() =>
            {
                powerReduceBar.DOFillAmount(powerBar.fillAmount, .25f).SetEase(Ease.Linear);
            });
        if (powerHealth == 0)
        {
            transform.GetChild(transform.childCount - 1).DORotate(new Vector3(27, 0, 0), .5f).SetEase(Ease.Linear);
            transform.GetChild(transform.childCount - 2).DORotate(new Vector3(27, 90, 270), .5f).SetEase(Ease.Linear);
            transform.GetChild(transform.childCount - 3).DORotate(new Vector3(27, 180, 180), .5f).SetEase(Ease.Linear);
            transform.GetChild(transform.childCount - 4).DORotate(new Vector3(27, 270, 90), .5f).SetEase(Ease.Linear);
            GetComponent<BoxCollider>().isTrigger = true;
            gameObject.layer = 0;
            int randomId = 1;
            transform.GetChild(randomId).gameObject.SetActive(true);
        }
        int newAttack = 100;
        ShowDamage(newAttack.ToString(), transform.position + Vector3.up, Quaternion.Euler(0, 90, 0));
    }
    void ShowDamage(string text, Vector3 pos, Quaternion rotation)
    {
        GameObject prefab = Instantiate(UIManager.uIManager.damagePopup, pos, rotation);
        prefab.GetComponentInChildren<TextMesh>().text = text;
    }
}
