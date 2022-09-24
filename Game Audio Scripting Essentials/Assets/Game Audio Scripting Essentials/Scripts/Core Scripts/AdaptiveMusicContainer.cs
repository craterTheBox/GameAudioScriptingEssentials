using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable] public class AudioLayer
{
    [Tooltip("Audio Layer")]
    [SerializeField] public AudioClipRandomizer _audioLayer;
    [Tooltip("Volume of the audio layer")]
    [Range(0.0f, 1.0f)]
    [SerializeField] public float _audioLayerVolumes;
    [Tooltip("Audio Randomizer Container object for each section on this layer")]
    [SerializeField] public AudioRandomizerContainer[] _arcObj;
    [Tooltip("Fuck you go to sleep you bastard")]
    [SerializeField] public SectionTransitions[] _sectionTransitions;
}
[Serializable] public class States
{
    [Range(0.0f, 1.0f)]
    [SerializeField] public float[] _stateAudioLayerVolumes;
}
[Serializable] public class SectionTransitions
{
    [Header("Section Transition")]
    [Tooltip("This is the section that is transitioned into")]
    [SerializeField] int _transitionInto;
    public int TransitionInto 
    { 
        get => _transitionInto;
    }
    public enum TransitionFade { NoFade, Crossfade }
    [Tooltip("Sets whether the transition crossfades the tracks or not")]
    [SerializeField] TransitionFade _fadeType;
    public TransitionFade FadeType
    {
        get => _fadeType;
    }
    public enum TransitionTrigger { OnEnd, OnTrigger }
    [Tooltip("Sets whether the transition happens when the audio clip ends or on a trigger")]
    [SerializeField] TransitionTrigger _triggerType;
    public TransitionTrigger TriggerType
    {
        get => _triggerType;
    }
    public enum TransitionQuantization { Immediate, OnNextBeat, OnNextSecondBeat, OnNextBar }
    [Tooltip("Sets whether the transition occurs immediately or on time with the tempo")]
    [SerializeField] TransitionQuantization _quantization;
    public TransitionQuantization Quantization
    {
        get => _quantization;
    }
}

[AddComponentMenu("Game Audio Scripting Essentials/Adaptive Music Container", 10)]
public class AdaptiveMusicContainer : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Tempo of the audio clip in beats per minute")]
    [Range(0, 300)]
    [SerializeField] int _trackBPM = 120;
    [Tooltip("Number of beats per bar")]
    [Range(1, 20)]
    [SerializeField] int _timeSignatureTop = 4;
    [Tooltip("Note value of the beats")]
    [Range(1, 32)]
    [SerializeField] int _timeSignatureBottom = 4;

    [Header("Sections")]
    [SerializeField] AudioLayer[] _sections;
    [Tooltip("Initial section of the track")]
    [SerializeField] int _initialSection = 0;
    int _currentSection = 0;


    [Header("States")]
    [SerializeField] States[] _states;
    [Tooltip("Initial state of the track. 0 uses the default volume levels")]
    [SerializeField] int _initialState = 0;
    int _currentState = 0;

    [Space]
    [Header("Debug")]
    [SerializeField] bool _ignoreWarnings = false;
    [SerializeField] bool _numbersToChangeState = true;

    void Start()
    {
        //Sets the initial section of the track
        _currentSection = _initialSection;
        for (int i = 0; i < _sections.Length; i++)
        {
            _sections[i]._audioLayer.SetAudioRandomizerContainer(_sections[i]._arcObj[_currentSection]);
        }
        
        //Checks if the layers are the same length
        float _previousAverageLength = -1.0f;

        for (int i = 0; i < _sections.Length; i++)
        {
            if (!_ignoreWarnings && (_previousAverageLength != -1.0f && _previousAverageLength != _sections[i]._audioLayer.GetSFXAverageLength()))
                Debug.LogWarning("WARNING: Audio Layers in obj \"" + this.name + "\" are at differing lengths. This is not a critical error.");
            _previousAverageLength = _sections[i]._audioLayer.GetSFXAverageLength();
        }

        //Plays each of the initial layers at the same time (or at least milliseconds off)
        for (int i = 0; i < _sections.Length; i++)
        {
            _sections[i]._audioLayer.PlaySFX();
        }

        //Sets the initial state of the layers
        _currentState = _initialState;

        if (_currentState == 0)
            for (int i = 0; i < _sections.Length; i++)
                _sections[i]._audioLayer.SetSFXVolume(_sections[i]._audioLayerVolumes);
        else
            for (int i = 0; i < _sections.Length; i++)
                _sections[i]._audioLayer.SetSFXVolume(_states[_currentState - 1]._stateAudioLayerVolumes[i]);
    }

    void Update()
    {
        //State Change - VERTICAL
        if (_numbersToChangeState)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetState(1, true);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SetState(2, true);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SetState(3, true);
        }

        //Section Transition - HORIZONTAL
        if (_numbersToChangeState)
        {
            if (Input.GetKeyDown(KeyCode.Alpha4))
                Transition(1);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                Transition(2);
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                Transition(3);
        }
    }

    void SetState(int newState, bool smooth)
    {
        //Abrupt state change
        if (!smooth)
            for (int i = 0; i < _sections.Length; i++)
                _sections[i]._audioLayer.SetSFXVolume(_states[newState - 1]._stateAudioLayerVolumes[i]);
        else
        {
            //Gradual state change
            IEnumerator GradualStateChange(int state)
            {
                float currentTime = 0.0f;
                float duration = 2.0f;
                float[] currentVolume = new float[_sections.Length];

                if (_currentState == 0)
                    for (int i = 0; i < _sections.Length; i++)
                        currentVolume[i] = _sections[i]._audioLayerVolumes;
                else
                    currentVolume = _states[_currentState - 1]._stateAudioLayerVolumes;


                while (currentTime < duration)
                {
                    for (int i = 0; i < _sections.Length; i++)
                    {
                        if (currentVolume[i] != _states[newState - 1]._stateAudioLayerVolumes[i])
                            _sections[i]._audioLayer.SetSFXVolume(Mathf.Lerp(currentVolume[i], _states[newState - 1]._stateAudioLayerVolumes[i], currentTime / duration));
                    }
                    currentTime += Time.deltaTime;

                    yield return null;
                }

                yield break;
            }
            StartCoroutine(GradualStateChange(newState));
        }
        _currentState = newState;
    }

    //This is called by things in the game to trigger the state change (i.e. through collision, interactions, etc.)
    public void StateChange(int newState)
    {
        SetState(newState, true);
    }

    public void Transition(int transition)
    {
        int newSection = _sections[_currentSection]._sectionTransitions[transition].TransitionInto;
        SectionTransitions.TransitionFade fadeType = _sections[_currentSection]._sectionTransitions[transition].FadeType;
        SectionTransitions.TransitionQuantization quantization = _sections[_currentSection]._sectionTransitions[transition].Quantization;
        SectionTransitions.TransitionTrigger triggerType = _sections[_currentSection]._sectionTransitions[transition].TriggerType;


        for (int i = 0; i < _sections.Length; i++)
        {
            //FUNCTIONAL
            if (fadeType == SectionTransitions.TransitionFade.NoFade)
            {
                _sections[i]._audioLayer.SetAudioRandomizerContainer(_sections[i]._arcObj[newSection]);
                _sections[i]._audioLayer.DestroySFX();
                _sections[i]._audioLayer.PlaySFX();
            }
        }

        

        //play a new SFX at 0 volume, fade old one down and fade new one up if crossfade
        
        _currentSection = newSection;
    }
}