using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winCountText, timeText;
    [SerializeField] float time;
    [SerializeField] GameManager gameManager;
    [SerializeField] CanvasGroup interact;
    public Button home;
    [SerializeField] Button winHome, loseHome, winRestart, loseRestart;
    [SerializeField] GameSound gameSound;
    float filAmount;
    public Image superBar, staminaBar, hungryBar, thirstyBar;
    public GameObject win, gameover;
    bool superAmount, staminaAmount;
    void Start()
    {
        gameSound.musicWindow.Play();
        time = (DataSave.Instance.difficulty + 1) * 100;
        winCountText.text = PlayerPrefs.GetInt("Win", DataSave.Instance.winCount).ToString();
        home.onClick.AddListener(Home);
        winHome.onClick.AddListener(Home);
        loseHome.onClick.AddListener(Home);
        winRestart.onClick.AddListener(Restart);
        loseRestart.onClick.AddListener(Restart);
    }
    void Update()
    {
        if (time > 0)
        {
            timeText.text = (int)(time / 60) + " : " + (int)(time % 60);
            time -= Time.deltaTime;
        }
        else
        {
            home.gameObject.SetActive(false);
            Time.timeScale = 0;
            win.gameObject.SetActive(true);
            DataSave.Instance.WinUpdate(DataSave.Instance.winCount + 1);
            PlayerPrefs.SetInt("Win", DataSave.Instance.winCount);
            winCountText.text = PlayerPrefs.GetInt("Win", DataSave.Instance.winCount).ToString();
            //StartCoroutine(SceneLaod());
        }
        HungryBar();
        ThirstyBar();
    }
    //IEnumerator SceneLaod()
    //{
    //    yield return new WaitForSecondsRealtime(2);
    //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    //    Time.timeScale = 1;
    //}
    void Home()
    {
        gameSound.musicWindow.Stop();
        gameSound.Click();
        StartCoroutine(LoadScene());
    }
    void Restart()
    {
        gameSound.musicWindow.Stop();
        gameSound.Click();
        StartCoroutine(RestartScene());
    }
    IEnumerator LoadScene()
    {
        yield return new WaitForSecondsRealtime(1);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    IEnumerator RestartScene()
    {
        yield return new WaitForSecondsRealtime(1);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void SuperBar()
    {
        if (!superAmount)
        {
            filAmount += Time.deltaTime / 25;
            superBar.fillAmount = filAmount;
        }
        if (superBar.fillAmount == 1)
        {
            Interact(1, "Power");
            superAmount = true;
        }
    }
    void HungryBar()
    {
        hungryBar.fillAmount -= Time.deltaTime / 50;
    }
    public void HungryInc(float inc)
    {
        hungryBar.fillAmount += inc;
    }
    void ThirstyBar()
    {
        thirstyBar.fillAmount -= Time.deltaTime / 50;
    }
    public void ThirstyInc(float inc)
    {
        thirstyBar.fillAmount += inc;
    }
    public void SuperBarReset()
    {
        superBar.DOFillAmount(0, 2).SetEase(Ease.Linear).OnComplete(() =>
        {
            filAmount = 0;
            Interact(0, "");
            superAmount = false;
        });
    }
    public bool StaminaUpdate(float value)
    {
        if (staminaBar.fillAmount != 0 && !staminaAmount)
        {
            if (value == 1)
            {
                staminaBar.fillAmount -= Time.deltaTime / 25;
            }
            else
            {
                staminaBar.fillAmount += Time.deltaTime / 10 ;
            }
        }
        else if (staminaBar.fillAmount == 0)
        {
            gameManager.player.GetComponent<Animator>().SetBool("Walk", false);
            staminaAmount = true;
            staminaBar.DOFillAmount(1, 2).SetEase(Ease.Linear).OnComplete(() =>
            {
                staminaAmount = false;
            });
        }
        return !staminaAmount;
    }
    public void Interact(float alpha, string message)
    {
        interact.alpha = alpha;
        interact.GetComponentInChildren<TextMeshProUGUI>().text = message;
    }
}
