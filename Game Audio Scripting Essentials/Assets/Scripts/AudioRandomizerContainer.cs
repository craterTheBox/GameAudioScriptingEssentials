using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Audio Randomizer Container", menuName = "Audio Randomizer Container", order = 51)]
public class AudioRandomizerContainer : ScriptableObject
{
    [Header("Audio")]
    [SerializeField] AudioClip[] _audioClips;

    [Header("Settings")]
    [SerializeField] public bool _noRepeats = true;
    [SerializeField] bool _randomPitch = true;
    [SerializeField] float _minPitch = 0.50f;
    [SerializeField] float _maxPitch = 1.50f;
    [SerializeField] float _volume = 1.0f;
    [SerializeField] bool _loop = false;

    public AudioClip[] GetAudioClips()
    {
        return _audioClips;
    }
    public bool GetNoRepeats()
    {
        return _noRepeats;
    }
    public void SetNoRepeats(bool _val)
    {
        _noRepeats = _val;
    }
    public bool GetRandomPitch()
    {
        return _randomPitch;
    }
    public float GetMinPitch()
    {
        return _minPitch;
    }
    public float GetMaxPitch()
    {
        return _maxPitch;
    }
    public bool GetLoop()
    {
        return _loop;
    }
    public float[] GetLengths()
    {
        float[] _lengths = new float[_audioClips.Length];

        for (int i = 0; i < _audioClips.Length; i++)
            _lengths[i] = _audioClips[i].length;

        return _lengths;
    }
    public float GetLength(int index)
    {
        return _audioClips[index].length;
    }
    public float GetVolume()
    {
        return _volume;
    }
    public void SetVolume(float _val)
    {
        _volume = _val;
    }
}
