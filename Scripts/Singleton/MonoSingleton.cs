using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    public static T Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = GetComponent<T>();
        }
        else Destroy(this);
        OnAwake();
    }

    protected virtual void OnAwake(){}
}
