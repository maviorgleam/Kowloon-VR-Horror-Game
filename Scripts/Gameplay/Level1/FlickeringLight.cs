using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    [Header("闪烁参数")]
    [Tooltip("闪烁时的最小光强")]
    public float minIntensity = 50f;
    [Tooltip("闪烁时的最大光强")]
    public float maxIntensity = 500f;
    [Tooltip("闪烁速度（每次闪烁间隔时间）")]
    public float flickerSpeed = 0.07f;

    private Light targetLight;
    private Coroutine flickerCoroutine;
    private bool isFlickering = false;

    void Awake()
    {
        targetLight = GetComponent<Light>();
        targetLight.enabled = false;
    }

    public void StartFlickering()
    {
        if (isFlickering) return;

        isFlickering = true;
        targetLight.enabled = true;
        flickerCoroutine = StartCoroutine(DoFlicker());
    }

    public void StopFlickering()
    {
        if (!isFlickering) return;

        isFlickering = false;
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
        }

        targetLight.enabled = true;
        targetLight.intensity = maxIntensity;
    }

    private IEnumerator DoFlicker()
    {
        while (isFlickering)
        {
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);
            yield return new WaitForSeconds(flickerSpeed);
        }
    }
}