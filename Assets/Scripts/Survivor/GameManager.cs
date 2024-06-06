using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<GameObject> characters;
    [SerializeField] Transform enemies;
    public List<Transform> enemyList;
    [SerializeField] NavMeshSurface surface;
    public Transform player;
    public GameUIManager gameUIManager;
    [SerializeField] Image healthBar;
    bool attack;
    void Start()
    {
        player = Instantiate(characters[DataSave.Instance.characterIndex], Vector3.zero, Quaternion.identity).transform;
        player.GetComponent<PlayerControl>().gameManager = this;
        enemyList.Add(enemies.GetChild(0));
        enemyList[0].GetComponent<NavMeshAgent>().SetDestination(player.GetComponent<PlayerControl>().points[0].position);
        enemyList[0].GetComponent<Animator>().SetBool("Walk", true);
        //InvokeRepeating("Builder", 0, 1);
        //surface.BuildNavMesh();
    }
    void Builder()
    {
        surface.BuildNavMesh();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            //bool posX = Mathf.Approximately(player.GetComponent<PlayerControl>().points[i].position.x, enemyList[i].position.x);
            //bool posZ = Mathf.Approximately(player.GetComponent<PlayerControl>().points[i].position.z, enemyList[i].position.z);
            bool posX = (Mathf.Abs(player.GetComponent<PlayerControl>().points[i].position.x - enemyList[i].position.x) <= .01f);
            bool posZ = (Mathf.Abs(player.GetComponent<PlayerControl>().points[i].position.z - enemyList[i].position.z) <= .01f);
            enemyList[i].transform.LookAt(player.position);
            if (!enemyList[i].GetComponent<NavMeshAgent>().hasPath)
            {
                //enemyList[i].transform.LookAt(player.position);
                enemyList[i].GetComponent<Animator>().SetBool("Walk", false);
                if ((!posX || !posZ) && enemyList[i].GetComponentInChildren<CanvasGroup>().alpha != 1)
                {
                    enemyList[i].GetComponent<NavMeshAgent>().SetDestination(player.GetComponent<PlayerControl>().points[i].position);
                    enemyList[i].GetComponent<Animator>().SetBool("Walk", true);
                }
                else if (posX && posZ && enemyList[i].GetComponentInChildren<CanvasGroup>().alpha != 1 && !attack)
                {
                    attack = true;
                    enemyList[i].GetComponent<Animator>().SetTrigger("Attack");
                    TakeDamage(.1f);
                    StartCoroutine(Attackable());
                }
            }
            else
            {
                if ((!posX || !posZ) && enemyList[i].GetComponentInChildren<CanvasGroup>().alpha != 1)
                {
                    enemyList[i].GetComponent<NavMeshAgent>().SetDestination(player.GetComponent<PlayerControl>().points[i].position);
                    enemyList[i].GetComponent<Animator>().SetBool("Walk", true);
                }
            }
        }
    }
    public void TakeDamage(float damage)
    {
        Debug.Log(healthBar.fillAmount);
        healthBar.fillAmount -= damage;
        if (healthBar.fillAmount == 0)
        {
            Time.timeScale = 0;
            gameUIManager.gameover.SetActive(true);
            gameUIManager.home.gameObject.SetActive(false);
            //StartCoroutine(SceneLaod());
        }
    }
    //IEnumerator SceneLaod()
    //{
    //    yield return new WaitForSecondsRealtime(2);
    //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    //    Time.timeScale = 1;
    //}
    IEnumerator Attackable()
    {
        yield return new WaitForSeconds(1);
        attack = false;
    }
}
