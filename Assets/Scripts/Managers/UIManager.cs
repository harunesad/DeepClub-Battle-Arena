using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using DG.Tweening;
using UnityEngine.AI;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    public static UIManager uIManager;
    public TextMeshProUGUI nicknameText, playerCount;
    public List<Button> choose, mods;
    public List<TextMeshProUGUI> playersNicknames;
    public List<Image> playersCharacter;
    public Button play, settings, character;
    public Button leave, playersCam, mainHome, home, playGame, survivor, multiplayer;
    public TextMeshProUGUI killInfo, deathInfo, killCountText, win, time, coin, collect;
    public GameObject killImage, damagePopup, settingsPanel;
    public Image gameBeforeLoading, gameLoading;
    public List<Sprite> characterImages;
    public FloatingJoystick moveJoystick;
    public FixedJoystick shootJoystick, superJoystick;
    [SerializeField] GameObject canvas, modBg;
    [SerializeField] Image gameBackground, charactersBackground, settingsBackgorund;
    public Image waitBackground;
    [SerializeField] AudioClip click, loadingSound;
    public AudioSource chooseSource, loadingSource;
    [SerializeField] Sprite selectCharacter, unSelectCharacter;
    [SerializeField] Slider effectsound, mainSound;
    public Sprite winSprite, loseSprite;
    int videoStep;
    private void Awake()
    {
        uIManager = this;
    }
    void Start()
    {
        mods[0].onClick.AddListener(delegate { ModSelect(0); });
        mods[1].onClick.AddListener(delegate { ModSelect(1); });
        mods[2].onClick.AddListener(delegate { ModSelect(2); });
        mods[3].onClick.AddListener(delegate { ModSelect(3); });

        character.onClick.AddListener(Games);
        multiplayer.onClick.AddListener(Character);
        survivor.onClick.AddListener(SurvivorOpen);
        settings.onClick.AddListener(Settings);
        playGame.onClick.AddListener(WaitPlayers);
        leave.onClick.AddListener(LeaveRoom);
        mainHome.onClick.AddListener(LeaveGame);
        home.onClick.AddListener(LeaveSettings);
        playersCam.onClick.AddListener(PlayerCamChange);
    }
    void LeaveGame()
    {
        Application.Quit();
    }
    void Games()
    {
        UIClose();
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        survivor.gameObject.SetActive(true);
        multiplayer.gameObject.SetActive(true);
        home.gameObject.SetActive(true);
        modBg.gameObject.SetActive(true);
    }
    void Settings()
    {
        UIClose();
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        home.gameObject.SetActive(true);
        settingsBackgorund.gameObject.SetActive(true);
        settingsPanel.SetActive(true);
    }
    void SurvivorOpen()
    {
        Application.OpenURL("https://deeplaystudio.itch.io/survivor");
    }
    void Character()
    {
        ServerControl.server.chooseSound = mainSound.value;
        ServerControl.server.effectSound = effectsound.value;
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        chooseSource.volume = ServerControl.server.chooseSound;
        chooseSource.Play();
        StepOneOne();
    }
    void LeaveSettings()
    {
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        UIClose();
        StepOne();
    }
    void ModSelect(int index)
    {
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        ServerControl.server.modId = index;
    }
    void CharacterChoose(int index)
    {
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        if (ServerControl.server.chooseChar >= 0)
        {
            choose[ServerControl.server.chooseChar].transform.parent.GetComponent<Image>().sprite = unSelectCharacter;
        }
        ServerControl.server.chooseChar = index - 1;
        choose[ServerControl.server.chooseChar].transform.parent.GetComponent<Image>().sprite = selectCharacter;
    }
    void WaitPlayers()
    {
        bool gameState = nicknameText.text.Length > 5 && !nicknameText.text.Contains(" ");
        if (ServerControl.server.chooseChar != -1 && ServerControl.server.modId != -1 && gameState)
        {
            UIClose();
            chooseSource.Stop();
            loadingSource.Play();
            choose[ServerControl.server.chooseChar].transform.parent.GetComponent<Image>().sprite = unSelectCharacter;
            AudioSource.PlayClipAtPoint(click, transform.position, .5f);
            LoadingStart(gameBeforeLoading);
            ServerControl.server.nickName = nicknameText.text;
            //for (int i = 0; i < mods.Count; i++)
            //{
            //    mods[i].gameObject.SetActive(false);
            //}
            //for (int i = 0; i < choose.Count; i++)
            //{
            //    choose[i].transform.parent.gameObject.SetActive(false);
            //}
            //playGame.gameObject.SetActive(false);
            ServerControl.server.ModActive();
            //ServerControl.server.step = 1;
            //PhotonNetwork.LeaveRoom();
        }
    }
    //Öldükten sonra butona basarak farklý oyuncularýn kameralarýna geçme
    void PlayerCamChange()
    {
        int otherCount = PhotonNetwork.PlayerListOthers.Count();
        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Count(); i++)
        {
            //Sýrayla oyuncularýn kameralarýna geçme
            if (i <= GameObject.FindGameObjectsWithTag("Player").Length - 2 && PhotonNetwork.PlayerListOthers[i] != null && GameObject.Find(PhotonNetwork.PlayerListOthers[i + 1].NickName) != null)
            {
                if (Camera.main.GetComponent<CameraFollow>().target.name == PhotonNetwork.PlayerListOthers[i].NickName)
                {
                    AudioSource.PlayClipAtPoint(click, transform.position, .5f);
                    GameObject obj = GameObject.Find(PhotonNetwork.PlayerListOthers[i + 1].NickName);
                    Camera.main.GetComponent<CameraFollow>().target = obj.transform;
                    Camera.main.GetComponent<CameraFollow>().target.GetChild(0).localRotation = Quaternion.identity;
                    playersCam.GetComponentInChildren<TextMeshProUGUI>().text = Camera.main.GetComponent<CameraFollow>().target.gameObject.GetPhotonView().Owner.NickName;
                    return;
                }
            }
        }
        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Count(); i++)
        {
            //Son oyuncuya geldiðinde en baþa dönme
            if (GameObject.Find(PhotonNetwork.PlayerListOthers[i].NickName) != null && PhotonNetwork.PlayerListOthers[i] != null)
            {
                AudioSource.PlayClipAtPoint(click, transform.position, .5f);
                GameObject obj = GameObject.Find(PhotonNetwork.PlayerListOthers[i].NickName);
                Camera.main.GetComponent<CameraFollow>().target = obj.transform;
                Camera.main.GetComponent<CameraFollow>().target.GetChild(0).localRotation = Quaternion.identity;
                playersCam.GetComponentInChildren<TextMeshProUGUI>().text = Camera.main.GetComponent<CameraFollow>().target.gameObject.GetPhotonView().Owner.NickName;
                break;
            }
        }
    }
    void LeaveRoom()
    {
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        LoadingStart(gameLoading);
        ServerControl.server.step = 2;
        leave.gameObject.SetActive(false);
        mainHome.gameObject.SetActive(false);
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.LeaveRoom();
    }
    public void LoadingStart(Image loading)
    {
        loading.gameObject.SetActive(true);
        //loading.GetComponent<RandomInfo>().InfoText();
        loading.DOColor(new Color(loading.color.r, loading.color.g, loading.color.b, 1), .1f).SetEase(Ease.Linear);
    }
    public void LoadingComplete(Image loading)
    {
        loading.DOColor(new Color(loading.color.r, loading.color.g, loading.color.b, 0), .1f).SetEase(Ease.Linear).OnComplete(() => 
        {
            loadingSource.Stop();
            loading.gameObject.SetActive(false);
        });
    }
    void UIClose()
    {
        for (int i = 0; i < canvas.transform.childCount - 4; i++)
        {
            if (canvas.transform.GetChild(i).gameObject.activeSelf)
            {
                canvas.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
    public void StepZero()
    {
        UIClose();
        gameBackground.gameObject.SetActive(true);
        coin.gameObject.SetActive(true);
        play.gameObject.SetActive(true);
        settings.gameObject.SetActive(true);
        mainHome.gameObject.SetActive(true);
    }
    public void StepOne()
    {
        UIClose();
        mainHome.gameObject.SetActive(true);
        gameBackground.gameObject.SetActive(true);
        character.gameObject.SetActive(true);
        settings.gameObject.SetActive(true);
        coin.gameObject.SetActive(true);
    }
    public void StepOneOne()
    {
        UIClose();
        for (int i = 0; i < mods.Count; i++)
        {
            mods[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < choose.Count; i++)
        {
            choose[i].transform.parent.gameObject.SetActive(true);
        }
        choose[0].onClick.AddListener(delegate { CharacterChoose(1); });
        choose[1].onClick.AddListener(delegate { CharacterChoose(2); });
        choose[2].onClick.AddListener(delegate { CharacterChoose(3); });
        choose[3].onClick.AddListener(delegate { CharacterChoose(4); });
        charactersBackground.gameObject.SetActive(true);
        playGame.gameObject.SetActive(true);
        nicknameText.transform.parent.parent.gameObject.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
        LoadingStart(gameBeforeLoading);
    }
    public void NewStepOne()
    {
        UIClose();
        mainHome.gameObject.SetActive(true);
    }
    public void StepTwo()
    {
        UIClose();
        ServerControl.server.killCount = 0;
        playerCount.gameObject.SetActive(true);
        killCountText.gameObject.SetActive(true);
        killCountText.text = "Kill: " + ServerControl.server.killCount.ToString();
        killImage.gameObject.SetActive(false);
        killInfo.text = "";
        deathInfo.text = "";
        collect.text = "";
        killInfo.gameObject.SetActive(true);
        deathInfo.gameObject.SetActive(true);
        collect.gameObject.SetActive(true);
#if UNITY_ANDROID || UNITY_IOS
        moveJoystick.transform.GetChild(0).gameObject.SetActive(false);
        moveJoystick.gameObject.SetActive(true);
        shootJoystick.gameObject.SetActive(true);
        superJoystick.gameObject.SetActive(true);
#endif
    }
}
[System.Serializable]
public class Properties
{
    public List<Button> buttons;
}
