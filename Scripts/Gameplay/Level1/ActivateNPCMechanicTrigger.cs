using UnityEngine;

public class ActivateNPCMechanicTrigger : BaseTrigger
{
    protected override void OnActivation(GameObject activator)
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ActivateNPCMechanic();
        }
        else
        {
            Debug.LogError("’“≤ªµΩ LevelManager.Instance£°");
        }
    }
}