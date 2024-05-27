using DG.Tweening;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackRange : MonoBehaviour
{
    PhotonView photonView;
    Animator avatarAnim;
    Player player;
    float time, newAmount;
    bool nameFind;
    public float animationFinish, maxBulletCount;
    public Transform firePoint;
    GameObject bullet;
    public GameObject bulletPrefab;
    [SerializeField] LineRenderer line;
    public GameObject attackText, attackLookat;
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
            if (Input.GetMouseButtonDown(0) && time >= 1.1f && player.bulletBar.fillAmount >= (1 / maxBulletCount))
            {
                SpawnBullet();
                return;
            }
            if (Input.GetMouseButtonDown(1) && player.superBar.fillAmount >= 1)
            {
                SpawnSuperBullet();
            }
#endif
#if UNITY_ANDROID || UNITY_IOS
            AndroidAttack();
#endif
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
            Vector3 linePos = attackText.transform.position + attackText.transform.forward * lineDistance;
            transform.LookAt(linePos);
            SpawnBullet();
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
        else if (shootSuper && Input.GetMouseButtonUp(0) && time >= 1.1f && player.bulletBar.fillAmount >= (1 / maxBulletCount))
        {
            Vector3 linePos = attackText.transform.position + attackText.transform.forward * lineDistance;
            transform.LookAt(linePos);
            SpawnSuperBullet();
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
            Debug.Log(hit.transform.name);
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
    void SpawnBullet()
    {
        avatarAnim.SetTrigger("Attack");
        StartCoroutine(AttackBullet());
    }
    void SpawnSuperBullet()
    {
        avatarAnim.SetTrigger("Attack");
        StartCoroutine(AttackSuperBullet());
    }
    IEnumerator AttackBullet()
    {
        player.move = true;
        time = 0;
        yield return new WaitForSeconds(animationFinish);
        bullet = PhotonNetwork.Instantiate(bulletPrefab.name, firePoint.position, firePoint.rotation);
        int bulletId = bullet.GetPhotonView().ViewID;
        photonView.RPC("BulletAttack", RpcTarget.All, bulletId, player.attack);
        player.move = false;
    }
    IEnumerator AttackSuperBullet()
    {
        player.move = true;
        yield return new WaitForSeconds(animationFinish);
        bullet = PhotonNetwork.Instantiate(bulletPrefab.name, firePoint.position, firePoint.rotation);
        int bulletId = bullet.GetPhotonView().ViewID;
        photonView.RPC("BulletSuperAttack", RpcTarget.All, bulletId, player.attack);
        player.move = false;
    }
    [PunRPC]
    void BulletAttack(int bulletId, float attack)
    {
        AudioSource.PlayClipAtPoint(attackSound, transform.position, ServerControl.server.effectSound);
        shoot = false;
        player.healthTime = 0;
        GameObject bullet = PhotonView.Find(bulletId).gameObject;
        bullet.GetComponent<Bullet>().attack = attack;
        bullet.GetComponent<Bullet>().parentPos = transform.position;
        bullet.GetComponent<Bullet>().super = false;
        player.reload = false;
        newAmount = player.bulletBar.fillAmount - (1 / maxBulletCount);
        player.bulletBar.DOFillAmount(newAmount, .25f).SetEase(Ease.Linear).OnComplete(() =>
        {
            player.reload = true;
        });
        bullet.GetComponent<Bullet>().parent = player.gameObject;
    }
    [PunRPC]
    void BulletSuperAttack(int bulletId, float attack)
    {
        AudioSource.PlayClipAtPoint(attackSound, transform.position, ServerControl.server.effectSound);
        shootSuper = false;
        player.healthTime = 0;
        GameObject bullet = PhotonView.Find(bulletId).gameObject;
        bullet.transform.localScale *= 3;
        bullet.GetComponent<Bullet>().attack = attack * 3;
        bullet.GetComponent<Bullet>().parentPos = transform.position;
        bullet.GetComponent<Bullet>().super = true;
        player.superReload = false;
        newAmount = 0;
        player.superBar.DOFillAmount(newAmount, .25f).SetEase(Ease.Linear).OnComplete(() =>
        {
            player.superReload = true;
        });
        bullet.GetComponent<Bullet>().parent = player.gameObject;
    }
}
