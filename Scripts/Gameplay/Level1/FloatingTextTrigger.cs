using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FloatingTextTrigger : MonoBehaviour
{
    [Header("检测设置")]
    [Tooltip("玩家“看到”文本所需的视野精确度（角度越小越精确）")]
    public float playerViewAngle = 60.0f;

    private Camera playerCamera;
    private float timeSeen = 0f;
    private bool hasBeenSeen = false;

    void Start()
    {
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("FloatingTextTrigger 找不到 Camera.main！");
            this.enabled = false;
        }
    }

    void Update()
    {
        if (hasBeenSeen) return;

        if (IsPlayerLookingAtMe())
        {
            timeSeen += Time.deltaTime;

            if (timeSeen >= 3.0f)
            {
                hasBeenSeen = true;
                Debug.Log("玩家已看到文本，文本消失。");
                Destroy(gameObject);
            }
        }
    }

    private bool IsPlayerLookingAtMe()
    {
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(transform.position);
        if (viewportPoint.z <= 0 || viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1)
            return false;

        Vector3 directionToText = (transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToText);
        if (angle > playerViewAngle / 2.0f)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, directionToText, out hit, 100f))
        {
            if (hit.transform == this.transform)
                return true;
        }
        return false;
    }
}