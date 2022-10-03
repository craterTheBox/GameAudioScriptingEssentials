using System;
using System.Collections;
using UnityEngine;
using static SectionTransitions;

[Serializable] public class Section
{
    [HideInInspector] public AudioClipRandomizer[] _audioLayerACR;
    [HideInInspector] public GameObject[] _layerObject;
    [Tooltip("Audio layer")]
    [SerializeField] Layer[] _audioLayers;
    public Layer[] AudioLayers => _audioLayers;
    public int AudioLayerCount() => _audioLayers.Length;
    [Tooltip("Transitions out of this section")]
    [SerializeField] SectionTransitions[] _sectionTransitions;
    public AudioClipRandomizer[] AudioLayerACR => _audioLayerACR;
    public SectionTransitions[] SectionTransitions => _sectionTransitions;
}
[Serializable] public class Layer
{
    [Tooltip("Audio Randomizer Container scriptable object with the audio clips")]
    [SerializeField] AudioRandomizerContainer _arcObj;
    public AudioRandomizerContainer ArcObj
    {
        get => _arcObj;
        set => _arcObj = value;
    }
    [Tooltip("Audio clip assets, replacing the need for an Audio Randomizer Container object")]
    [SerializeField] AudioClip[] _audioClips;
    public AudioClip[] AudioClips => _audioClips;
    [Tooltip("Volume of the audio layer")]
    [Range(0.0f, 1.0f)]
    [SerializeField] public float _audioLayerVolumes = 1.0f;
}
[Serializable] public class States
{
    [Tooltip("Set volume levels for each layer accordingly. This applies to all sections.")]
    [Range(0.0f, 1.0f)]
    [SerializeField] public float[] _stateAudioLayerVolumes;
    public float[] StateAudioLayerVolumes => _stateAudioLayerVolumes;
    [Tooltip("When checked, the volume will fade in/out when transitioning states.")]
    [SerializeField] bool _gradualStateChange = true;
    public bool GradualStateChange => _gradualStateChange;
    [Tooltip("Sets the length of the state transition in seconds. Only applicable when Gradual.")]
    [Range(0.0f, 10.0f)]
    [SerializeField] float _gradualStateChangeTime = 1.0f;
    public float GradualStateChangeTime => _gradualStateChangeTime;
}
[Serializable] public class SectionTransitions
{
    [Header("Section Transition")]
    [Tooltip("This is the section that is transitioned into")]
    [SerializeField] int _transitionInto;
    public int TransitionInto => _transitionInto;
    public enum TransitionTrigger { OnEnd, OnTrigger }
    [Tooltip("Sets whether the transition happens when the audio clip ends or on a trigger.\n" +
        "\nOnEnd: Transitions when the audio clip ends. This setting only works when not looped." +
        "\nOnTrigger: Transitions when the transition is triggered.")]
    [SerializeField] TransitionTrigger _triggerType;
    public TransitionTrigger TriggerType => _triggerType;
    public enum TransitionFade { NoFade, Crossfade }
    [Tooltip("Sets whether the transition crossfades the tracks or not.\n" +
        "\nNoFade: Stops one clip and starts the other." +
        "\nCrossfade: Blends both clips together, stops previous clip once silent.")]
    [SerializeField] TransitionFade _fadeType;
    public TransitionFade FadeType => _fadeType;
    public enum TransitionQuantization { Immediate, OnNextBeat, OnNextSecondBeat, OnNextBar }
    [Tooltip("Sets whether the transition occurs immediately or on time with the tempo. This setting is bypassed by \"OnEnd\" trigger type.\n" +
        "\nImmediate: Immediately transitions sections." +
        "\nOnNextBeat: Transitions on any beat." +
        "\nOnNextSecondBeat: Transitions on any even-numbered beat." +
        "\nOnNextBar: Transitions on the next beat 1.")]
    [SerializeField] TransitionQuantization _quantization;
    public TransitionQuantization Quantization => _quantization;
    [Tooltip("Sets the length of the transition in seconds. Only applicable to Crossfades.")]
    [Range(0.0f, 10.0f)]
    [SerializeField] float _crossfadeTime = 1.0f;
    public float CrossfadeTime => _crossfadeTime;
}

