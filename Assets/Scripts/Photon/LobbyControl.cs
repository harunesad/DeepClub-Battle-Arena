using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System;
using System.Linq;

public class LobbyControl : MonoBehaviour
{
    PhotonView photonView;
    public int chooseChar;
    public bool ready, leave;
    public float time = 5;
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    void Start()
    {
        chooseChar = -1;
        for (int i = 0; i < UIManager.uIManager.playersNicknames.Count; i++)
        {
            UIManager.uIManager.playersNicknames[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < UIManager.uIManager.playersCharacter.Count; i++)
        {
            UIManager.uIManager.playersCharacter[i].gameObject.SetActive(true);
        }
        UIManager.uIManager.waitBackground.gameObject.SetActive(true);
        if (photonView.IsMine)
        {
            UIManager.uIManager.time.text = time.ToString();
            UIManager.uIManager.time.transform.parent.gameObject.SetActive(true);
        }
    }
    [PunRPC]
    void ChooseCharacter(int chooseChar, string myName)
    {
        gameObject.name = myName;
        this.chooseChar = chooseChar;
    }
    void Update()
    {
        CharacterShowing();
        TimeStart();
        TimeUpdate();
    }
    [PunRPC]
    void TimeReduce(float newTime)
    {
        UIManager.uIManager.time.text = ((int)newTime).ToString();
    }
    void CharacterShowing()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("ChooseCharacter", RpcTarget.All, ServerControl.server.chooseChar, ServerControl.server.nickName);
            for (int i = 0; i < UIManager.uIManager.playersNicknames.Count; i++)
            {
                UIManager.uIManager.playersNicknames[0].text = ServerControl.server.player.name;
                UIManager.uIManager.playersCharacter[0].gameObject.SetActive(true);
                UIManager.uIManager.playersCharacter[0].GetComponent<Image>().sprite = UIManager.uIManager.characterImages[ServerControl.server.player.GetComponent<LobbyControl>().chooseChar];
                if (i != 0)
                {
                    GameObject player = null;
                    if (i <= PhotonNetwork.PlayerListOthers.Count())
                    {
                        player = GameObject.Find(PhotonNetwork.PlayerListOthers[i - 1].NickName);
                    }
                    if (player != null)
                    {
                        UIManager.uIManager.playersNicknames[i].text = player.name;
                        UIManager.uIManager.playersCharacter[i].gameObject.SetActive(true);
                        UIManager.uIManager.playersCharacter[i].GetComponent<Image>().sprite = UIManager.uIManager.characterImages[player.GetComponent<LobbyControl>().chooseChar];
                    }
                    else
                    {
                        UIManager.uIManager.playersNicknames[i].text = "";
                        UIManager.uIManager.playersCharacter[i].gameObject.SetActive(false);
                    }
                }
            }
        }
    }
    void TimeStart()
    {
        if (PhotonNetwork.PlayerList.Length == 4 && !ready)
        {
            UIManager.uIManager.time.text = ((int)time).ToString();
            ready = true;
        }
    }
    void TimeUpdate()
    {
        if (ready)
        {
            if (UIManager.uIManager.time.text != "" && Convert.ToInt32(UIManager.uIManager.time.text) != 0)
            {
                if (photonView.IsMine && PhotonNetwork.IsMasterClient && GameObject.FindGameObjectsWithTag("View").Length == 4)
                {
                    time -= Time.deltaTime;
                    time = Mathf.Clamp(time, 0, 5);
                    photonView.RPC("TimeReduce", RpcTarget.All, time);
                }
                else if (GameObject.FindGameObjectsWithTag("View").Length != 4)
                {
                    time = 5;
                    UIManager.uIManager.time.text = ((int)time).ToString();
                    //ready = false;
                }
            }
            else if (UIManager.uIManager.time.text != "" && Convert.ToInt32(UIManager.uIManager.time.text) == 0)
            {
                if (ServerControl.server.chooseChar == -1)
                {
                    ServerControl.server.chooseChar = 0;
                }
                UIManager.uIManager.time.text = "";
                UIManager.uIManager.loadingSource.Play();
                UIManager.uIManager.LoadingStart(UIManager.uIManager.gameLoading);
                Exit();
            }
        }
    }
    void Exit()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.Log("Photon aðýna baðlý deðil veya odada deðil.");
            ServerControl.server.Rejoin();
        }
        else
        {
            PhotonNetwork.LeaveRoom();
        }
        Debug.Log("Ýstemci Durumu: " + PhotonNetwork.NetworkClientState + gameObject.GetPhotonView().ViewID);
    }
}
