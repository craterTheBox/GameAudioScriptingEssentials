using System.Collections;
using UnityEngine;
using GameAudioScriptingEssentials;

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
            //If no ARC Object, this will create one using the clips provided (granted, with default settings)
            if (_sections[_newSection].AudioLayers[i].ArcObj == null && _sections[_newSection].AudioLayers[i].AudioClips != null)
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

        for (int i = 0; i < _sections[_newSection].AudioLayers.Length; i++)
        {
            string _name = "Section " + _newSection + ", Layer " + i;

            _sections[_newSection]._layerObject[i] = new GameObject(_name);
            _sections[_newSection]._layerObject[i].transform.SetParent(transform);
            _sections[_newSection]._audioLayerACR[i] = _sections[_newSection]._layerObject[i].AddComponent<AudioClipRandomizer>();
            _sections[_newSection]._audioLayerACR[i].ArcObj = _sections[_newSection].AudioLayers[i].ArcObj;
            _sections[_newSection]._audioLayerACR[i].OverrideArcSettings = false;
            _sections[_newSection]._audioLayerACR[i].PlaySFX();
            _sections[_newSection]._audioLayerACR[i].SFXVolume = _sections[_newSection].AudioLayers[i]._audioLayerVolumes;
        }
    }
    
    //This will transition out of the current state using the index provided into whatever section is set by that transition. 
    //If no transition state at that index exists, it will not transition. 
    public void TransitionSection(int _transitionIndex)
    {
        if (_sections[_currentSection].SectionTransitions.Length <= _transitionIndex)
        {
            Debug.LogWarning("WARNING: Transition Index does not exist. Continuing on this section.");
            return;
        }

        int _newSection = _sections[_currentSection].SectionTransitions[_transitionIndex].TransitionInto;
        float _crossfadeTime = _sections[_currentSection].SectionTransitions[_transitionIndex].CrossfadeTime;

        if (!_isRunningCrossfade && !_isRunningCrossfadeCheck)
            StartCoroutine(Crossfade(_newSection, _crossfadeTime));
        else if (_isRunningCrossfade && !_isRunningCrossfadeCheck)
            StartCoroutine(WaitForCrossfade(_newSection, _crossfadeTime));
    }

    IEnumerator Crossfade(int _newSection, float _crossfadeTime)
    {
        _isRunningCrossfade = true;

        float _currentTime = 0.0f;

        InitializeSection(_newSection);

        while (_currentTime < _crossfadeTime)
        {
            for (int i = 0; i < _sections[_currentSection]._audioLayerACR.Length; i++)
            {
                _sections[_newSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(0.0f, 1.0f, _currentTime / _crossfadeTime);
                _sections[_currentSection]._audioLayerACR[i].SFXVolume = Mathf.Lerp(1.0f, 0.0f, _currentTime / _crossfadeTime);
            }
            _currentTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < _sections[_newSection]._layerObject.Length; i++)
            Destroy(_sections[_currentSection]._layerObject[i], 1.0f);

        _isRunningCrossfade = false;
        _currentSection = _newSection;
    }
    IEnumerator WaitForCrossfade(int _newSection, float _crossfadeTime)
    {
        _isRunningCrossfadeCheck = true;
        while (_isRunningCrossfade)
            yield return null;
        _isRunningCrossfadeCheck = false;
        StartCoroutine(Crossfade(_newSection, _crossfadeTime));
    }
}