[AddComponentMenu("Game Audio Scripting Essentials/Adaptive Music Container", 10)]
public class AdaptiveMusicContainer : MonoBehaviour
{
    public enum PlayType { Automatic, Manual }
    [Header("Audio Settings")]
    [Tooltip("Determines whether this container will play the audio automatically on enable or on trigger. \n\nAutomatic: Plays when the scene is run. \nManual: Plays when triggered.")]
    [SerializeField] PlayType _playType = PlayType.Automatic;
    [Tooltip("Tempo of the audio clip in beats per minute. This is required for quantization to work properly.")]
    [Range(0, 300)]
    [SerializeField] int _trackBPM = 120;
    [Tooltip("Number of beats per bar")]
    [Range(1, 20)]
    [SerializeField] int _timeSignatureTop = 4;
    [Tooltip("Note value of the beats")]
    [Range(1, 32)]
    [SerializeField] int _timeSignatureBottom = 4;
    bool _isPlaying = false;

    [Header("Sections")]
    [SerializeField] Section[] _sections;
    [Tooltip("Initial section of the track")]
    [SerializeField] int _initialSection = 0;
    int _currentSection = 0;
    bool _isRunningCrossfade, _isRunningCrossfadeCheck, _isRunningQuantize, _isRunningQuantizeCheck = false;

    [Header("States")]
    [SerializeField] States[] _states;
    [Tooltip("Initial state of the track. If no states are provided, it uses the default volume levels.")]
    [SerializeField] int _initialState = 0;
    int _currentState = 0;

    [Space]
    [Header("Debug")]
    [Tooltip("Warnings will not appear in the console when toggled on.")]
    [SerializeField] bool _ignoreWarnings = false;

    void Start()
    {
        if (_playType == PlayType.Automatic)
        {
            RunContainer();
        }
    }
    public void RunContainer()
    {
        if (!_isPlaying)
        {
            _isPlaying = true;
            InitializeSection(_initialSection);
            _currentSection = _initialSection;
            if (_playType == PlayType.Automatic)
            {
                SetStateImmediate(_initialState);
            }
            else
            {
                SetState(_initialState);
            }
        }
    }

    void InitializeSection(int _newSection)
    {
        //Check for ARC Object or create one using the audio clips
        for (int i = 0; i < _sections[_newSection].AudioLayers.Length; i++)
        {
            if (_sections[_newSection].AudioLayers[i].ArcObj.AudioClips.Length > 0)
            {
                break;
            }
            //If no ARC Object, this will create one using the clips provided (granted, with default settings)
            else if (_sections[_newSection].AudioLayers[i].ArcObj.AudioClips.Length == 0 && _sections[_newSection].AudioLayers[i].AudioClips.Length != 0)
            {
                _sections[_newSection].AudioLayers[i].ArcObj = ScriptableObject.CreateInstance<AudioRandomizerContainer>();
                _sections[_newSection].AudioLayers[i].ArcObj.AudioClips = _sections[_newSection].AudioLayers[i].AudioClips;
                //Now it has an ARC Object. You're welcome
            }
            else
            {
                Debug.LogWarning("WARNING: No audio clips or Audio Randomizer Container found.");
            }
        }

        _sections[_newSection]._layerObject = new GameObject[_sections[_newSection].AudioLayerCount()];
        _sections[_newSection]._audioLayerACR = new AudioClipRandomizer[_sections[_newSection].AudioLayerCount()];

        //Plays the SFX
        for (int i = 0; i < _sections[_newSection].AudioLayers.Length; i++)
        {
            string _name = "Section " + _newSection + ", Layer " + i;

            _sections[_newSection]._layerObject[i] = new GameObject(_name);
            _sections[_newSection]._layerObject[i].transform.SetParent(transform);
            _sections[_newSection]._audioLayerACR[i] = _sections[_newSection]._layerObject[i].AddComponent<AudioClipRandomizer>();
            _sections[_newSection]._audioLayerACR[i].ArcObj = _sections[_newSection].AudioLayers[i].ArcObj;
            _sections[_newSection]._audioLayerACR[i].OverrideArcSettings = false;
            _sections[_newSection]._audioLayerACR[i].PlaySFX();

            ///////////////

            if (_sections[_newSection].AudioLayerCount() <= _states[_currentState].StateAudioLayerVolumes.Length || i < _states[_currentState].StateAudioLayerVolumes.Length)
            {
                _sections[_newSection].AudioLayerACR[i].SFXVolume = _states[_currentState].StateAudioLayerVolumes[i];
            }
            else if (_states[_currentState].StateAudioLayerVolumes.Length <= i)
            {
                _sections[_newSection].AudioLayerACR[i].SFXVolume = _sections[_newSection].AudioLayers[i]._audioLayerVolumes;
            }
        }

        //Checks if the layers are the same length
        float _previousAverageLength = -1.0f;

        for (int i = 0; i < _sections[_newSection].AudioLayers.Length; i++)
        {
            if (!_ignoreWarnings && _previousAverageLength != -1.0f && _previousAverageLength != _sections[_newSection]._audioLayerACR[i].GetSFXAverageLength())
            {
                Debug.LogWarning("WARNING: Audio Layers in obj \"" + name + "\" are at differing lengths. This is not a critical error.");
            }
            _previousAverageLength = _sections[_newSection]._audioLayerACR[i].GetSFXAverageLength();
        }
    }

