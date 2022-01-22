using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region namespace BattleCity start
namespace BattleCity
{
public class GenericSingleton<T> : MonoBehaviour where T : Component
{
    private static T instance = null;
    public static T Instance {
        get {
            return instance;
        }
    }

    protected virtual void Awake() {
        if (instance == null) {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
}

public class GameManager : GenericSingleton<GameManager>
{
    public Camera pCamera;
    public Camera oCamera;
    public Player pfPlayer;
    public Enemy pfEnemy;
    public Animator animRespawn;
    public GameObject respawnUnder;
    public Text textScoreVal;
    public Text textDefeatNum;
    public Text textLifeNum;
    public Image imgGameOver;
    public Joystick joystick;
    public GameObject dPadX;
    private Player playerInstance;
    private bool isPerspective = false;
    private const float respawnPlayerTime = 1f;
    private float respawnPlayerTimer = -1f;
    private bool requestRespawnPlayerFlag = false;
    private float enemyAlive = 0;
    private const float maxEnemyAlive = 5;
    private const float respawnEnemyTime = 2f;
    private float respawnEnemyTimer = -1f;
    private int scoreVal = 0;
    private int defeatEnemyNum = 0;
    private int life = 3;
    private bool isGameOver = false;

    protected override void Awake()
    {
        base.Awake();
        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    private void Start() {
        MapGenerator.Instance.GenerateMap();
        RequestRespawnPlayer();
        AudioManager.Instance.PlayClip(Resources.Load<AudioClip>("AudioClips/Start"));
    }

    private void Update() {
        respawnPlayerTimer -= Time.deltaTime;
        respawnEnemyTimer -= Time.deltaTime;

        MainLoop();
        HandleInput();
    }

    private void LateUpdate() {
        textScoreVal.text = string.Format("{0:D5}", scoreVal);
        textDefeatNum.text = defeatEnemyNum.ToString();
        textLifeNum.text = life.ToString();
    }

    private void MainLoop() {
        if (isGameOver) return;

        // Respawn player and enemies
        RespawnPlayer();
        RespawnEnemy();
    }

    private void HandleInput() {
        if (Input.GetKeyDown(KeyCode.V))
            SwitchPerspective();
    }

    public void SwitchPerspective(bool? perspective = null) {
        perspective = perspective ?? (!isPerspective);

        if (perspective != isPerspective) {
            isPerspective = perspective.Value;
            pCamera.gameObject.SetActive(perspective.Value);
            oCamera.gameObject.SetActive(!perspective.Value);
            dPadX.SetActive(perspective.Value);
        }
    }

    public Player GetPlayerInstance() {
        return playerInstance;
    }

    public void PlayerDied() {
        if (--life > 0) {
            playerInstance = null;
            RequestRespawnPlayer();
        } else {
            GameOver();
        }
    }

    public void EnemyDied() {
        enemyAlive--;
        respawnEnemyTimer = respawnEnemyTime;
        defeatEnemyNum++;
        scoreVal += 100;
    }

    public bool RequestRespawnPlayer() {
        if (respawnPlayerTimer > 0)
            return false;
        else {
            requestRespawnPlayerFlag = true;
            respawnPlayerTimer = respawnPlayerTime;
        }
        return true;
    }

    public void RespawnPlayer() {
        if (respawnPlayerTimer <= 0 && requestRespawnPlayerFlag) {
            requestRespawnPlayerFlag = false;
            GeneratePlayer();
        }
    }

    public void GeneratePlayer() {
        Vector2 respawnPos = MapGenerator.Instance.GetPlayerRespawnPosition();
        Animator animInstance = Instantiate(animRespawn, respawnUnder.transform);
        animInstance.transform.localPosition = respawnPos;
        StartCoroutine(
            NewClosureWithYieldInstruction(
                () => {
                    playerInstance = Instantiate(pfPlayer, respawnUnder.transform);
                    playerInstance.transform.localPosition = respawnPos;
                },
                new WaitForSeconds(animInstance.GetCurrentAnimatorStateInfo(0).length)
            )
        );
    }

    public void RespawnEnemy() {
        if (respawnEnemyTimer <= 0 && enemyAlive < maxEnemyAlive) {
            respawnEnemyTimer = respawnEnemyTime;
            enemyAlive++;
            GenerateEnemy();
        }
    }

    public void GenerateEnemy() {
        Vector2 respawnPos = MapGenerator.Instance.GetEnemyRespawnPosition();
        Animator animInstance = Instantiate(animRespawn, respawnUnder.transform);
        animInstance.transform.localPosition = respawnPos;
        StartCoroutine(
            NewClosureWithYieldInstruction(
                () => {
                    Instantiate(pfEnemy, respawnUnder.transform).transform.localPosition = respawnPos;
                },
                new WaitForSeconds(animInstance.GetCurrentAnimatorStateInfo(0).length)
            )
        );
    }

    public void GameOver() {
        if (isGameOver) return;
        isGameOver = true;
        imgGameOver.gameObject.SetActive(true);
        if (playerInstance != null) {
            playerInstance.GetComponent<Player>().enabled = false;
        }
    }

    // @note    返回一个带yield指令的闭包
    // @param   func 函数  instruction yield指令  times 重复次数（0为无数次，默认为1）
    // @return  一个带yield指令的闭包
    public IEnumerator NewClosureWithYieldInstruction(System.Action func, YieldInstruction instruction, int times = 1) {
        if (times < 0)
            yield break;
        for (int i = 0; times == 0 ? true : i < times; i++) {
            yield return instruction;
            func();
        }
    }

    public void OnButtonClick(string msg) {
        if (msg == "BtnA") {
            if (playerInstance != null) {
                playerInstance.Fire();
            }
        } else if (msg == "BtnB") {
            SwitchPerspective();
        } else if (msg == "BtnLeft") {
            if (pCamera.TryGetComponent<RotateCamAround>(out RotateCamAround rotateCamAround)) {
                rotateCamAround.RotateAround(-45f, 0.2f);
            }
        } else if (msg == "BtnRight") {
            if (pCamera.TryGetComponent<RotateCamAround>(out RotateCamAround rotateCamAround)) {
                rotateCamAround.RotateAround(45f, 0.2f);
            }
        }
    }
}
}
#endregion
