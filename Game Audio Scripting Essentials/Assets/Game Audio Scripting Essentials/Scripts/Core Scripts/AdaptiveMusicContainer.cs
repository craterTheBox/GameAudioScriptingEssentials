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
    [Tooltip("Transitions out of this section")]
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
    [Tooltip("Sets whether the transition crossfades the tracks or not.\n" +
        "\nNoFade: Stops one clip and starts the other." +
        "\nCrossfade: Blends both clips together, stops previous clip once silent.")]
    [SerializeField] TransitionFade _fadeType;
    public TransitionFade FadeType
    {
        get => _fadeType;
    }
    public enum TransitionTrigger { OnEnd, OnTrigger }
    [Tooltip("Sets whether the transition happens when the audio clip ends or on a trigger.\n" +
        "\nOnEnd: Transitions when the audio clip ends." +
        "\nOnTrigger: Transitions when the transition is triggered.")]
    [SerializeField] TransitionTrigger _triggerType;
    public TransitionTrigger TriggerType
    {
        get => _triggerType;
    }
    public enum TransitionQuantization { Immediate, OnNextBeat, OnNextSecondBeat, OnNextBar }
    [Tooltip("Sets whether the transition occurs immediately or on time with the tempo. This setting is bypassed by \"OnEnd\" trigger type.\n" +
        "\nImmediate: Immediately transitions sections." +
        "\nOnNextBeat: Transitions on any beat." +
        "\nOnNextSecondBeat: Transitions on any even-numbered beat." +
        "\nOnNextBar: Transitions on the next beat 1.")]
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
    //Metronome _metronome;

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
    [SerializeField] bool _playMetronome = false;

    void Start()
    {
        /*/Metronome
        _metronome = GetComponent<Metronome>();
        _metronome.BeatsPerMinute = _trackBPM;
        _metronome.TopTimeSignature = _timeSignatureTop;
        _metronome.BottomTimeSignature = _timeSignatureBottom;
        //_metronome._beat.SetSFXVolume(_playMetronome ? 1.0f : 0.0f);
        //_metronome._beatOne.SetSFXVolume(_playMetronome ? 1.0f : 0.0f);
        //*/

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
        //Metronome
        //_metronome._beat.SetSFXVolume((_playMetronome) ? 1.0f : 0.0f);
        //_metronome._beatOne.SetSFXVolume((_playMetronome) ? 1.0f : 0.0f);

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
        SectionTransitions.TransitionTrigger triggerType = _sections[_currentSection]._sectionTransitions[transition].TriggerType;
        SectionTransitions.TransitionQuantization quantization = _sections[_currentSection]._sectionTransitions[transition].Quantization;

        //FadeType = NoFade, Crossfade
        //TriggerType = OnEnd, OnTrigger
        //Quantization = Immediate, OnNextBeat, OnNextSecondBeat, OnNextBar

        //NoFade, OnTrigger, Immediate Quantization
        if (fadeType == SectionTransitions.TransitionFade.NoFade && 
            triggerType == SectionTransitions.TransitionTrigger.OnTrigger && 
            quantization == SectionTransitions.TransitionQuantization.Immediate)
        {
            for (int i = 0; i < _sections.Length; i++)
            {
                _sections[i]._audioLayer.SetAudioRandomizerContainer(_sections[i]._arcObj[newSection]);
                if (!_ignoreWarnings) Debug.Log("NoFade, OnTrigger, Immediate.");
                _sections[i]._audioLayer.DestroySFX();
                _sections[i]._audioLayer.PlaySFX();
            }
        }
        //Everything else that isn't no fade, on trigger, and immediate
        else
        {
            StartCoroutine(TransitionCoroutine(newSection, fadeType, triggerType, quantization));
        }

        StateChange(_currentState);

        _currentSection = newSection;
    }

    //I'm very sorry if you look at this script, but this is what you get for wanting adaptive music from a not-mainly programmer
    IEnumerator TransitionCoroutine(int newSection, SectionTransitions.TransitionFade fadeType, SectionTransitions.TransitionTrigger triggerType, SectionTransitions.TransitionQuantization quantization)
    {
        while (true)
        {
            //FadeType: NoFade
            if (fadeType == SectionTransitions.TransitionFade.NoFade)
            {
                /*/TriggerType: OnTrigger
                if (triggerType == SectionTransitions.TransitionTrigger.OnTrigger)
                {
                    //Quantization: OnNextBeat
                    if (quantization == SectionTransitions.TransitionQuantization.OnNextBeat)
                    {
                        while (!_metronome._beatOne.SFXStartedPlaying() ^ !_metronome._beat.SFXStartedPlaying())
                        {
                            yield return null;
                        }
                        if (!_ignoreWarnings) Debug.Log("NoFade, OnEnd, OnNextBeat.");
                        DestroyOldPlayNew(newSection);
                        yield break;
                    }
                    //Quantization: OnNextSecondBeat
                    else if (quantization == SectionTransitions.TransitionQuantization.OnNextSecondBeat)
                    {
                        while (_metronome.BeatCount % 2 != 0
                            //&& !_metronome._beat.SFXStartedPlaying()
                            )
                        {
                            yield return null;
                        }
                        if (!_ignoreWarnings) Debug.Log("NoFade, OnEnd, OnNextSecondBeat.");
                        DestroyOldPlayNew(newSection);
                        yield break;
                    }
                    //Quantization: OnNextBar
                    else if (quantization == SectionTransitions.TransitionQuantization.OnNextBar)
                    {
                        while (//!_metronome._beatOne.SFXStartedPlaying() &&
                            _metronome.BeatCount != 1)
                        {
                            yield return null;
                        }
                        if (!_ignoreWarnings) Debug.Log("NoFade, OnEnd, OnNextBar.");
                        DestroyOldPlayNew(newSection);
                        yield break;
                    }
                }
                //TriggerType: OnEnd. Bypasses all Quantization settings
                else //*/
                if (triggerType == SectionTransitions.TransitionTrigger.OnEnd)
                {
                    while (_sections[0]._audioLayer.IsSFXPlaying())
                    {
                        yield return null;
                    }
                    if (!_ignoreWarnings) Debug.Log("NoFade, OnEnd, any quant.");
                    DestroyOldPlayNew(newSection);
                    yield break;
                }
            }
            //FadeType: Crossfade
            else if (fadeType == SectionTransitions.TransitionFade.Crossfade)
            {
                //TriggerType: OnTrigger
                if (triggerType == SectionTransitions.TransitionTrigger.OnTrigger)
                {
                    /*/Quantization: OnNextBeat
                    if (quantization == SectionTransitions.TransitionQuantization.OnNextBeat)
                    {
                        while (!_metronome._beatOne.SFXStartedPlaying() ^ !_metronome._beat.SFXStartedPlaying())
                        {
                            yield return null;
                        }
                        if (!_ignoreWarnings) Debug.Log("Crossfade, OnEnd, OnNextBeat.");
                        DestroyOldPlayNewCrossfade(newSection);
                        yield break;
                    }
                    //Quantization: OnNextSecondBeat
                    else if (quantization == SectionTransitions.TransitionQuantization.OnNextSecondBeat)
                    {
                        while (_metronome.BeatCount % 2 != 0 && !_metronome._beat.SFXStartedPlaying())
                        {
                            yield return null;
                        }
                        if (!_ignoreWarnings) Debug.Log("Crossfade, OnEnd, OnNextSecondBeat.");
                        DestroyOldPlayNewCrossfade(newSection);
                        yield break;
                    }
                    //Quantization: OnNextBar
                    else if (quantization == SectionTransitions.TransitionQuantization.OnNextBar)
                    {
                        while (!_metronome._beatOne.SFXStartedPlaying())
                        {
                            yield return null;
                        }
                        if (!_ignoreWarnings) Debug.Log("Crossfade, OnEnd, OnNextBar.");
                        DestroyOldPlayNewCrossfade(newSection);
                        yield break;
                    }//*/
                }
                //TriggerType: OnEnd. Bypasses all Quantization settings
                else if (triggerType == SectionTransitions.TransitionTrigger.OnEnd)
                {
                    while (_sections[0]._audioLayer.IsSFXPlaying())
                    {
                        yield return null;
                    }
                    if (!_ignoreWarnings) Debug.Log("Crossfade, OnEnd, any quant.");
                    DestroyOldPlayNewCrossfade(newSection);
                    yield break;
                }
            }
            //In case I forgot about anything, just play the damn thing
            else
            {
                if (!_ignoreWarnings) Debug.Log("You missed a case dummy");
                DestroyOldPlayNew(newSection);
                yield break;
            }
        }
    }
    void DestroyOldPlayNew(int newSection)
    {
        for (int i = 0; i < _sections.Length; i++)
        {
            _sections[i]._audioLayer.SetAudioRandomizerContainer(_sections[i]._arcObj[newSection]);

            _sections[i]._audioLayer.DestroySFX();
            _sections[i]._audioLayer.PlaySFX();
        }
    }
    void DestroyOldPlayNewCrossfade(int newSection)
    {
        for (int i = 0; i < _sections.Length; i++)
        {
            _sections[i]._audioLayer.SetAudioRandomizerContainer(_sections[i]._arcObj[newSection]);
            _sections[i]._audioLayer.PlaySFX();
            _sections[i]._audioLayer.SetSFXVolume(0.0f, _sections[i]._audioLayer.GetComponents<AudioSource>()[1]);
        }
        StartCoroutine(FadeVolume(false));
        StartCoroutine(FadeVolume(true)); 
    }
    IEnumerator FadeVolume(bool up)
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
            if (up) //FADE OUT SOURCE 0
            {
                for (int i = 0; i < _sections.Length; i++)
                {
                    _sections[i]._audioLayer.SetSFXVolume(Mathf.Lerp(currentVolume[i], 0.0f, currentTime / duration), _sections[i]._audioLayer.GetComponents<AudioSource>()[0]);

                    if (_sections[i]._audioLayer.GetSFXVolume(_sections[i]._audioLayer.GetComponents<AudioSource>()[0]) <= 0.01f)
                        _sections[i]._audioLayer.DestroySFX(_sections[i]._audioLayer.GetComponents<AudioSource>()[0]);
                }
            }
            else //FADE IN SOURCE 1
            {
                for (int i = 0; i < _sections.Length; i++)
                {
                    _sections[i]._audioLayer.SetSFXVolume(Mathf.Lerp(0.0f, _states[_currentState]._stateAudioLayerVolumes[i], currentTime / duration), _sections[i]._audioLayer.GetComponents<AudioSource>()[1]);
                }
            }
            currentTime += Time.deltaTime;

            yield return null;
        }
        yield break;
    }
}