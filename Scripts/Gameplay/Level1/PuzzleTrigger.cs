using UnityEngine;

public class PuzzleTrigger : BaseTrigger
{
    [Header("谜题触发器设置")]
    [Tooltip("这个触发器对应的ID")]
    public int triggerID = 1;

    protected override void OnActivation(GameObject activator)
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ReportTriggerHit(triggerID);
        }
        else
        {
            Debug.LogError("找不到 LevelManager.Instance！");
        }
    }
}