    void Update()
    {
        if (_isPlaying)
        {
            //Checks for if the section has a transition trigger OnEnd
            for (int i = 0; i < _sections[_currentSection].SectionTransitions.Length; i++)
            {
                if (_sections[_currentSection].SectionTransitions[i].TriggerType == SectionTransitions.TransitionTrigger.OnEnd && !_sections[_currentSection]._audioLayerACR[0].Loop)
                {
                    TransitionSection(i);
                    break;
                }
            }
        }
    }

    public void SetState(int _newState)
    {
        if (_sections.Length == 0 ^ _sections[_currentSection]._layerObject.Length == 0)
        {
            if (!_ignoreWarnings)
            {
                Debug.Log("Attempting to switch states without anything playing.");
            }
            return;
        }
        //Sets the current state of the layers
        if (_newState >= _states.Length)
        {
            for (int i = 0; i < _states[_currentState]._stateAudioLayerVolumes.Length; i++)
            {
                if (_sections[_currentSection].AudioLayerCount() <= _states[_newState].StateAudioLayerVolumes.Length || i < _states[_currentState].StateAudioLayerVolumes.Length)
                {
                    _sections[_currentSection].AudioLayerACR[i].SFXVolume = _states[_newState].StateAudioLayerVolumes[i];
                }
                else if (_states[_newState].StateAudioLayerVolumes.Length <= i)
                {
                    _sections[_currentSection].AudioLayerACR[i].SFXVolume = _sections[_currentSection].AudioLayers[i]._audioLayerVolumes;
                }
            }
        }

        //Abrupt state change
        if (!_states[_newState].GradualStateChange)
        {
            for (int i = 0; i < _sections[_currentSection].AudioLayerCount(); i++)
            {
                if (_sections[_currentSection].AudioLayerCount() <= _states[_newState].StateAudioLayerVolumes.Length || i < _states[_currentState].StateAudioLayerVolumes.Length)
                {
                    _sections[_currentSection].AudioLayerACR[i].SFXVolume = _states[_newState].StateAudioLayerVolumes[i];
                }
                else if (_states[_newState].StateAudioLayerVolumes.Length <= i)
                {
                    _sections[_currentSection].AudioLayerACR[i].SFXVolume = _sections[_currentSection].AudioLayers[i]._audioLayerVolumes;
                }
            }
        }
        else
        {
            //Gradual state change
            IEnumerator GradualStateChange(int _newState)
            {
                float _currentTime = 0.0f;
                float[] _currentVolume = new float[_sections[_currentSection].AudioLayerCount()];

                for (int i = 0; i < _sections[_currentSection].AudioLayerCount(); i++)
                {
                    _currentVolume[i] = _sections[_currentSection].AudioLayerACR[i].SFXVolume;
                }

                while (_currentTime < _states[_newState].GradualStateChangeTime)
                {
                    for (int i = 0; i < _sections[_currentSection].AudioLayerCount(); i++)
                    {
                        if (_sections[_currentSection].AudioLayerCount() <= _states[_newState].StateAudioLayerVolumes.Length || i < _states[_newState].StateAudioLayerVolumes.Length)
                        {
                            if (_currentVolume[i] != _states[_newState].StateAudioLayerVolumes[i])
                            {
                                _sections[_currentSection].AudioLayerACR[i].SFXVolume = Mathf.Lerp(_currentVolume[i], _states[_newState].StateAudioLayerVolumes[i], _currentTime / _states[_newState].GradualStateChangeTime);
                            }
                        }
                        else if (_states[_currentState].StateAudioLayerVolumes.Length <= i)
                        {
                            if (_currentVolume[i] != _sections[_currentSection].AudioLayers[i]._audioLayerVolumes)
                            {
                                _sections[_currentSection].AudioLayerACR[i].SFXVolume = Mathf.Lerp(_currentVolume[i], _sections[_currentSection].AudioLayers[i]._audioLayerVolumes, _currentTime / _states[_newState].GradualStateChangeTime);
                            }
                        }
                    }

                    _currentTime += Time.deltaTime;

                    yield return null;
                }

                yield break;
            }
            StartCoroutine(GradualStateChange(_newState));
        }
        _currentState = _newState;
    }
    public void SetStateImmediate(int _newState)
    {
        if (_sections.Length == 0 ^ _sections[_currentSection]._layerObject.Length == 0)
        {
            if (!_ignoreWarnings)
            {
                Debug.Log("Attempting to switch states without anything playing.");
            }
            return;
        }
        if (_states[_newState] == null)
        {
            for (int i = 0; i < _states[_currentState].StateAudioLayerVolumes.Length; i++)
            {
                if (_sections[_currentSection].AudioLayerCount() <= _states[_newState].StateAudioLayerVolumes.Length)
                {
                    _sections[_currentSection].AudioLayerACR[i].SFXVolume = _states[_currentState].StateAudioLayerVolumes[i];
                }
                else if (_states[_newState].StateAudioLayerVolumes.Length <= i)
                {
                    _sections[_currentSection].AudioLayerACR[i].SFXVolume = _sections[_currentSection].AudioLayers[i]._audioLayerVolumes;
                }
            }

            return;
        }
        //Sets the initial volume abruptly
        for (int i = 0; i < _sections[_currentSection].AudioLayerCount(); i++)
        {
            if (_sections[_currentSection].AudioLayerCount() <= _states[_newState].StateAudioLayerVolumes.Length)
            {
                _sections[_currentSection].AudioLayerACR[i].SFXVolume = _states[_newState].StateAudioLayerVolumes[i];
            }
            else if (_states[_newState].StateAudioLayerVolumes.Length <= i)
            {
                _sections[_currentSection].AudioLayerACR[i].SFXVolume = _sections[_currentSection].AudioLayers[i]._audioLayerVolumes;
            }
        }

        _currentState = _newState;
    }

