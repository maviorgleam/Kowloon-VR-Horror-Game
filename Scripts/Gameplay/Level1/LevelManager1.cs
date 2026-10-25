using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private enum LevelState
    {
        Level1_Puzzle,
        Level2_ShowingText, // 新状态：文本已激活，正在等待被玩家注视
        Level2_Intro,       // 文本已被注视，正在等待玩家踩Lvl2的触发器
        Level2_NPCOn,
        Level_Failed
    }
    private LevelState currentState;

    [Header("Level 1 设置")]
    [Tooltip("将4个需要按顺序闪烁的光源 (FlickeringLight 脚本) 拖拽到这里")]
    public List<FlickeringLight> puzzleLights;
    private int currentPuzzleIndex = 0;
    public GameObject Look1;
    public GameObject Look2;

    [Header("Level 2 Text (新)")]
    [Tooltip("将场景中预先放置好的Lvl2 '不要回头' 文本对象拖拽到这里")]
    public GameObject level2TextObject;
    [Tooltip("玩家“看到”文本所需的视野精确度（角度）")]
    public float level2TextViewAngle = 60.0f;
    [Tooltip("玩家需要注视文本多少秒后文本才会消失")]
    public float level2TextGazeDuration = 10.0f;
    private float textSeenTimer = 0f;

    [Header("Level 2 NPC")]
    [Tooltip("玩家回头超过这个角度（度）则触发失败")]
    public float turnAngleThreshold = 135.0f;
    [Tooltip("Jumpscare 时 NPC 生成在玩家背后的距离")]
    public float npcSpawnDistance = 2.0f;
    [Tooltip("要生成的NPC预制体")]
    public GameObject npcPrefab;

    [Header("关卡状态 (只读)")]
    [Tooltip("如果玩家回头，此值为 True")]
    public bool levelFailed = false;

    private Transform playerTransform;
    private Camera playerCamera; // 用于Lvl2的文本注视检测
    private Vector3 playerForwardAtActivation;

    void Start()
    {
        levelFailed = false;

        GameObject playerBody = GameObject.FindWithTag("Player");
        if (playerBody != null)
        {
            playerTransform = playerBody.transform;
        }
        else
        {
            Debug.LogError("LevelManager 无法找到带有 'Player' 标签的对象！关卡无法启动。");
            this.enabled = false;
            return;
        }

        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("LevelManager 无法找到 Camera.main！Lvl2文本检测将失败。");
        }

        if (Look1 != null) { Look1.SetActive(true); }
        if (Look2 != null) { Look2.SetActive(false); }
        // 确保Lvl2的文本在游戏开始时是隐藏的
        if (level2TextObject != null)
        {
            level2TextObject.SetActive(false);
        }

        currentState = LevelState.Level1_Puzzle;
        currentPuzzleIndex = 0;
        ActivateNextPuzzleLight();
    }

    void Update()
    {
        // 状态机
        switch (currentState)
        {
            case LevelState.Level2_ShowingText:
                CheckIfPlayerSeesText();
                break;

            case LevelState.Level2_NPCOn:
                CheckForPlayerTurn();
                break;
        }
    }

    #region Level 1 逻辑

    private void ActivateNextPuzzleLight()
    {
        if (puzzleLights == null || puzzleLights.Count == 0)
        {
            Debug.LogError("LevelManager: 'Puzzle Lights' 列表为空！");
            return;
        }

        if (currentPuzzleIndex < puzzleLights.Count)
        {
            puzzleLights[currentPuzzleIndex].StartFlickering();
        }
        else
        {
            StartLevel2();
        }
    }

    public void ReportTriggerHit(int triggerID)
    {
        if (currentState != LevelState.Level1_Puzzle) return;

        if (triggerID != (currentPuzzleIndex + 1))
        {
            return;
        }

        puzzleLights[currentPuzzleIndex].StopFlickering();
        currentPuzzleIndex++;
        if (currentPuzzleIndex == 3)
        {
            Look1.SetActive(false);
            Look2.SetActive(true);
        }
        ActivateNextPuzzleLight();
    }

    #endregion

    #region Level 2 逻辑

    private void StartLevel2()
    {
        Debug.LogWarning("Level 1 完成！开始 Level 2 ...");
        currentState = LevelState.Level2_ShowingText;

        // 激活场景中预置的文本
        if (level2TextObject != null)
        {
            level2TextObject.SetActive(true);
            textSeenTimer = 0f;
        }
        else
        {
            // 如果没有设置文本，直接跳过，等待Lvl2的触发器
            Debug.LogWarning("Lvl2 文本未设置，直接进入下一阶段。");
            currentState = LevelState.Level2_Intro;
        }
    }

    private void CheckIfPlayerSeesText()
    {
        if (level2TextObject == null || playerCamera == null) return;

        // 使用 IsPlayerLookingAt 辅助方法来检测
        if (IsPlayerLookingAt(level2TextObject, level2TextViewAngle))
        {
            textSeenTimer += Time.deltaTime;
            if (textSeenTimer >= level2TextGazeDuration)
            {
                Debug.Log("玩家已看到文本，文本消失。");
                level2TextObject.SetActive(false);
                currentState = LevelState.Level2_Intro; // 切换到“等待触发器”状态
            }
        }
    }

    public void ActivateNPCMechanic()
    {
        // 只有在文本被看到之后（Intro状态），这个触发器才有效
        //if (currentState != LevelState.Level2_Intro) return;

        Debug.Log("NPC 机制已激活... 最好别回头...");
        currentState = LevelState.Level2_NPCOn;
        playerForwardAtActivation = playerCamera.transform.forward;
    }

    private void CheckForPlayerTurn()
    {
        if (levelFailed) return;

        float angle = Vector3.Angle(playerForwardAtActivation, playerCamera.transform.forward);
        if (angle > turnAngleThreshold)
        {
            TriggerFailureSequence();
        }
    }

    private void TriggerFailureSequence()
    {
        Debug.LogWarning("你回头了！ 游戏失败。");

        levelFailed = true;
        currentState = LevelState.Level_Failed;

        Vector3 spawnPos = playerCamera.transform.position - (playerTransform.forward * npcSpawnDistance);

        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 2.0f, Vector3.down, out hit, 5.0f))
        {
            spawnPos = hit.point;
        }

        GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        npc.transform.LookAt(playerTransform);
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 辅助方法：检查玩家是否正在注视某个物体
    /// </summary>
    private bool IsPlayerLookingAt(GameObject target, float viewAngle)
    {
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(target.transform.position);
        if (viewportPoint.z <= 0 || viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1)
        {
            return false; // 不在屏幕上
        }

        Vector3 directionToTarget = (target.transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToTarget);
        if (angle > viewAngle / 2.0f)
        {
            return false; // 在屏幕边缘
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, directionToTarget, out hit, 100f))
        {
            if (hit.transform == target.transform || hit.transform.IsChildOf(target.transform))
            {
                return true; // 正在直视
            }
        }
        return false; // 被挡住了
    }

    #endregion
}