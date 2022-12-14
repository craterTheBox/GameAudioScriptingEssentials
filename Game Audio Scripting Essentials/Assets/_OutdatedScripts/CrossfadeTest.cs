using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAudioScriptingEssentials;

public class CrossfadeTest : MonoBehaviour
{
    [SerializeField] AudioClipRandomizer m_clipSectionOne;
    GameObject buffer;
    AudioClipRandomizer m_clipSectionTwo;
    [SerializeField] AudioClipRandomizer[] m_audioLayer;
    [SerializeField] AudioRandomizerContainer[] m_sections;

    bool isPlayingTrackOne = true;
    bool isRunningCrossfade = false;
    bool isRunningCheck = false;

    void Start()
    {
        buffer = new GameObject("Section 2");
        buffer.transform.SetParent(transform);

        m_clipSectionTwo = buffer.AddComponent<AudioClipRandomizer>();
        
        m_clipSectionTwo.ArcObj = m_sections[1];
        m_clipSectionTwo.OverrideArcSettings = false;

        if (isPlayingTrackOne)
            m_clipSectionOne.PlaySFX();
    }

    public void SwapTrack()
    {
        if (!isRunningCrossfade && !isRunningCheck)
            StartCoroutine(FadeTrack());
        else if (isRunningCrossfade && !isRunningCheck)
            StartCoroutine(WaitForCoroutineToFinish());
    }

    IEnumerator FadeTrack()
    {
        isRunningCrossfade = true;

        float timeToFade = 1.0f;
        float timeElapsed = 0.0f;

        if (isPlayingTrackOne)
        {
            m_clipSectionTwo.PlaySFX();

            while (timeElapsed < timeToFade)
            {
                m_clipSectionTwo.SFXVolume = Mathf.Lerp(0.0f, 1.0f, timeElapsed / timeToFade);
                m_clipSectionOne.SFXVolume = Mathf.Lerp(1.0f, 0.0f, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            m_clipSectionOne.DestroySFX();
        }
        else
        {
            m_clipSectionOne.PlaySFX();

            while (timeElapsed < timeToFade)
            {
                m_clipSectionOne.SFXVolume = Mathf.Lerp(0.0f, 1.0f, timeElapsed / timeToFade);
                m_clipSectionTwo.SFXVolume = Mathf.Lerp(1.0f, 0.0f, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            m_clipSectionTwo.DestroySFX();
        }
        isRunningCrossfade = false;
        isPlayingTrackOne = !isPlayingTrackOne;
    }
    IEnumerator WaitForCoroutineToFinish()
    {
        isRunningCheck = true;
        while (isRunningCrossfade)
            yield return null;
        isRunningCheck = false;
        StartCoroutine(FadeTrack());
    }
}