    //This will transition out of the current state using the index provided into whatever section is set by that transition. 
    //If no transition state at that index exists, it will not transition. 
    public void TransitionSection(int _transitionIndex)
    {
        if (_sections.Length == 0 ^ _sections[_currentSection]._layerObject.Length == 0)
        {
            if (!_ignoreWarnings)
            {
                Debug.Log("Attempting to transition without anything playing.");
            }
            return;
        }
        if (_currentSection == _sections[_currentSection].SectionTransitions[_transitionIndex].TransitionInto)
        {
            if (!_ignoreWarnings)
            {
                Debug.Log("Attempting to transition into the current region.");
            }
            return;
        }
        if (_sections[_currentSection].SectionTransitions.Length <= _transitionIndex)
        {
            if (!_ignoreWarnings)
            {
                Debug.LogWarning("WARNING: Transition Index does not exist. Continuing on this section.");
            }
            return;
        }

        int _newSection = _sections[_currentSection].SectionTransitions[_transitionIndex].TransitionInto;
        float _crossfadeTime = _sections[_currentSection].SectionTransitions[_transitionIndex].CrossfadeTime;

        SectionTransitions.TransitionTrigger triggerType = _sections[_currentSection].SectionTransitions[_transitionIndex].TriggerType;
        SectionTransitions.TransitionFade fadeType = _sections[_currentSection].SectionTransitions[_transitionIndex].FadeType;
        SectionTransitions.TransitionQuantization quantization = _sections[_currentSection].SectionTransitions[_transitionIndex].Quantization;

        //Trigger: OnTrigger
        if (triggerType == SectionTransitions.TransitionTrigger.OnTrigger)
        {
            //Fade: NoFade
            if (fadeType == SectionTransitions.TransitionFade.NoFade)
            {
                //Quant: Immediate
                if (quantization == SectionTransitions.TransitionQuantization.Immediate)
                {
                    if (!_ignoreWarnings)
                    {
                        Debug.Log("OnTrigger, NoFade, Immediate.");
                    }

                    InitializeSection(_newSection);
                    for (int i = 0; i < _sections[_currentSection].AudioLayerACR.Length; i++)
                    {
                        Destroy(_sections[_currentSection]._layerObject[i]);
                    }
                    _currentSection = _newSection;
                }
                //Quant: All Others
                else
                {
                    if (!_ignoreWarnings)
                    {
                        Debug.Log("OnTrigger, NoFade, " + quantization);
                    }

                    if (!_isRunningQuantize && !_isRunningQuantizeCheck)
                    {
                        StartCoroutine(Quantize(_newSection, quantization, fadeType, _crossfadeTime));
                    }
                    else if (_isRunningQuantize && !_isRunningQuantizeCheck)
                    {
                        StartCoroutine(WaitForQuantize(_newSection, quantization, fadeType, _crossfadeTime));
                    }
                }
            }
            //Fade: Crossfade
            else if (fadeType == SectionTransitions.TransitionFade.Crossfade)
            {
                //Quant: Immediate // WORKS
                if (quantization == SectionTransitions.TransitionQuantization.Immediate)
                {
                    if (!_ignoreWarnings)
                    {
                        Debug.Log("OnTrigger, Crossfade, Immediate.");
                    }

                    if (!_isRunningCrossfade && !_isRunningCrossfadeCheck)
                    {
                        StartCoroutine(Crossfade(_newSection, _crossfadeTime));
                    }
                    else if (_isRunningCrossfade && !_isRunningCrossfadeCheck)
                    {
                        StartCoroutine(WaitForCrossfade(_newSection, _crossfadeTime));
                    }
                }
                //Quant: All Others
                else
                {
                    if (!_ignoreWarnings)
                    {
                        Debug.Log("OnTrigger, Crossfade, " + quantization);
                    }

                    if (!_isRunningQuantize && !_isRunningQuantizeCheck)
                    {
                        StartCoroutine(Quantize(_newSection, quantization, fadeType, _crossfadeTime));
                    }
                    else if (_isRunningQuantize && !_isRunningQuantizeCheck)
                    {
                        StartCoroutine(WaitForQuantize(_newSection, quantization, fadeType, _crossfadeTime));
                    }
                }
            }
        }
        //Trigger: OnEnd
        else if (triggerType == SectionTransitions.TransitionTrigger.OnEnd)
        {
            IEnumerator NoFadeOnEnd()
            {
                float _longestLength = 0.0f;
                int _index = 0;

                for (int i = 0; i < _sections[_currentSection].AudioLayerACR.Length; i++)
                {
                    if (_longestLength < _sections[_currentSection].AudioLayerACR[i].GetSFXLongestLength())
                    {
                        _longestLength = _sections[_currentSection].AudioLayerACR[i].GetSFXLongestLength();
                        _index = i;
                    }
                }
                //Checks the longest layer to see if it is playing
                while (_sections[_currentSection].AudioLayerACR[_index].IsSFXPlaying())
                {
                    yield return null;
                }

                if (!_ignoreWarnings)
                {
                    Debug.Log("NoFade, OnEnd, Any Quantization.");
                }

                InitializeSection(_newSection);
                for (int i = 0; i < _sections[_currentSection].AudioLayerACR.Length; i++)
                {
                    Destroy(_sections[_currentSection]._layerObject[i]);
                }

                _currentSection = _newSection;
                SetStateImmediate(_currentState);
            }
            StartCoroutine(NoFadeOnEnd());
        }
        //Failsafe
        else
        {
            if (!_ignoreWarnings)
            {
                Debug.Log("You missed a case, dummy");
            }

            InitializeSection(_newSection);
            for (int i = 0; i < _sections[_currentSection].AudioLayerACR.Length; i++)
            {
                Destroy(_sections[_currentSection]._layerObject[i]);
            }
        }
    }
    IEnumerator Crossfade(int _newSection, float _crossfadeTime)
    {
        _isRunningCrossfade = true;

        float _currentTime = 0.0f;

        InitializeSection(_newSection);

        while (_currentTime < _crossfadeTime)
        {
            if (_sections[_currentSection].AudioLayerACR.Length == _sections[_newSection].AudioLayerACR.Length)
            {
                for (int i = 0; i < _sections[_currentSection].AudioLayerACR.Length; i++)
                {
                    if (_sections[_currentSection].AudioLayerCount() <= _states[_currentState].StateAudioLayerVolumes.Length || i < _states[_currentState].StateAudioLayerVolumes.Length)
                    {
                        _sections[_newSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(0.0f, 1.0f * _states[_currentState].StateAudioLayerVolumes[i], _currentTime / _crossfadeTime);

                        if (_sections[_currentSection]._layerObject[i].GetComponent<AudioSource>())
                        {
                            _sections[_currentSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(1.0f * _states[_currentState].StateAudioLayerVolumes[i], 0.0f, _currentTime / _crossfadeTime);
                        }
                    }
                    else if (_states[_currentState].StateAudioLayerVolumes.Length <= i)
                    {
                        _sections[_newSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(0.0f, 1.0f * _sections[_currentSection].AudioLayers[i]._audioLayerVolumes, _currentTime / _crossfadeTime);

                        if (_sections[_currentSection]._layerObject[i].GetComponent<AudioSource>())
                        {
                            _sections[_currentSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(1.0f * _sections[_currentSection].AudioLayers[i]._audioLayerVolumes, 0.0f, _currentTime / _crossfadeTime);
                        }
                    }
                }
            }
            //Goes down here when the sections have a different number of layers
            else if (_sections[_currentSection].AudioLayerACR.Length != _sections[_newSection].AudioLayerACR.Length)
            {
                //Index of the section with more layers
                int _mostLayers = (_sections[_currentSection].AudioLayerACR.Length > _sections[_newSection].AudioLayerACR.Length) ? _currentSection : _newSection;
                int _leastLayers = (_sections[_currentSection].AudioLayerACR.Length < _sections[_newSection].AudioLayerACR.Length) ? _currentSection : _newSection;

                for (int i = 0; i < _sections[_mostLayers].AudioLayerACR.Length; i++)
                {
                    //If the section with the least layers has a layer count of less than or equal to i (layer count), only update the section with more layers
                    if (_sections[_leastLayers].AudioLayerACR.Length <= i)
                    {
                        if (_leastLayers == _currentSection)
                        {
                            if (_sections[_newSection].AudioLayerCount() <= _states[_currentState].StateAudioLayerVolumes.Length || i < _states[_currentState].StateAudioLayerVolumes.Length)
                            {
                                _sections[_newSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(0.0f, 1.0f * _states[_currentState].StateAudioLayerVolumes[i], _currentTime / _crossfadeTime);
                            }
                            else if (_states[_currentState].StateAudioLayerVolumes.Length <= i)
                            {
                                _sections[_newSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(0.0f, 1.0f * _sections[_newSection].AudioLayers[i]._audioLayerVolumes, _currentTime / _crossfadeTime);
                            }
                        }
                        else
                        {
                            if (_sections[_currentSection]._layerObject[i].GetComponent<AudioSource>() && _sections[_currentSection].AudioLayerCount() <= _states[_currentState].StateAudioLayerVolumes.Length || i < _states[_currentState].StateAudioLayerVolumes.Length)
                            {
                                _sections[_currentSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(1.0f * _states[_currentState].StateAudioLayerVolumes[i], 0.0f, _currentTime / _crossfadeTime);
                            }
                            else if (_sections[_currentSection]._layerObject[i].GetComponent<AudioSource>() && _states[_currentState].StateAudioLayerVolumes.Length <= i)
                            {
                                _sections[_currentSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(1.0f * _sections[_currentSection].AudioLayers[i]._audioLayerVolumes, 0.0f, _currentTime / _crossfadeTime);
                            }
                        }
                    }
                    //Otherwise it will just do the normal thing and update both
                    else
                    {
                        _sections[_newSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(0.0f, 1.0f * _states[_currentState].StateAudioLayerVolumes[i], _currentTime / _crossfadeTime);

                        if (_sections[_currentSection]._layerObject[i].GetComponent<AudioSource>())
                        {
                            _sections[_currentSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(1.0f * _states[_currentState].StateAudioLayerVolumes[i], 0.0f, _currentTime / _crossfadeTime);
                        }
                    }
                }
            }

            _currentTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < _sections[_currentSection]._layerObject.Length; i++)
        {
            Destroy(_sections[_currentSection]._layerObject[i]);
        }

        _isRunningCrossfade = false;
        _currentSection = _newSection;
    }
    IEnumerator WaitForCrossfade(int _newSection, float _crossfadeTime)
    {
        _isRunningCrossfadeCheck = true;
        while (_isRunningCrossfade)
        {
            yield return null;
        }

        _isRunningCrossfadeCheck = false;
        StartCoroutine(Crossfade(_newSection, _crossfadeTime));
    }
    IEnumerator Quantize(int _newSection, TransitionQuantization _quantization, TransitionFade _fade, float _crossfadeTime)
    {
        _isRunningQuantize = true;

        float _timeToNextBeat = (60.0f / _trackBPM) / (_timeSignatureBottom / 4);
        float _timeToNextBar = _timeToNextBeat * _timeSignatureTop;

        if (_quantization == TransitionQuantization.OnNextBeat)
        {
            while ((float)Math.Round(_sections[_currentSection].AudioLayerACR[0].SFXPlayPosition, 1) % _timeToNextBeat != 0)
            {
                yield return null;
            }
        }
        else if (_quantization == TransitionQuantization.OnNextSecondBeat)
        {
            while ((float)Math.Round(_sections[_currentSection].AudioLayerACR[0].SFXPlayPosition, 1) % _timeToNextBeat * 2 != 0)
            {
                yield return null;
            }
        }
        else if (_quantization == TransitionQuantization.OnNextBar)
        {
            while ((float)Math.Round(_sections[_currentSection].AudioLayerACR[0].SFXPlayPosition, 1) % _timeToNextBar != 0)
            {
                yield return null;
            }
        }
        if (_fade == TransitionFade.NoFade)
        {
            InitializeSection(_newSection);
            for (int i = 0; i < _sections[_currentSection].AudioLayerACR.Length; i++)
            {
                Destroy(_sections[_currentSection]._layerObject[i]);
            }
        }
        else
        {
            if (!_ignoreWarnings)
            {
                Debug.Log("OnTrigger, Crossfade, Immediate.");
            }

            if (!_isRunningCrossfade && !_isRunningCrossfadeCheck)
            {
                StartCoroutine(Crossfade(_newSection, _crossfadeTime));
            }
            else if (_isRunningCrossfade && !_isRunningCrossfadeCheck)
            {
                StartCoroutine(WaitForCrossfade(_newSection, _crossfadeTime));
            }
        }
        _isRunningQuantize = false;
    }
    IEnumerator WaitForQuantize(int _newSection, TransitionQuantization _quantization, TransitionFade _fade, float _crossfadeTime)
    {
        _isRunningQuantizeCheck = true;
        while (_isRunningQuantize)
        {
            yield return null;
        }

        _isRunningQuantizeCheck = false;
        StartCoroutine(Quantize(_newSection, _quantization, _fade, _crossfadeTime));
    }
}