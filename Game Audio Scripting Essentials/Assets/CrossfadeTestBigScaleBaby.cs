using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

[Serializable] public class Section
{
    [HideInInspector] public AudioClipRandomizer[] _audioLayerACR;
    [HideInInspector] public GameObject[] _layerObject;
    [SerializeField] Layer[] _audioLayers;
    [Tooltip("Transitions out of this section")]
    [SerializeField] SectionTransitions[] _sectionTransitions;

    public Layer[] AudioLayers
    {
        get => _audioLayers;
    }
    public int AudioLayerCount() => _audioLayers.Length;
    public SectionTransitions[] SectionTransitions => _sectionTransitions;
}
[Serializable] public class Layer
{
    [Tooltip("Audio Randomizer Container scriptable object with the audio clips")]
    [SerializeField] AudioRandomizerContainer _arcObj;
    [Tooltip("Audio clip assets, replacing the need for an Audio Randomizer Container object")]
    [SerializeField] AudioClip[] _audioClips;
    [Tooltip("Volume of the audio layer")]
    [Range(0.0f, 1.0f)]
    [SerializeField] public float _audioLayerVolumes = 1.0f;

    public AudioRandomizerContainer ArcObj
    {
        get => _arcObj;
        set => _arcObj = value;
    }
    public AudioClip[] AudioClips
    {
        get => _audioClips;
    }
}

public class CrossfadeTestBigScaleBaby : MonoBehaviour
{
    [SerializeField] Section[] _sections;
    [SerializeField] int _initialSection = 0;
    int _currentSection = 0;
    bool _isRunningCrossfade, _isRunningCrossfadeCheck = false;

    void Start()
    {
        InitializeSection(_initialSection);
        _currentSection = _initialSection;
    }
    void InitializeSection(int _newSection)
    {
        for (int i = 0; i < _sections[_newSection].AudioLayers.Length; i++)
        {
            //Uses the ARC Object by default if one is available
            if (_sections[_newSection].AudioLayers[i].ArcObj != null)
            {

            }
            else if (_sections[_newSection].AudioLayers[i].AudioClips != null)
            {
                _sections[_newSection].AudioLayers[i].ArcObj = ScriptableObject.CreateInstance<AudioRandomizerContainer>();
                _sections[_newSection].AudioLayers[i].ArcObj.AudioClips = _sections[_newSection].AudioLayers[i].AudioClips;
                //Now it has an ARC Object. You're welcome

                //TODO: All the ARC Object stuff here
            }
            else
            {
                Debug.LogWarning("You gotta add sounds chief");
            }
        }

        _sections[_newSection]._layerObject = new GameObject[_sections[_newSection].AudioLayerCount()];
        _sections[_newSection]._audioLayerACR = new AudioClipRandomizer[_sections[_newSection].AudioLayerCount()];

        for (int i = 0; i < _sections[_newSection].AudioLayers.Length; i++)
        {
            //Uses the ARC Object by default if one is available
            if (_sections[_newSection].AudioLayers[i].ArcObj != null)
            {
                string _name = "Section " + _newSection + ", Layer " + i;

                _sections[_newSection]._layerObject[i] = new GameObject(_name);
                _sections[_newSection]._layerObject[i].transform.SetParent(transform);
                _sections[_newSection]._audioLayerACR[i] = _sections[_newSection]._layerObject[i].AddComponent<AudioClipRandomizer>();
                _sections[_newSection]._audioLayerACR[i].SetAudioRandomizerContainer(_sections[_newSection].AudioLayers[i].ArcObj);
                _sections[_newSection]._audioLayerACR[i].SetOverrideArcSettings(false);
                _sections[_newSection]._audioLayerACR[i].PlaySFX();
                _sections[_newSection]._audioLayerACR[i].SetSFXVolume(_sections[_newSection].AudioLayers[i]._audioLayerVolumes);
            }
            else
            {
                Debug.LogWarning("You gotta add sounds chief part 2");
            }
        }
    }
    
    //This will transition out of the current state using the index provided into whatever section is set by that transition. 
    //If no transition state at that index exists, it will not transition. 
    public void TransitionSection(int _transitionIndex)
    {
        /*if (_sections[_currentSection].SectionTransitions.Length >= _transitionIndex)
        {
            Debug.LogWarning("WARNING: Transition Index does not exist. Continuing on this section.");
            return;
        }//*/

        int newSection = _sections[_currentSection].SectionTransitions[_transitionIndex].TransitionInto;

        if (!_isRunningCrossfade && !_isRunningCrossfadeCheck)
            StartCoroutine(Crossfade(newSection));
        else if (_isRunningCrossfade && !_isRunningCrossfadeCheck)
            StartCoroutine(WaitForCrossfade(newSection));
    }

    IEnumerator Crossfade(int _newSection)
    {
        _isRunningCrossfade = true;

        float timeToFade = 1.0f;
        float timeElapsed = 0.0f;

        //

        InitializeSection(_newSection);

        while (timeElapsed < timeToFade)
        {
            for (int i = 0; i < _sections[_currentSection]._audioLayerACR.Length; i++)
            {
                _sections[_newSection]._audioLayerACR[i].SetSFXVolume(Mathf.Lerp(0.0f, 1.0f, timeElapsed / timeToFade));
                _sections[_currentSection]._audioLayerACR[i].SetSFXVolume(Mathf.Lerp(1.0f, 0.0f, timeElapsed / timeToFade));
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < _sections[_newSection]._layerObject.Length; i++)
            Destroy(_sections[_currentSection]._layerObject[i], 1.0f);

        //

        _isRunningCrossfade = false;
        _currentSection = _newSection;
    }
    IEnumerator WaitForCrossfade(int _newSection)
    {
        _isRunningCrossfadeCheck = true;
        while (_isRunningCrossfade)
            yield return null;
        _isRunningCrossfadeCheck = false;
        StartCoroutine(Crossfade(_newSection));
    }
}
