using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Bullet : MonoBehaviourPun
{
    PhotonView photonView;
    public float speed = 10f;
    public GameObject parent;
    public float attack, oldAttack;
    public Vector3 parentPos;
    float distance;
    public float range;
    Vector3 destroyPos;
    public bool super;
    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        destroyPos = transform.position + transform.forward * range;
    }
    void Update()
    {
        if (!photonView.IsMine)
            return;
        RangeBullet();
    }
    void RangeBullet()
    {
        if (parent.GetComponent<PlayerAttackRange>() != null)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            if (transform.position == destroyPos)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 0)
        {
            if (parent.GetComponent<PlayerAttackWizard>() != null)
            {
                Instantiate(ServerControl.server.lightning, transform.position + Vector3.up * 3, ServerControl.server.lightning.transform.rotation);
            }
            Destroy(gameObject);
        }
        if (photonView.IsMine)
        {
            if (other.CompareTag("Player") && other.gameObject != parent)
            {
                string otherName = other.gameObject.name;
                photonView.RPC("Attack", RpcTarget.All, otherName, super);
                //int otherId = other.gameObject.GetPhotonView().ViewID;
                //photonView.RPC("Attack", RpcTarget.All, otherId);
                PhotonNetwork.Destroy(gameObject);
            }
            if (other.gameObject.layer == 9)
            {
                string otherName = other.gameObject.name;
                photonView.RPC("PowerupAttack", RpcTarget.All, otherName);
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
    [PunRPC]
    void Attack(string otherName, bool super)
    {
        GameObject other = GameObject.Find(otherName);
        if (parent.GetComponent<PlayerAttackRange>() != null)
        {
            distance = Vector3.Distance(parentPos, other.transform.position);
            if (distance < range)
            {
                float attackMultiply = attack / range;
                attack = distance * attackMultiply;
            }
        }
        else if (parent.GetComponent<PlayerAttackWizard>() != null)
        {
            var lightning = Instantiate(ServerControl.server.lightning, transform.position + Vector3.up * 3, ServerControl.server.lightning.transform.rotation);
            if (attack != oldAttack)
            {
                lightning.transform.localScale *= 3;
            }
        }
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
            parent.GetComponent<Player>().superBar.DOFillAmount(parent.GetComponent<Player>().superBar.fillAmount + .2f, 1).SetEase(Ease.Linear);
        }
        if (other.GetComponent<Player>().health == 0)
        {
            if (other.GetComponent<PhotonView>() != null && other.GetComponent<PhotonView>().IsMine == true)
            {
                UIManager.uIManager.leave.gameObject.SetActive(true);
                UIManager.uIManager.playersCam.gameObject.SetActive(true);
                UIManager.uIManager.playersCam.GetComponentInChildren<TextMeshProUGUI>().text = parent.GetPhotonView().Owner.NickName;
                Camera.main.GetComponent<CameraFollow>().target = parent.transform;
                parent.transform.GetChild(0).localRotation = Quaternion.identity;
            }
            if (parent.GetComponent<PhotonView>().IsMine == true)
            {
                ServerControl.server.killCount++;
                UIManager.uIManager.killCountText.text = "Kill: " + ServerControl.server.killCount.ToString();
            }
            UIManager.uIManager.killImage.SetActive(true);
            UIManager.uIManager.killInfo.text = parent.GetPhotonView().Owner.NickName;
            if (other.GetComponent<PhotonView>() != null)
            {
                UIManager.uIManager.deathInfo.text = other.gameObject.GetPhotonView().Owner.NickName;
            }
            else
            {
                UIManager.uIManager.deathInfo.text = other.gameObject.name.Substring(0, other.gameObject.name.Length - 5);
            }
            Debug.Log(other.GetComponent<Player>().health);
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
    void PowerupAttack(string otherName)
    {
        GameObject other = GameObject.Find(otherName).gameObject;
        if (parent.GetComponent<PlayerAttackRange>() != null)
        {
            distance = Vector3.Distance(parentPos, other.transform.position);
            if (distance < range)
            {
                float attackMultiply = attack / range;
                attack = distance * attackMultiply;
            }
        }
        else if (parent.GetComponent<PlayerAttackWizard>() != null)
        {
            var lightning = Instantiate(ServerControl.server.lightning, transform.position + Vector3.up * 3 + Vector3.left, ServerControl.server.lightning.transform.rotation);
            if (attack != oldAttack)
            {
                lightning.transform.localScale *= 3;
            }
        }
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
        powerHealthText.text = ((int)powerHealth).ToString();
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
