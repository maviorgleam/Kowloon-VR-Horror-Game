using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class BaseTrigger : MonoBehaviour
{
    [Header("基类触发设置")]
    [Tooltip("只有带有此标签的对象才能激活此触发器")]
    public string requiredTag = "Player";

    [Tooltip("此触发器是否只能被激活一次？")]
    public bool triggerOnce = true;

    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            return;
        }

        if (triggerOnce && hasBeenTriggered)
        {
            return;
        }

        hasBeenTriggered = true;
        OnActivation(other.gameObject);
    }

    protected abstract void OnActivation(GameObject activator);

    protected virtual void OnValidate()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"已自动将 {name} 上的 Collider 设置为 'Is Trigger'。", this);
        }
    }
}