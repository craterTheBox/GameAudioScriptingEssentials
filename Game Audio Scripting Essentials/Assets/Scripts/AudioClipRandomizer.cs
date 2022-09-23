using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioClipRandomizer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip[] _audioClips;
    [SerializeField] AudioRandomizerContainer arcObj = null;
    [SerializeField] AudioMixerGroup _mixerGroup = null;

    [Header("Settings")]
    [SerializeField] bool _noRepeats = true;
    [SerializeField] bool _randomPitch = true;
    [SerializeField] float _minPitch = 0.50f;
    [SerializeField] float _maxPitch = 1.50f;
    [SerializeField] bool _loop = false;

    int _lastIndex = -1;
    bool arcObjExists = false;

    void Start()
    {
        DoesArcObjExist();
        //Ensures that if this is done on a single clip it will repeat
        if (!arcObjExists && _audioClips.Length == 1)
            _noRepeats = false;
    }

    public void PlaySFX()
    {
        int _index = 0;
        float _pitch = 1.0f;
        float _volume = 1.0f;
        AudioClip _clip;

        DoesArcObjExist();

        if (!arcObjExists)
        {
            if (_noRepeats)
                while (_lastIndex == _index)
                    _index = Random.Range(0, _audioClips.Length);
            if (_randomPitch)
                _pitch = Random.Range(_minPitch, _maxPitch);

            _clip = _audioClips[_index];
        }
        else
        {
            if (arcObj.GetNoRepeats())
                while (_lastIndex == _index)
                    _index = Random.Range(0, arcObj.GetAudioClips().Length);

            if (arcObj.GetRandomPitch())
                _pitch = Random.Range(arcObj.GetMinPitch(), arcObj.GetMaxPitch());

            _clip = arcObj.GetAudioClips()[_index];

            _volume = arcObj.GetVolume();

            _loop = arcObj.GetLoop();
        }

        _lastIndex = _index;

        AudioSource _newAudioSource = gameObject.AddComponent<AudioSource>();
        //_audioSource = _newAudioSource;
        _newAudioSource.clip = _clip;
        _newAudioSource.pitch = _pitch;
        _newAudioSource.volume = _volume;
        _newAudioSource.loop = _loop;
        _newAudioSource.Play();

        if (!_loop) 
            Destroy(_newAudioSource, _clip.length + 0.2f);
    }

    public void SetSFXVolume(float _volume)
    {
        AudioSource _current = GetComponent<AudioSource>();

        DoesArcObjExist();

        _current.volume = _volume;
    }

    public float[] GetSFXLength()
    {
        float[] _lengths = new float[_audioClips.Length];

        DoesArcObjExist();

        if (!arcObjExists)
        {
            for (int i = 0; i < _audioClips.Length; i++)
            {
                _lengths[i] = _audioClips[i].length;
            }
        }
        else
        {
            for (int i = 0; i < arcObj.GetAudioClips().Length; i++)
            {
                _lengths[i] = arcObj.GetLength(i);
            }
        }

        return _lengths;
    }
    public float GetSFXAverageLength()
    {
        float averageLength = -1.0f;

        for (int i = 0; i < _audioClips.Length; i++)
        {
            averageLength += _audioClips[i].length;
        }
        averageLength /= _audioClips.Length;

        return averageLength;
    }
    void DoesArcObjExist()
    {
        if (arcObj != null)
        {
            arcObjExists = true;
            if (arcObj.GetAudioClips().Length == 1)
                arcObj.SetNoRepeats(false);
        }
    }
}
