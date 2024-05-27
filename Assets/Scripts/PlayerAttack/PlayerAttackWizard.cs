using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackWizard : MonoBehaviour
{
    PhotonView photonView;
    Animator avatarAnim;
    Player player;
    float time, newAmount;
    public float animationFinish, maxBulletCount;
    public Transform firePoint;
    bool nameFind;
    GameObject bullet;
    public GameObject bulletPrefab;
    [SerializeField] LayerMask playerLayer;
    public GameObject attackLookat;
    [SerializeField] float lineDistance;
    [SerializeField] AudioClip attackSound;
    bool shoot, shootSuper;
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        avatarAnim = GetComponent<Animator>();
        player = GetComponent<Player>();
        attackLookat.transform.parent = null;
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
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5, playerLayer);
            Array.Sort(colliders, new DistanceCompare(transform));
            foreach (var item in colliders)
            {
                if (item.gameObject.layer == 12 && ServerControl.server.nickName != item.gameObject.name && item.GetComponent<Player>().meshes[0].activeSelf)
                {
                    Debug.Log(item.gameObject.name);
                    transform.LookAt(item.transform);
                    firePoint.position = item.transform.position + Vector3.up * 3;
                    SpawnBullet();
                    break;
                }
                else if (item.gameObject.layer == 9)
                {
                    Debug.Log(item.gameObject.name);
                    transform.LookAt(item.transform);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    firePoint.position = item.transform.position + Vector3.up * 3;
                    SpawnBullet();
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
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5, playerLayer);
            Array.Sort(colliders, new DistanceCompare(transform));
            foreach (var item in colliders)
            {
                if (item.gameObject.layer == 12 && ServerControl.server.nickName != item.gameObject.name && item.GetComponent<Player>().meshes[0].activeSelf)
                {
                    transform.LookAt(item.transform);
                    firePoint.position = item.transform.position + Vector3.up * 3;
                    SpawnSuperBullet();
                    break;
                }
                else if (item.gameObject.layer == 9)
                {
                    transform.LookAt(item.transform);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    firePoint.position = item.transform.position + Vector3.up * 3;
                    SpawnSuperBullet();
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
            attackLookat.GetComponent<SpriteRenderer>().enabled = true;
            attackLookat.transform.position = new Vector3((shootJoystick.Vertical * lineDistance) + transform.position.x, 13.35f, (-shootJoystick.Horizontal * lineDistance) + transform.position.z);
            shoot = true;
            return;
        }
        else if (shoot && Input.GetMouseButtonUp(0) && time >= 1.1f && player.bulletBar.fillAmount >= (1 / maxBulletCount))
        {
            transform.LookAt(new Vector3(attackLookat.transform.position.x, transform.position.y, attackLookat.transform.position.z));
            firePoint.position = attackLookat.transform.position + Vector3.up * 3;
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
        else if (Mathf.Abs(shootJoystick.Horizontal) < .1f || Mathf.Abs(shootJoystick.Vertical) < .1f && attackLookat.gameObject.activeSelf)
        {
            attackLookat.GetComponent<SpriteRenderer>().enabled = false;
            shoot = false;
        }
        AndroidSuperAttack();
    }
    void AndroidSuperAttack()
    {
        FixedJoystick shootJoystick = UIManager.uIManager.superJoystick;
        if ((Mathf.Abs(shootJoystick.Horizontal) > .5f || Mathf.Abs(shootJoystick.Vertical) > .5f) && player.superBar.fillAmount >= 1)
        {
            attackLookat.GetComponent<SpriteRenderer>().enabled = true;
            attackLookat.transform.position = new Vector3((shootJoystick.Vertical * lineDistance) + transform.position.x, 13.35f, (-shootJoystick.Horizontal * lineDistance) + transform.position.z);
            shootSuper = true;
        }
        else if (shootSuper && Input.GetMouseButtonUp(0) && player.superBar.fillAmount >= 1)
        {
            transform.LookAt(new Vector3(attackLookat.transform.position.x, transform.position.y, attackLookat.transform.position.z));
            firePoint.position = attackLookat.transform.position + Vector3.up * 3;
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
        else if (Mathf.Abs(shootJoystick.Horizontal) < .1f || Mathf.Abs(shootJoystick.Vertical) < .1f && attackLookat.gameObject.activeSelf)
        {
            attackLookat.GetComponent<SpriteRenderer>().enabled = false;
            shootSuper = false;
        }
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
        string parentName = gameObject.name;
        photonView.RPC("BulletAttack", RpcTarget.All, bulletId, player.attack);
        player.move = false;
    }
    IEnumerator AttackSuperBullet()
    {
        player.move = true;
        yield return new WaitForSeconds(animationFinish);
        bullet = PhotonNetwork.Instantiate(bulletPrefab.name, firePoint.position, firePoint.rotation);
        int bulletId = bullet.GetPhotonView().ViewID;
        string parentName = gameObject.name;
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
        Debug.Log(bullet.name);
        bullet.GetComponent<Bullet>().attack = attack;
        bullet.GetComponent<Bullet>().oldAttack = attack;
        bullet.GetComponent<Bullet>().super = true;
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
        bullet.GetComponent<Bullet>().attack = attack * 3;
        bullet.GetComponent<Bullet>().oldAttack = attack;
        bullet.GetComponent<Bullet>().parentPos = transform.position;
        bullet.GetComponent<Bullet>().super = false;
        player.superReload = false;
        newAmount = 0;
        player.superBar.DOFillAmount(newAmount, .25f).SetEase(Ease.Linear).OnComplete(() =>
        {
            player.superReload = true;
        });
        bullet.GetComponent<Bullet>().parent = player.gameObject;
    }
}
