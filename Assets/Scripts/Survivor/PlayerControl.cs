using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    Animator playerAnim;
    float inputX, inputZ;
    bool pressed = true, movable = true;
    public float turnSmoothTime = 2, turnSmoothVelocity;
    Vector3 movement;
    public float moveSpeed;
    public List<Transform> points;
    [SerializeField] List<Vector3> pointsPos;
    public GameManager gameManager;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float animFinish;
    float survivorMoveSpeed;
    private void Awake()
    {
        playerAnim = GetComponent<Animator>();
        for (int i = 0; i < points.Count; i++)
        {
            pointsPos.Add(points[i].position);
            points[i].parent = null;
        }
    }
    void Start()
    {
        playerAnim.SetBool("Idle", true);
    }
    void FixedUpdate()
    {
        survivorMoveSpeed = moveSpeed * gameManager.gameUIManager.thirstyBar.fillAmount * gameManager.gameUIManager.hungryBar.fillAmount;
        if (movable && gameManager.gameUIManager.StaminaUpdate(1))
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].position = transform.position + pointsPos[i];
            }
            Move();
        }
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            gameManager.gameUIManager.SuperBar();
            gameManager.gameUIManager.StaminaUpdate(-1);
        }
        else
        {
            gameManager.gameUIManager.StaminaUpdate(1);
        }
        if (Input.GetKeyDown(KeyCode.E) && gameManager.gameUIManager.superBar.fillAmount == 1)
        {
            StartCoroutine(WaitSuper());
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6 && gameManager.enemyList.Count < 8)
        {
            for (int i = 0; i < gameManager.enemyList.Count; i++)
            {
                if (gameManager.enemyList[i].enemy == other.transform)
                {
                    return;
                }
            }
            EnemyProp enemyProp = new EnemyProp();
            enemyProp.enemy = other.transform;
            enemyProp.attack = false;

            gameManager.enemyList.Add(enemyProp);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            gameManager.gameUIManager.Interact(1, "Drink");
            if (Input.GetKeyDown(KeyCode.E))
            {
                other.gameObject.SetActive(false);
                gameManager.gameUIManager.ThirstyInc(.25f);
                gameManager.gameUIManager.Interact(0, "");
            }
        }
        else if (other.gameObject.layer == 8)
        {
            gameManager.gameUIManager.Interact(1, "Eat");
            if (Input.GetKeyDown(KeyCode.E))
            {
                other.gameObject.SetActive(false);
                gameManager.gameUIManager.HungryInc(.25f);
                gameManager.gameUIManager.Interact(0, "");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            gameManager.gameUIManager.Interact(0, "");
        }
        else if (other.gameObject.layer == 8)
        {
            gameManager.gameUIManager.Interact(0, "");
        }
    }
    IEnumerator WaitSuper()
    {
        movable = false;
        playerAnim.SetTrigger("Super");
        Collider[] enemies = Physics.OverlapBox(transform.position, Vector3.one, Quaternion.identity, enemyLayer);
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponentInChildren<CanvasGroup>().alpha = 1;
        }
        yield return new WaitForSecondsRealtime(animFinish);
        for (int i = 0; i < enemies.Length; i++)
        {
            StartCoroutine(AlphaToZero(enemies[i].GetComponentInChildren<CanvasGroup>()));
        }
        gameManager.gameUIManager.SuperBarReset();
        movable = true;
    }
    IEnumerator AlphaToZero(CanvasGroup canvasGroup)
    {
        yield return new WaitForSecondsRealtime(3);
        canvasGroup.alpha = 0;
    }
    void Move()
    {
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D) ||
    Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
            pressed = true;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            pressed = false;
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");
        if (inputX != 0 || inputZ != 0)
        {
            Vector3 direction = new Vector3(inputX, 0, inputZ).normalized;
            float targetAngle = 0;
            if (direction.magnitude >= .1f)
            {
                targetAngle = (float)Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }
            movement = transform.forward;
            transform.Translate(movement * survivorMoveSpeed * Time.deltaTime, Space.World);
            playerAnim.SetBool("Walk", true);
        }
        else
        {
            playerAnim.SetBool("Walk", false);
        }
    }
}
