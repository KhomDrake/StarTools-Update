using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StarTemperatureTransformation
{
    [Range(0, 200)] public float time = 0f;
    [Range(600, 50000)] public float targetTemperature = 2500f;
    [Range(0, 200)] public float startAfterHowMuchTime = 0f;
}

[System.Serializable]
public class StarSizeTransformation
{
    [Range(0, 200)] public float time = 0f;
    [Range(1, 50)] public int timesSunSize = 1;
    [Range(0, 200)] public float startAfterHowMuchTime = 0f;

    private int sunSize = 100;

    public float GetTargetSize()
    {
        return sunSize * timesSunSize;
    }
}

[System.Serializable]
public class StarPositionTransformation
{
    public Transform orbitingObject;
    public Ellipse orbitPath;
    public float orbitProgress = 0f;
    public float orbitPeriod = 3f;

    public void SetOrbitingObjectPosition()
    {
        Vector2 orbitPos = orbitPath.Evaluate(orbitProgress);
        orbitingObject.localPosition = new Vector3(orbitPos.x, 0, orbitPos.y);
    }

    public IEnumerator AnimateOrbit(bool orbitActive)
    {
        if(orbitPeriod < 0.1f)
        {
            orbitPeriod = 0.1f;
        }

        float orbitSpeed = 1f / orbitPeriod;
        while(orbitActive)
        {
            orbitProgress += Time.deltaTime * orbitSpeed;
            orbitSpeed %= 1f;
            SetOrbitingObjectPosition();
            yield return null;
        }
    }
}

[System.Serializable]
public class StarTransformationConfiguration
{
    public bool changeSize;
    public StarSizeTransformation starSizeTransformation;
    public bool changeTemperature;
    public StarTemperatureTransformation starTemperatureTransformation;
    public bool orbitActive;
    public StarPositionTransformation starPositionTransformation;
}

[RequireComponent(typeof(Star),typeof(StarRotator))]
public class StarTransformation : MonoBehaviour
{
    public StarTransformationConfiguration configuration;
    private Star star;

    private void Awake()
    {
        star = GetComponent<Star>();
        ChangeSize();
        ChangeTemperature();
        Orbit();
    }

    void ChangeSize()
    {
        if (!configuration.changeSize) return;

        StartCoroutine(StartingToChangeSize());
    }

    IEnumerator StartingToChangeSize()
    {
        yield return new WaitForSeconds(configuration.starSizeTransformation.startAfterHowMuchTime);

        float timer = 0f;
        float initialScale = transform.localScale.x;
        float newScale = initialScale;

        while (timer >= 0)
        {
            newScale = Mathf.SmoothStep(initialScale, 
                configuration.starSizeTransformation.GetTargetSize(), 
                timer/ configuration.starSizeTransformation.time);
            timer += Time.deltaTime;
            transform.localScale = new Vector3(newScale, newScale, newScale);
            yield return null;
        }
    }

    void ChangeTemperature()
    {
        if (!configuration.changeTemperature) return;

        StartCoroutine(StartingToChangeTemperature());
    }

    IEnumerator StartingToChangeTemperature()
    {
        yield return new WaitForSeconds(configuration.starTemperatureTransformation.startAfterHowMuchTime);

        float timer = 0f;
        float initialTemperature = star.GetTemperature();
        float newTemperature = initialTemperature;

        while (timer >= 0)
        {
            newTemperature = Mathf.SmoothStep(initialTemperature,
                configuration.starTemperatureTransformation.targetTemperature,
                timer / configuration.starSizeTransformation.time);
            timer += Time.deltaTime;
            star.SetTemperature(newTemperature);
            yield return null;
        }
    }

    void Orbit()
    {
        if (!configuration.orbitActive) return;

        configuration.starPositionTransformation.SetOrbitingObjectPosition();
        StartCoroutine(configuration.starPositionTransformation.AnimateOrbit(configuration.orbitActive));
    }
}
