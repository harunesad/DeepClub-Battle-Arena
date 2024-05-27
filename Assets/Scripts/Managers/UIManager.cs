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
    public List<Button> choose, mods, avatarPanel;
    public List<Properties> avatarButtons;
    public List<TextMeshProUGUI> playersNicknames;
    public List<Image> playersCharacter;
    public Button play, settings, character;
    public Button messageOpen, leave, playersCam, mainHome, home, playGame, moveGame, sound, survivor, multiplayer, skip;
    public TMP_InputField message, messageArea;
    public TextMeshProUGUI killInfo, deathInfo, killCountText, win, time, coin, collect, xpLevel;
    public GameObject killImage, warningImage, damagePopup, settingsPanel;
    public Image mainLoading, modLoading, gameBeforeLoading, gameLoading, xpBar, interact;
    public List<Sprite> characterImages;
    public FloatingJoystick moveJoystick;
    public FixedJoystick shootJoystick, superJoystick;
    [SerializeField] GameObject canvas, freeLookWeb, modBg;
    [SerializeField] List<GameObject> avatarPanels;
    [SerializeField] Sprite open, close;
    [SerializeField] Image gameBackground, charactersBackground, settingsBackgorund, nicknameBackground;
    public Image waitBackground, freeLookMobile;
    [SerializeField] RawImage minimap;
    [SerializeField] AudioClip click, loadingSound;
    public AudioSource chooseSource, loadingSource, birdSource;
    [SerializeField] Sprite selectCharacter, unSelectCharacter;
    [SerializeField] Slider effectsound, mainSound;
    [SerializeField] List<Sprite> hairs;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] VideoClip first, second;
    public Sprite winSprite, loseSprite, closeSound, openSound;
    public bool ai;
    [SerializeField] RawImage soundRaw;
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
        avatarPanel[0].onClick.AddListener(delegate { AvatarPanelsActive(0); });
        avatarPanel[1].onClick.AddListener(delegate { AvatarPanelsActive(1); });
        avatarPanel[2].onClick.AddListener(delegate { AvatarPanelsActive(2); });
        avatarPanel[3].onClick.AddListener(delegate { AvatarPanelsActive(3); });
        avatarPanel[4].onClick.AddListener(delegate { AvatarPanelsActive(4); });
        avatarPanel[5].onClick.AddListener(delegate { AvatarPanelsActive(5); });

        play.onClick.AddListener(Login);
        character.onClick.AddListener(Games);
        multiplayer.onClick.AddListener(Character);
        survivor.onClick.AddListener(SurvivorOpen);
        settings.onClick.AddListener(Settings);
        playGame.onClick.AddListener(WaitPlayers);
        leave.onClick.AddListener(LeaveRoom);
        mainHome.onClick.AddListener(LeaveRoom);
        home.onClick.AddListener(LeaveSettings);
        playersCam.onClick.AddListener(PlayerCamChange);
        messageOpen.onClick.AddListener(MessageOpen);
        moveGame.onClick.AddListener(GameMove);
        sound.onClick.AddListener(SoundState);
        skip.onClick.AddListener(SkipVideo);
        string videoURL = System.IO.Path.Combine(Application.streamingAssetsPath, "Deeplay.mp4");

        videoPlayer.url = videoURL;
        //videoPlayer.Play();
        videoPlayer.prepareCompleted += Prepared;
        videoPlayer.Prepare();
        videoPlayer.time = .1f;

        videoPlayer.loopPointReached += OnVideoEnd;
    }
    void Prepared(VideoPlayer vp)
    {
        vp.Play();
    }
    void SkipVideo()
    {
        if (videoStep == 0)
        {
            videoPlayer.time = 2;
        }
        else if (videoStep == 1)
        {
            videoPlayer.time = 19;
        }
    }
    void OnVideoEnd(VideoPlayer vp)
    {
        // Video bittiðinde yapýlacak iþlemler
        if (videoStep == 0)
        {
            string videoURL = System.IO.Path.Combine(Application.streamingAssetsPath, "Storytelling.mp4");

            videoPlayer.url = videoURL;
            videoPlayer.prepareCompleted += Prepared;
            videoPlayer.Prepare();
            //videoPlayer.clip = second;
            videoStep++;
        }
        else if (videoStep == 1)
        {
            videoPlayer.gameObject.SetActive(false);
            soundRaw.gameObject.SetActive(false);
            videoStep++;
        }
        Debug.Log("Video bitti!");
        // Buraya istediðiniz kodlarý ekleyin
    }
    void AvatarPanelsActive(int avatarPanel)
    {
        if (sound.GetComponent<Image>().sprite == closeSound)
        {
            AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        }
        for (int i = 0; i < avatarPanels.Count; i++)
        {
            avatarPanels[i].gameObject.SetActive(false);
        }
        avatarPanels[avatarPanel].gameObject.SetActive(true);
    }
    public void AvatarPartsSelect(int partId)
    {
        if (sound.GetComponent<Image>().sprite == closeSound)
        {
            AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        }
        int panelId = -1;
        for (int i = 0; i < avatarPanels.Count; i++)
        {
            if (avatarPanels[i].activeSelf)
            {
                panelId = i;
            }
        }
        ServerControl.server.avatarsId[panelId] = partId;
        if (panelId == 0)
        {
            avatarPanel[panelId].GetComponent<Image>().sprite = avatarButtons[panelId].buttons[partId].GetComponent<Image>().sprite;
            avatarPanel[1].GetComponent<Image>().sprite = hairs[partId];
        }
        else if (panelId >= 1)
        {
            ColorBlock colorBlock = avatarPanel[panelId].GetComponent<Button>().colors;
            colorBlock.normalColor = avatarButtons[panelId].buttons[partId].GetComponent<Button>().colors.normalColor;
            colorBlock.highlightedColor = avatarButtons[panelId].buttons[partId].GetComponent<Button>().colors.highlightedColor;
            colorBlock.selectedColor = avatarButtons[panelId].buttons[partId].GetComponent<Button>().colors.selectedColor;
            avatarPanel[panelId].GetComponent<Button>().colors = colorBlock;
        }
    }
    void SoundState()
    {
        if (sound.GetComponent<Image>().sprite == closeSound)
        {
            sound.GetComponent<Image>().sprite = openSound;
        }
        else
        {
            sound.GetComponent<Image>().sprite = closeSound;
        }
        if (sound.GetComponent<Image>().sprite == closeSound)
        {
            AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        }
    }
    void Login()
    {
        if (nicknameText.text.Length >= 6 && !nicknameText.text.Contains(" "))
        {
            if (ServerControl.server.step == 0)
            {
                if (sound.GetComponent<Image>().sprite == closeSound)
                {
                    AudioSource.PlayClipAtPoint(click, transform.position, .5f);
                }
                if (sound.GetComponent<Image>().sprite == closeSound)
                {
                    birdSource.Play();
                }
                LoadingStart(mainLoading);
                PhotonNetwork.ConnectUsingSettings();
                ServerControl.server.nickName = nicknameText.text;
                StepZero();
            }
        }
    }
    public void Interact(bool state, string message)
    {
        interact.GetComponentInChildren<TextMeshProUGUI>().text = message;
        interact.gameObject.SetActive(state);
    }
    void GameMove()
    {
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        if (moveGame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "OYUNA GÝT!")
        {
            ServerControl.server.mainAvatar.GetComponent<NavMeshAgent>().enabled = true;
            ServerControl.server.mainAvatar.GetComponent<NavMeshAgent>().isStopped = false;
            ServerControl.server.mainAvatar.GetComponent<NavMeshAgent>().SetDestination(ServerControl.server.portal.transform.position);
            ai = true;
            moveGame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "DUR!";
        }
        else
        {
            moveGame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "OYUNA GÝT!";
            ServerControl.server.mainAvatar.GetComponent<NavMeshAgent>().isStopped = true;
            ai = false;
            ServerControl.server.mainAvatar.GetComponent<NavMeshAgent>().enabled = false;
        }
        for (int i = 0; i < ServerControl.server.mainAvatar.transform.childCount; i++)
        {
            if (ServerControl.server.mainAvatar.transform.GetChild(i).gameObject.activeSelf)
            {
                ServerControl.server.mainAvatar.transform.GetChild(i).GetComponent<Animator>().SetBool("Walk", ai);
            }
        }
    }
    public void MessageOpen()
    {
        if (sound.GetComponent<Image>().sprite == closeSound)
        {
            AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        }
        ServerControl.server.chatAtcive = !ServerControl.server.chatAtcive;
        message.gameObject.SetActive(!message.gameObject.activeSelf);
        messageArea.gameObject.SetActive(!messageArea.gameObject.activeSelf);
        messageArea.GetComponentInChildren<Scrollbar>().value = 1;
        if (message.gameObject.activeSelf)
        {
            messageOpen.GetComponent<Image>().sprite = close;
        }
        else
        {
            messageOpen.GetComponent<Image>().sprite = open;
        }
        warningImage.SetActive(false);
    }
    void LeaveRoom()
    {
        birdSource.Play();
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        LoadingStart(mainLoading);
        ServerControl.server.step = 2;
        leave.gameObject.SetActive(false);
        mainHome.gameObject.SetActive(false);
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.LeaveRoom();
    }
    void SurvivorOpen()
    {
        Application.OpenURL("https://deeplaystudio.itch.io/survivor");
    }
    void Settings()
    {
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        UIClose();
        home.gameObject.SetActive(true);
        settingsBackgorund.gameObject.SetActive(true);
        settingsPanel.SetActive(true);
    }
    void LeaveSettings()
    {
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        UIClose();
        StepOne();
    }
    void Games()
    {
        AudioSource.PlayClipAtPoint(click, transform.position, .5f);
        survivor.gameObject.SetActive(true);
        multiplayer.gameObject.SetActive(true);
        home.gameObject.SetActive(true);
        modBg.gameObject.SetActive(true);
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
        if (ServerControl.server.chooseChar != -1 && ServerControl.server.modId != -1)
        {
            chooseSource.Stop();
            loadingSource.Play();
            choose[ServerControl.server.chooseChar].transform.parent.GetComponent<Image>().sprite = unSelectCharacter;
            AudioSource.PlayClipAtPoint(click, transform.position, .5f);
            LoadingStart(gameBeforeLoading);
            for (int i = 0; i < mods.Count; i++)
            {
                mods[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < choose.Count; i++)
            {
                choose[i].transform.parent.gameObject.SetActive(false);
            }
            playGame.gameObject.SetActive(false);
            PhotonNetwork.LeaveRoom();
        }
    }
    //Öldükten sonra butona basarak farklý oyuncularýn kameralarýna geçme
    void PlayerCamChange()
    {
        int otherCount = PhotonNetwork.PlayerListOthers.Count();
        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Count(); i++)
        {
            //Sýrayla oyuncularýn kameralarýna geçme
            if (i <= GameObject.FindGameObjectsWithTag("Player").Length - 2 && PhotonNetwork.PlayerListOthers[i] != null/* && PhotonNetwork.PlayerListOthers[i + 1] != null*/ && GameObject.Find(PhotonNetwork.PlayerListOthers[i + 1].NickName) != null)
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
    public void LoadingComplete(Image loading)
    {
        birdSource.Stop();
        loading.DOColor(new Color(loading.color.r, loading.color.g, loading.color.b, 0), .1f).SetEase(Ease.Linear).OnComplete(() => 
        {
            loadingSource.Stop();
            loading.gameObject.SetActive(false);
        });
    }
    public void LoadingStart(Image loading)
    {
        loading.gameObject.SetActive(true);
        //loading.GetComponent<RandomInfo>().InfoText();
        loading.DOColor(new Color(loading.color.r, loading.color.g, loading.color.b, 1), .1f).SetEase(Ease.Linear);
    }
    void UIClose()
    {
        if (freeLookWeb.activeSelf)
        {
            freeLookWeb.SetActive(false);
        }
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
        StepZeroOne();
        nicknameBackground.gameObject.gameObject.SetActive(true);
        nicknameBackground.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = ServerControl.server.nickName;
        nicknameBackground.transform.GetChild(0).GetComponent<Image>().sprite = avatarPanel[0].GetComponent<Image>().sprite;
        minimap.gameObject.SetActive(true);
        messageArea.text = "";
        messageOpen.gameObject.SetActive(true);
        playerCount.gameObject.SetActive(true);
        moveGame.gameObject.SetActive(true);
#if UNITY_ANDROID || UNITY_IOS
        freeLookMobile.gameObject.SetActive(true);
        moveJoystick.gameObject.SetActive(true);
#endif
#if UNITY_WEBGL
        freeLookWeb.SetActive(true);
#endif
    }
    void StepZeroOne()
    {
        ServerControl.server.mainFloor.SetActive(true);
        ServerControl.server.mainShip.SetActive(true);
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
