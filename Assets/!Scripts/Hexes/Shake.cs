using UnityEngine;
using System.Collections;

public class Shake : MonoBehaviour
{
    public float shakeDuration = 0.5f;
    public float shakeAmplitude = 0.1f;
    public float shakeFrequency = 10f;

    public IEnumerator StartShakeCoroutine()
    {

        Vector3 originalPosition = transform.position;
        float startTime = Time.time;

        while (Time.time - startTime < shakeDuration)
        {
            float t = (Time.time - startTime) / shakeDuration;

            float offsetX = shakeAmplitude * Mathf.Sin(shakeFrequency * 2 * Mathf.PI * t);
            float offsetY = shakeAmplitude * Mathf.Sin(shakeFrequency * 2 * Mathf.PI * t + Mathf.PI / 2);
            float offsetZ = shakeAmplitude * Mathf.Sin(shakeFrequency * 2 * Mathf.PI * t + Mathf.PI);
            transform.position = originalPosition + new Vector3(offsetX, offsetY, offsetZ);
            yield return null;
        }

        transform.position = originalPosition;
    }

    public void StartShake()
    {
        StartCoroutine(StartShakeCoroutine());
    }
}