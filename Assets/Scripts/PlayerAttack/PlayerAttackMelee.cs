using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System;
using DG.Tweening;
using TMPro;

public class PlayerAttackMelee : MonoBehaviour
{
    PhotonView photonView;
    Animator avatarAnim;
    Player player;
    float time, newAmount;
    public float animationFinish, maxBulletCount;
    bool nameFind;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LineRenderer line;
    [SerializeField] GameObject attackText, attackLookat;
    [SerializeField] float lineDistance;
    [SerializeField] AudioClip attackSound;
    RaycastHit hit;
    bool shoot, shootSuper;
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        avatarAnim = GetComponent<Animator>();
        player = GetComponent<Player>();
    }
    void Update()
    {
        if (photonView.IsMine)
        {
            if (GameObject.FindGameObjectsWithTag("Player").Length == 4 && !nameFind)
            {
                nameFind = true;
            }
            else if (GameObject.FindGameObjectsWithTag("Player").Length != 4 && !nameFind)
            {
                return;
            }
            time += Time.deltaTime;
#if UNITY_WEBGL
            WebAttack();
#endif
#if UNITY_ANDROID || UNITY_IOS
            AndroidAttack();
#endif
        }
    }
    void WebAttack()
    {
        if (Input.GetMouseButtonDown(0) && time >= 1.1f && player.bulletBar.fillAmount >= (1 / maxBulletCount))
        {
            photonView.RPC("MeleeReload", RpcTarget.All);
            SwordAnimation();
            Collider[] colliders = Physics.OverlapSphere(transform.position, 1, playerLayer);
            Array.Sort(colliders, new DistanceCompare(transform));
            foreach (var item in colliders)
            {
                if (item.gameObject.layer == 12 && ServerControl.server.nickName != item.gameObject.name)
                {
                    transform.LookAt(item.transform);
                    AttackSword(item, player.attack, false);
                    break;
                }
                else if (item.gameObject.layer == 9)
                {
                    transform.LookAt(item.transform);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    AttackSwordPowerup(item, player.attack);
                    break;
                }
            }
            return;
        }
        WebSuperAttack();
    }
    void WebSuperAttack()
    {
        if (Input.GetMouseButtonDown(1) && player.superBar.fillAmount >= 1)
        {
            photonView.RPC("SuperMeleeReload", RpcTarget.All);
            SwordSuperAnimation();
            Collider[] colliders = Physics.OverlapSphere(transform.position, 1, playerLayer);
            Array.Sort(colliders, new DistanceCompare(transform));
            foreach (var item in colliders)
            {
                if (item.gameObject.layer == 12 && ServerControl.server.nickName != item.gameObject.name)
                {
                    transform.LookAt(item.transform);
                    AttackSword(item, player.attack * 3, true);
                    break;
                }
                else if (item.gameObject.layer == 9)
                {
                    transform.LookAt(item.transform);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    AttackSwordPowerup(item, player.attack * 3);
                    break;
                }
            }
        }
    }
    void AndroidAttack()
    {
        FixedJoystick shootJoystick = UIManager.uIManager.shootJoystick;
        if ((Mathf.Abs(shootJoystick.Horizontal) > .5f || Mathf.Abs(shootJoystick.Vertical) > .5f) && time >= 1.1f && player.bulletBar.fillAmount >= (1 / maxBulletCount))
        {
            attackLookat.transform.position = new Vector3(shootJoystick.Vertical + transform.position.x, 13.25f, -shootJoystick.Horizontal + transform.position.z);
            line.gameObject.SetActive(true);
            photonView.RPC("LineActive", RpcTarget.All, attackLookat.transform.position);
            return;
        }
        else if (shoot && Input.GetMouseButtonUp(0) && time >= 1.1f && player.bulletBar.fillAmount >= (1 / maxBulletCount))
        {
            photonView.RPC("MeleeReload", RpcTarget.All);
            SwordAnimation();
            Collider[] colliders = Physics.OverlapSphere(transform.position, 1, playerLayer);
            Array.Sort(colliders, new DistanceCompare(transform));
            foreach (var item in colliders)
            {
                if (item.gameObject.layer == 12 && ServerControl.server.nickName != item.gameObject.name)
                {
                    transform.LookAt(item.transform);
                    AttackSword(item, player.attack, false);
                    return;
                }
                else if (item.gameObject.layer == 9)
                {
                    transform.LookAt(item.transform);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    AttackSwordPowerup(item, player.attack);
                    return;
                }
            }
            Vector3 linePos = attackText.transform.position + attackText.transform.forward * lineDistance;
            transform.LookAt(linePos);
        }
        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                shoot = false;
            }
        }
        else if (Mathf.Abs(shootJoystick.Horizontal) < .3f || Mathf.Abs(shootJoystick.Vertical) < .3f && line.gameObject.activeSelf)
        {
            line.gameObject.SetActive(false);
            shoot = false;
        }
        AndroidSuperAttack();
    }
    void AndroidSuperAttack()
    {
        FixedJoystick shootJoystick = UIManager.uIManager.superJoystick;
        if ((Mathf.Abs(shootJoystick.Horizontal) > .5f || Mathf.Abs(shootJoystick.Vertical) > .5f) && player.superBar.fillAmount >= 1)
        {
            attackLookat.transform.position = new Vector3(shootJoystick.Vertical + transform.position.x, 13.25f, -shootJoystick.Horizontal + transform.position.z);
            line.gameObject.SetActive(true);
            photonView.RPC("SuperLineActive", RpcTarget.All, attackLookat.transform.position);
        }
        else if (shootSuper && Input.GetMouseButtonUp(0) && player.superBar.fillAmount >= 1)
        {
            photonView.RPC("SuperMeleeReload", RpcTarget.All);
            SwordSuperAnimation();
            Collider[] colliders = Physics.OverlapSphere(transform.position, 1, playerLayer);
            Array.Sort(colliders, new DistanceCompare(transform));
            foreach (var item in colliders)
            {
                if (item.gameObject.layer == 12 && ServerControl.server.nickName != item.gameObject.name)
                {
                    transform.LookAt(item.transform);
                    AttackSword(item, player.attack * 3, true);
                    return;
                }
                else if (item.gameObject.layer == 9)
                {
                    transform.LookAt(item.transform);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    AttackSwordPowerup(item, player.attack * 3);
                    return;
                }
            }
            Vector3 linePos = attackText.transform.position + attackText.transform.forward * lineDistance;
            transform.LookAt(linePos);
        }
        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                shootSuper = false;
            }
        }
        else if (Mathf.Abs(shootJoystick.Horizontal) < .3f || Mathf.Abs(shootJoystick.Vertical) < .3f && line.gameObject.activeSelf)
        {
            line.gameObject.SetActive(false);
            shootSuper = false;
        }
    }
    [PunRPC]
    void LineActive(Vector3 pos)
    {
        attackText.transform.LookAt(new Vector3(pos.x, 1, pos.z));
        attackText.transform.eulerAngles = new Vector3(0, attackText.transform.eulerAngles.y, 0);
        line.SetPosition(0, attackText.transform.position);
        if (Physics.Raycast(attackText.transform.position + Vector3.up, attackText.transform.forward, out hit, lineDistance))
        {
            line.SetPosition(1, new Vector3(hit.point.x, 13.25f, hit.point.z));
        }
        else
        {
            Vector3 linePos = attackText.transform.position + attackText.transform.forward * lineDistance;
            line.SetPosition(1, linePos);
            line.SetPosition(1, new Vector3(line.GetPosition(1).x, 13.25f, line.GetPosition(1).z));
        }
        Debug.DrawRay(attackText.transform.position, attackText.transform.forward, Color.red, lineDistance);
        shoot = true;
    }
    [PunRPC]
    void SuperLineActive(Vector3 pos)
    {
        attackText.transform.LookAt(new Vector3(pos.x, 1, pos.z));
        attackText.transform.eulerAngles = new Vector3(0, attackText.transform.eulerAngles.y, 0);
        line.SetPosition(0, attackText.transform.position);
        if (Physics.Raycast(attackText.transform.position + Vector3.up, attackText.transform.forward, out hit, lineDistance))
        {
            line.SetPosition(1, new Vector3(hit.point.x, 13.25f, hit.point.z));
        }
        else
        {
            Vector3 linePos = attackText.transform.position + attackText.transform.forward * lineDistance;
            line.SetPosition(1, linePos);
            line.SetPosition(1, new Vector3(line.GetPosition(1).x, 13.25f, line.GetPosition(1).z));
        }
        Debug.DrawRay(attackText.transform.position, attackText.transform.forward, Color.red, lineDistance);
        shootSuper = true;
    }
    [PunRPC]
    void MeleeReload()
    {
        AudioSource.PlayClipAtPoint(attackSound, transform.position, .5f);
        shoot = false;
        player.reload = false;
        newAmount = player.bulletBar.fillAmount - (1 / maxBulletCount);
        player.bulletBar.DOFillAmount(newAmount, .25f).SetEase(Ease.Linear).OnComplete(() =>
        {
            player.reload = true;
        });
        player.healthTime = 0;
    }
    [PunRPC]
    void SuperMeleeReload()
    {
        AudioSource.PlayClipAtPoint(attackSound, transform.position, .5f);
        shootSuper = false;
        player.superReload = false;
        newAmount = 0;
        player.superBar.DOFillAmount(newAmount, .25f).SetEase(Ease.Linear).OnComplete(() =>
        {
            player.superReload = true;
        });
        player.healthTime = 0;
    }
    void SwordAnimation()
    {
        avatarAnim.SetTrigger("Attack");
        StartCoroutine(AnimationFinish());
    }
    void SwordSuperAnimation()
    {
        avatarAnim.SetTrigger("SuperAttack");
        StartCoroutine(AnimationFinish());
    }
    IEnumerator AnimationFinish()
    {
        player.move = true;
        time = 0;
        yield return new WaitForSeconds(animationFinish);
        player.move = false;
    }
    public void AttackSword(Collider other, float attack, bool super)
    {
        string otherName = other.gameObject.name;
        photonView.RPC("AttackMelee", RpcTarget.All, otherName, attack, super);
    }
    public void AttackSwordPowerup(Collider other, float attack)
    {
        string otherName = other.gameObject.name;
        photonView.RPC("PowerupAttackMelee", RpcTarget.All, otherName, attack);
    }
    [PunRPC]
    void AttackMelee(string otherName, float attack, bool super)
    {
        GameObject other = GameObject.Find(otherName);
        TextMeshProUGUI healthText = other.GetComponent<Player>().healthText;
        other.GetComponent<Player>().health -= attack;
        other.GetComponent<Player>().health = Mathf.Clamp(other.GetComponent<Player>().health, 0, other.GetComponent<Player>().maxHealth);
        other.GetComponent<Player>().healthReduceBar.color = Color.red;
        other.GetComponent<Player>().healthBar.DOFillAmount(other.GetComponent<Player>().health / other.GetComponent<Player>().maxHealth, .25f).
            SetEase(Ease.Linear).OnComplete(() =>
            {
                other.GetComponent<Player>().healthReduceBar.DOFillAmount(other.GetComponent<Player>().healthBar.fillAmount, .25f).SetEase(Ease.Linear);
            });
        healthText.text = ((int)other.GetComponent<Player>().health).ToString();
        other.GetComponent<Player>().healthTime = 0;
        int newAttack = (int)attack;
        ShowDamage(newAttack.ToString(), other.transform.position + Vector3.up, other.transform.rotation);
        if (!super)
        {
            GetComponent<Player>().superBar.DOFillAmount(GetComponent<Player>().superBar.fillAmount + .2f, 1).SetEase(Ease.Linear);
        }
        if (other.GetComponent<Player>().health == 0)
        {
            if (other.GetComponent<PhotonView>() != null && other.GetComponent<PhotonView>().IsMine == true)
            {
                UIManager.uIManager.leave.gameObject.SetActive(true);
                UIManager.uIManager.playersCam.gameObject.SetActive(true);
                UIManager.uIManager.playersCam.GetComponentInChildren<TextMeshProUGUI>().text = gameObject.GetPhotonView().Owner.NickName;
                Camera.main.GetComponent<CameraFollow>().target = gameObject.transform;
                gameObject.transform.GetChild(0).localRotation = Quaternion.identity;
            }
            if (gameObject.GetComponent<PhotonView>().IsMine == true)
            {
                ServerControl.server.killCount++;
                UIManager.uIManager.killCountText.text = "Kill: " + ServerControl.server.killCount.ToString();
            }
            UIManager.uIManager.killImage.SetActive(true);
            UIManager.uIManager.killInfo.text = gameObject.GetPhotonView().Owner.NickName;
            if (other.GetComponent<PhotonView>() != null)
            {
                UIManager.uIManager.deathInfo.text = other.gameObject.GetPhotonView().Owner.NickName;
            }
            else
            {
                UIManager.uIManager.deathInfo.text = other.gameObject.name.Substring(0, other.gameObject.name.Length - 5);
            }
            if (other.GetComponent<Player>().powerCount == 0)
            {
                other.GetComponent<Player>().power[0].transform.parent = null;
                other.GetComponent<Player>().power[0].SetActive(true);
            }
            for (int i = 0; i < other.GetComponent<Player>().powerCount; i++)
            {
                other.GetComponent<Player>().power[i].transform.parent = null;
                other.GetComponent<Player>().power[i].SetActive(true);
            }
            if (other.GetComponent<Player>().newPlayer != null)
            {
                Destroy(other.GetComponent<Player>().newPlayer);
            }
            Destroy(other.gameObject);
        }
    }
    [PunRPC]
    void PowerupAttackMelee(string otherName, float attack)
    {
        GameObject other = GameObject.Find(otherName).gameObject;
        GameObject canvas = other.transform.GetChild(0).gameObject;
        TextMeshProUGUI powerHealthText = other.GetComponentInChildren<TextMeshProUGUI>();
        Image powerReduceBar = canvas.transform.GetChild(0).GetComponent<Image>();
        Image powerBar = canvas.transform.GetChild(1).GetComponent<Image>();
        float powerHealth = float.Parse(powerHealthText.text);
        if (powerHealth <= 0)
        {
            return;
        }
        powerHealth -= attack;
        powerHealth = powerHealth <= 0 ? 0 : powerHealth;
        powerHealthText.text = powerHealth.ToString();
        powerBar.DOFillAmount(powerHealth / 1000, .1f).
            SetEase(Ease.Linear).OnComplete(() =>
            {
                powerReduceBar.DOFillAmount(powerBar.fillAmount, .25f).SetEase(Ease.Linear);
            });
        if (powerHealth == 0)
        {
            other.transform.GetChild(other.transform.childCount - 1).DORotate(new Vector3(27, 0, 0), .5f).SetEase(Ease.Linear);
            other.transform.GetChild(other.transform.childCount - 2).DORotate(new Vector3(27, 90, 270), .5f).SetEase(Ease.Linear);
            other.transform.GetChild(other.transform.childCount - 3).DORotate(new Vector3(27, 180, 180), .5f).SetEase(Ease.Linear);
            other.transform.GetChild(other.transform.childCount - 4).DORotate(new Vector3(27, 270, 90), .5f).SetEase(Ease.Linear);
            other.GetComponent<BoxCollider>().isTrigger = true;
            other.gameObject.layer = 0;
            int randomId = 1;
            other.transform.GetChild(randomId).gameObject.SetActive(true);
        }
        int newAttack = (int)attack;
        ShowDamage(newAttack.ToString(), other.transform.position + Vector3.up, Quaternion.Euler(0, 90, 0));
    }
    void ShowDamage(string text, Vector3 pos, Quaternion rotation)
    {
        GameObject prefab = Instantiate(UIManager.uIManager.damagePopup, pos, rotation);
        prefab.GetComponentInChildren<TextMesh>().text = text;
    }
}

