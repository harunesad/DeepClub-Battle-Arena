using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;
using DG.Tweening;
using Photon.Realtime;
using System.Collections;

public class Player : MonoBehaviour
{
    PhotonView photonView;
    Animator avatarAnim;
    public float health = 100, moveSpeed, attack, maxHealth;
    public int powerCount;
    public TextMeshProUGUI healthText, nickNameText, powerText;
    public Image healthBar, bulletBar, healthReduceBar, powerImage, superBar;
    float inputX;
    float inputZ;
    Vector3 movement;
    public GameObject canvas;
    public List<GameObject> power;
    public float poisonTime;
    public float healthTime, healthInc = 10, healthIncTime;
    bool nameFind, pressed = true;
    public bool poison;
    public bool reload, superReload;
    public bool move;
    public float turnSmoothTime = .5f, turnSmoothVelocity;
    public LayerMask hideLayer;
    public List<GameObject> meshes;
    public bool inGrass, wall;
    public GameObject newPlayer;
    public float reloadTime;
    public AudioClip collect;
    Player newPlayerProperty;
    private void Awake()
    {
        canvas.transform.parent = null;
        avatarAnim = GetComponent<Animator>();
        if (gameObject.GetPhotonView() != null)
        {
            newPlayer.transform.parent = null;
            newPlayerProperty = newPlayer.GetComponent<Player>();
            newPlayerProperty.canvas.SetActive(false);
            newPlayerProperty.canvas.transform.parent = null;
            photonView = GetComponent<PhotonView>();
            for (int i = 0; i < ServerControl.server.newPlayers.Count; i++)
            {
                if (ServerControl.server.newPlayers[i] == null)
                {
                    ServerControl.server.newPlayers[i] = newPlayer;
                    break;
                }
            }
        }
    }
    private void Start()
    {
        if (gameObject.GetPhotonView() == null)
        {
            return;
        }
        if (photonView.IsMine)
        {
            avatarAnim.SetBool("Idle", true);
            Camera.main.GetComponent<CameraFollow>().target = transform;
            if (transform.position.z < 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            nickNameText.color = Color.green;
            MyName(ServerControl.server.nickName, health);
        }
    }
    [PunRPC]
    void MyName(string myName, float health)
    {
        nickNameText.text = myName;
        gameObject.name = myName;
        healthText.text = health.ToString();
        newPlayer.name = myName + "Clone";
    }
    private void Update()
    {
        if (photonView == null && GameObject.FindGameObjectsWithTag("Player").Length == 1)
        {
            Destroy(gameObject);
        }
        PoisonActive();
        Healing();
        if (bulletBar.fillAmount < 1 && reload)
        {
            ReloadBar();
        }
        if (gameObject.GetPhotonView() == null)
        {
            return;
        }
        NewPlayerActive();
        if (photonView.IsMine)
        {
            if (GameObject.FindGameObjectsWithTag("Player").Length == 4 && !nameFind)
            {
                photonView.RPC("MyName", RpcTarget.All, ServerControl.server.nickName, health);
                UIManager.uIManager.LoadingComplete(UIManager.uIManager.gameLoading);
                nameFind = true;
            }
            else if (GameObject.FindGameObjectsWithTag("Player").Length != 4 && !nameFind)
            {
                return;
            }
            WebMove();
            AndroidMove();
        }
    }
    //private void OnDrawGizmos()
    //{
    //    if (gameObject.GetPhotonView() == null)
    //    {
    //        return;
    //    }
    //    if (photonView.IsMine && attackType == Attack.Wizard)
    //    {
    //        Gizmos.DrawWireSphere(transform.position, 5);
    //    }
    //    if (photonView.IsMine && attackType == Attack.Melee)
    //    {
    //        Gizmos.DrawWireSphere(transform.position, 2);
    //    }
    //}
    private void OnTriggerEnter(Collider other)
    {
        if (gameObject.GetPhotonView() == null)
        {
            return;
        }
        if (photonView.IsMine)
        {
            PowerForce powerForce = other.GetComponent<PowerForce>();
            if (other.gameObject.layer == 10 && (powerForce == null || powerForce.collectable))
            {
                string otherName = other.gameObject.name;
                photonView.RPC("Power", RpcTarget.All, otherName);
                //photonView.RPC("PowerHealth", RpcTarget.All, otherName);
            }
            if (other.gameObject.layer == 19)
            {
                string otherName = other.gameObject.name;
                photonView.RPC("TokenCollect", RpcTarget.All, otherName);
                TokenIncrease();
            }
            if (other.gameObject.layer == 14)
            {
                photonView.RPC("Hide", RpcTarget.Others);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            poisonTime = 0;
            poison = false;
        }
        if (gameObject.GetPhotonView() == null)
        {
            return;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            poison = true;
        }
        if (gameObject.GetPhotonView() == null)
        {
            return;
        }
        if (photonView.IsMine)
        {
            if (other.gameObject.layer == 14)
            {
                //int objView = gameObject.GetPhotonView().ViewID;
                photonView.RPC("Show", RpcTarget.Others);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine && !collision.gameObject.CompareTag("Ground"))
        {
            wall = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (photonView.IsMine && !collision.gameObject.CompareTag("Ground"))
        {
            StartCoroutine(NotWall());
        }
    }
    IEnumerator NotWall()
    {
        yield return new WaitForSeconds(.15f);
        wall = false;
    }
    void PoisonActive()
    {
        if (poison)
        {
            healthTime = 0;
            if (poisonTime <= 0)
            {
                ParticleSystem lightning = Instantiate(ServerControl.server.lightning, transform.position + Vector3.up * 3, ServerControl.server.lightning.transform.rotation);
                var mainModule = lightning.main;
                mainModule.startColor = Color.green;
                Injury();
            }
            poisonTime += Time.deltaTime;
            if (poisonTime >= 3)
            {
                poisonTime = 0;
            }
        }
    }
    void Healing()
    {
        if (healthTime <= 4 && health < maxHealth)
        {
            healthTime += Time.deltaTime;
        }
        else if (healthTime >= 4)
        {
            HealthRegeneration(healthInc);
        }
    }
    void NewPlayerActive()
    {
        newPlayerProperty.healthText.text = health.ToString();
        newPlayerProperty.health = health;
        newPlayerProperty.maxHealth = maxHealth;
        newPlayerProperty.healthBar.fillAmount = health / maxHealth;
        newPlayerProperty.healthReduceBar.fillAmount = health / maxHealth;
        newPlayerProperty.bulletBar.fillAmount = bulletBar.fillAmount;
        newPlayerProperty.powerImage.gameObject.SetActive(powerImage.gameObject.activeSelf);
        newPlayerProperty.powerText.text = powerText.text;
        newPlayer.transform.position = Vector3.Lerp(newPlayer.transform.position, transform.position, 100);
        newPlayerProperty.canvas.transform.position = Vector3.Lerp(newPlayerProperty.canvas.transform.position, transform.position + Vector3.up * 2, 100);
        newPlayer.transform.rotation = transform.rotation;
    }
    void WebMove()
    {
#if UNITY_WEBGL
            inputX = Input.GetAxisRaw("Horizontal");
            inputZ = Input.GetAxisRaw("Vertical");
            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D) ||
                Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
                pressed = true;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
                Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
                pressed = false;
            if (wall)
            {
                inputX = -inputX;
                inputZ = -inputZ;
            }
            Vector3 direction = new Vector3(inputZ, 0, -inputX).normalized;
            if (direction.magnitude >= .1f)
            {
                float targetAngle = (float)Math.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }
            if (!pressed && !move)
            {
                movement = Vector3.forward;
                transform.Translate(movement * moveSpeed * Time.deltaTime);
                avatarAnim.SetBool("Walk", true);
            }
            else
            {
                avatarAnim.SetBool("Walk", false);
            }
#endif
    }
    void AndroidMove()
    {
#if UNITY_ANDROID || UNITY_IOS
        FloatingJoystick moveJoystick = UIManager.uIManager.moveJoystick;
        bool active = moveJoystick.transform.GetChild(0).gameObject.activeSelf;
        if (active && !move && (moveJoystick.Horizontal > 0 || moveJoystick.Horizontal < 0 || moveJoystick.Vertical > 0 || moveJoystick.Vertical < 0))
        {
            float horizontal = moveJoystick.Horizontal;
            float vertical = moveJoystick.Vertical;
            if (wall)
            {
                horizontal = -horizontal;
                vertical = -vertical;
            }
            Vector3 pos = new Vector3(vertical + transform.position.x, 0, -horizontal + transform.position.z);
            transform.LookAt(new Vector3(pos.x, 0, pos.z));
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            movement = Vector3.forward;
            transform.Translate(movement * moveSpeed * Time.deltaTime);
            avatarAnim.SetBool("Walk", true);

        }
        else
        {
            avatarAnim.SetBool("Walk", false);
        }
#endif
    }
    [PunRPC]
    void Hide()
    {
        //GameObject obj = PhotonView.Find(objView).gameObject;
        for (int i = 0; i < meshes.Count; i++)
        {
            //Color color = obj.GetComponent<Avatar>().meshes[i].GetComponent<SkinnedMeshRenderer>().materials[0].color;
            //obj.GetComponent<Avatar>().meshes[i].GetComponent<SkinnedMeshRenderer>().materials[0].DOColor(new Color(color.r, color.g, color.b, 0), .25f).SetEase(Ease.Linear);
            meshes[i].SetActive(false);
            inGrass = true;
        }
        canvas.SetActive(false);
    }
    [PunRPC]
    void Show()
    {
        //GameObject obj = PhotonView.Find(objView).gameObject;
        for (int i = 0; i < meshes.Count; i++)
        {
            //Color color = obj.GetComponent<Avatar>().meshes[i].GetComponent<SkinnedMeshRenderer>().materials[0].color;
            //obj.GetComponent<Avatar>().meshes[i].GetComponent<SkinnedMeshRenderer>().materials[0].DOColor(new Color(color.r, color.g, color.b, 1), .25f).SetEase(Ease.Linear);
            meshes[i].SetActive(true);
            inGrass = false;
        }
        canvas.SetActive(true);
    }
    [PunRPC]
    void Power(string otherName)
    {
        attack += 50;
        health += 400;
        maxHealth += 400;
        healthText.text = ((int)health).ToString();
        healthReduceBar.color = Color.blue;
        healthReduceBar.DOFillAmount(health / maxHealth, .25f).
            SetEase(Ease.Linear).OnComplete(() =>
            {
                healthBar.DOFillAmount(healthReduceBar.fillAmount, .25f).SetEase(Ease.Linear);
            });
        GameObject other = GameObject.Find(otherName).gameObject;
        other.gameObject.SetActive(false);
        powerCount++;
        powerText.text = powerCount.ToString();
        powerImage.gameObject.SetActive(true);
        AudioSource.PlayClipAtPoint(collect, transform.position, .5f);
    }
    [PunRPC]
    void TokenCollect(string otherName)
    {
        GameObject other = GameObject.Find(otherName).gameObject;
        other.gameObject.SetActive(false);
        AudioSource.PlayClipAtPoint(collect, transform.position, .5f);
    }
    void TokenIncrease()
    {
        if (ServerControl.server.wallet)
        {
            ServerControl.server.tokenCount++;
        }
        else
        {
            ServerControl.server.coinCount++;
        }
    }
    //[PunRPC]
    void ReloadBar()
    {
        if (bulletBar.fillAmount <= 1)
        {
            bulletBar.fillAmount += (Time.deltaTime / 15) * reloadTime;
        }
    }
    //[PunRPC]
    void HealthRegeneration(float healthInc)
    {
        if (healthIncTime < 1)
        {
            healthIncTime += Time.deltaTime;
        }
        else
        {
            healthIncTime = 0;
            health += healthInc;
            health = health > maxHealth ? maxHealth : health;
            healthText.text = ((int)health).ToString();
            healthReduceBar.color = Color.blue;
            healthReduceBar.DOFillAmount(health / maxHealth, .25f).
                SetEase(Ease.Linear).OnComplete(() =>
                {
                    healthBar.DOFillAmount(healthReduceBar.fillAmount, .25f).SetEase(Ease.Linear);
                });
        }
    }
    //[PunRPC]
    void Injury()
    {
        health -= 100;
        health = health < 0 ? 0 : health;
        healthReduceBar.color = Color.red;
        healthBar.DOFillAmount(health / maxHealth, .25f).
            SetEase(Ease.Linear).OnComplete(() =>
            {
                healthReduceBar.DOFillAmount(healthBar.fillAmount, .25f).SetEase(Ease.Linear);
            });
        healthText.text = ((int)health).ToString();
        if (health == 0)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                UIManager.uIManager.leave.gameObject.SetActive(true);
                UIManager.uIManager.playersCam.gameObject.SetActive(true);
                for (int i = 0; i < PhotonNetwork.PlayerListOthers.Count(); i++)
                {
                    if (PhotonNetwork.PlayerListOthers[i] != null && GameObject.Find(PhotonNetwork.PlayerListOthers[i].NickName) != null)
                    {
                        UIManager.uIManager.playersCam.GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.PlayerListOthers[i].NickName;
                        GameObject otherObj = GameObject.Find(PhotonNetwork.PlayerListOthers[i].NickName);
                        Camera.main.GetComponent<CameraFollow>().target = otherObj.transform;
                        otherObj.transform.GetChild(0).localRotation = Quaternion.identity;
                        break;
                    }
                }
            }
            UIManager.uIManager.killImage.SetActive(true);
            UIManager.uIManager.killInfo.text = "Electric";
            if (GetComponent<PhotonView>() != null)
            {
                UIManager.uIManager.deathInfo.text = gameObject.GetPhotonView().Owner.NickName;
            }
            else
            {
                UIManager.uIManager.deathInfo.text = gameObject.name.Substring(0, gameObject.name.Length - 5);
            }
            if (GetComponent<Player>().powerCount == 0)
            {
                GetComponent<Player>().power[0].transform.parent = null;
                GetComponent<Player>().power[0].SetActive(true);
            }
            for (int i = 0; i < GetComponent<Player>().powerCount; i++)
            {
                GetComponent<Player>().power[i].transform.parent = null;
                GetComponent<Player>().power[i].SetActive(true);
            }
            Destroy(GetComponent<Player>().newPlayer);
            Destroy(gameObject);
        }
    }
}
