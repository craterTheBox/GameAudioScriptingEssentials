using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveMusicContainer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] int _trackBPM = 120;
    [SerializeField] int _timeSignatureTop = 4;
    [SerializeField] int _timeSignatureBottom = 4;
    enum State { None, State1, State2, State3, State4, State5 }
    [SerializeField] State _initState;
    State _currentState = 0;

    [Header("Layers")]
    [SerializeField] AudioClipRandomizer[] _audioLayers;
    [Range(0.0f, 1.0f)]
    [SerializeField] float[] _audioLayerVolumes;

    [Header("States")]
    [Range(0.0f, 1.0f)]
    [SerializeField] float[] _state1AudioLayerVolumes;
    [Range(0.0f, 1.0f)]
    [SerializeField] float[] _state2AudioLayerVolumes;
    [Range(0.0f, 1.0f)]
    [SerializeField] float[] _state3AudioLayerVolumes;
    [Range(0.0f, 1.0f)]
    [SerializeField] float[] _state4AudioLayerVolumes;
    [Range(0.0f, 1.0f)]
    [SerializeField] float[] _state5AudioLayerVolumes;
    float[][] _statesAudioLayerVolumes;

    [Header("Debug")]
    [SerializeField] bool _ignoreWarnings = false;


    void Start()
    {
        //Checks if the layers are the same length
        float _previousAverageLength = -1.0f;

        for (int i = 0; i < _audioLayers.Length; i++)
        {
            if (_previousAverageLength != -1.0f && _previousAverageLength != _audioLayers[i].GetSFXAverageLength())
                Debug.LogWarning("WARNING: Audio Layers in obj \"" + this.name + "\" are at differing lengths. This is not a critical error.");
            _previousAverageLength = _audioLayers[i].GetSFXAverageLength();
        }

        //Plays each of the initial layers at the same time (or at least milliseconds off)
        for (int i = 0; i < _audioLayers.Length; i++)
        {
            _audioLayers[i].PlaySFX();
        }

        //Sets the initial state of the layers
        _currentState = _initState;
        _statesAudioLayerVolumes = new float[5][];
        _statesAudioLayerVolumes[0] = _state1AudioLayerVolumes;
        _statesAudioLayerVolumes[1] = _state2AudioLayerVolumes;
        _statesAudioLayerVolumes[2] = _state3AudioLayerVolumes;
        _statesAudioLayerVolumes[3] = _state4AudioLayerVolumes;
        _statesAudioLayerVolumes[4] = _state5AudioLayerVolumes;

        for (int i = 0; i < _audioLayers.Length; i++)
        {
            _audioLayers[i].SetSFXVolume(_statesAudioLayerVolumes[(int)_currentState][i]);
        }
        
    }

    void Update()
    {
        //State Change - VERTICAL
        if (true)
        {

        }

        //Section Transition - HORIZONTAL

    }

    void SetState(State newState)
    {
        _currentState = newState;

        for (int i = 0; i < _audioLayers.Length; i++)
        {
            _audioLayers[i].SetSFXVolume(_statesAudioLayerVolumes[(int)_currentState][i]);
        }
    }

    void Transition()
    {

    }
}
