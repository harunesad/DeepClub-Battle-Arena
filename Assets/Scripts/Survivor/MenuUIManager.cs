using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] Button play, settings, characterChoose;
    [SerializeField] Button mainHome;
    [SerializeField] CanvasGroup characterWindow, settingsWindow, entryWindow;
    [SerializeField] Sprite select, unselect;
    [SerializeField] List<Button> characters;
    [SerializeField] TextMeshProUGUI winCountText;
    [SerializeField] TMP_Dropdown difficulty;
    [SerializeField] MenuSound menuSound;
    [SerializeField] Image choosen;
    void Start()
    {
        winCountText.text = PlayerPrefs.GetInt("Win", DataSave.Instance.winCount).ToString();
        mainHome.onClick.AddListener(MainHome);
        characterChoose.onClick.AddListener(CharacterWindow);
        settings.onClick.AddListener(SettingsOpen);
        play.onClick.AddListener(PlayGame);
        for (int i = 0; i < characters.Count; i++)
        {
            int j = i;
            characters[i].onClick.AddListener(delegate { CharacterSelect(characters[j].transform.parent.GetComponent<Image>(), j); });
        }
        StartCoroutine(MenuSound());
    }
    IEnumerator CharacterSound()
    {
        yield return new WaitUntil(() => characterWindow.alpha == 1);
        menuSound.characterWindow.Play();
        yield return new WaitUntil(() => characterWindow.alpha == 0);
        menuSound.characterWindow.Stop();
    }
    IEnumerator MenuSound()
    {
        yield return new WaitUntil(() => entryWindow.alpha == 1);
        menuSound.loadingWindow.Play();
        yield return new WaitUntil(() => entryWindow.alpha == 0);
        menuSound.loadingWindow.Stop();
    }
    void MainHome()
    {
        menuSound.Click();
        Debug.Log("Exit");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    void CharacterWindow()
    {
        UITransition(entryWindow, characterWindow);
        StartCoroutine(CharacterSound());
    }
    void SettingsOpen()
    {
        UITransition(entryWindow, settingsWindow);
        entryWindow.blocksRaycasts = false;
    }
    void CharacterSelect(Image choosenImage, int index)
    {
        menuSound.Click();
        choosen.sprite = unselect;
        choosen = choosenImage;
        choosen.sprite = select;
        DataSave.Instance.CharacterSelect(index);
    }
    void PlayGame()
    {
        menuSound.Click();
        characterWindow.blocksRaycasts = false;
        characterWindow.DOFade(0, 1).SetEase(Ease.Linear).OnComplete(() =>
        {
            DataSave.Instance.WinUpdate(int.Parse(winCountText.text));
            DataSave.Instance.DifficultySelect(difficulty.value);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        });
    }
    public void MainSoundUpdate(Slider slider)
    {
        DataSave.Instance.SoundUpdate(-1, slider.value);
        menuSound.characterWindow.volume = slider.value;
        menuSound.loadingWindow.volume = slider.value;
    }
    public void EffectSoundUpdate(Slider slider)
    {
        DataSave.Instance.SoundUpdate(slider.value, -1);
    }
    public void HomeOpen(CanvasGroup canvasGroup)
    {
        StartCoroutine(MenuSound());
        UITransition(canvasGroup, entryWindow);
    }
    void UITransition(CanvasGroup close, CanvasGroup open)
    {
        menuSound.Click();
        close.blocksRaycasts = false;
        close.DOFade(0, 1).SetEase(Ease.Linear).OnComplete(() =>
        {
            open.DOFade(1, 1).SetEase(Ease.Linear).OnComplete(() =>
            {
                open.blocksRaycasts = true;
            });
        });
    }
}