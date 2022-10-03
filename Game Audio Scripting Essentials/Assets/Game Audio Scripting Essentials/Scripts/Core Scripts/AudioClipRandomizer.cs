using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[AddComponentMenu("Game Audio Scripting Essentials/Audio Clip Randomizer", 20)]
public class AudioClipRandomizer : MonoBehaviour
{
    [Header("Audio")]

    [Tooltip("Audio Randomizer Container scriptable object with the audio clips")]
    [SerializeField] AudioRandomizerContainer _arcObj;
    [Tooltip("Audio clip assets, replacing the need for an Audio Randomizer Container object")]
    [SerializeField] AudioClip[] _audioClips;
    [Tooltip("Audio Mixer Group that these audio clips will be routed to")]
    [SerializeField] AudioMixerGroup _mixerGroup;

    [Header("Settings")]
    [Tooltip("Toggle to override Audio Randomizer Container settings with the settings below")]
    [SerializeField] bool _overrideArcSettings = true;
    [Tooltip("Toggle for whether or not it will repeat the same clip in a row. This is automatically disabled when there is only one clip")]
    [SerializeField] public bool _noRepeats = true;
    [Tooltip("Toggle for randomizing the pitch of the audio clips")]
    [SerializeField] bool _randomPitch = true;
    [Tooltip("Minimum pitch value")]
    [Range(-3.0f, 3.0f)]
    [SerializeField] float _minPitch = 0.50f;
    [Tooltip("Maximum pitch value")]
    [Range(-3.0f, 3.0f)]
    [SerializeField] float _maxPitch = 1.50f;
    [Tooltip("Volume of the audio clips")]
    [Range(0.0f, 1.0f)]
    [SerializeField] float _volume = 1.0f;
    [Tooltip("Toggle for the clips looping or playing once")]
    [SerializeField] bool _loop = false;
    [Tooltip("Sets the priority of the audio clips")]
    [Range(0, 256)]
    [SerializeField] int _priority = 128;
    [Tooltip("Panning value of the audio clips (-1 being hard left, 1 being hard right, 0 being mono)")]
    [Range(-1.0f, 1.0f)]
    [SerializeField] float _stereoPan = 0.0f;
    [Tooltip("How much the audio clips are attenuated in the world (0 being 2D with no attenuation, 1 being 3D with full attenuation)")]
    [Range(0.0f, 1.0f)]
    [SerializeField] float _spatialBlend = 0.0f;

    int _lastIndex = -1;
    bool _arcObjExists = false;
    bool _initVolumeOverwritten = false;
    bool _isRunningCheck = false;

    void Start()
    {
        DoesArcObjExist();
        //Ensures that if this is done on a single clip it will repeat
        if (!_arcObjExists && _audioClips.Length == 1)
        {
            _noRepeats = false;
        }
    }
    public void PlaySFX()
    {
        int _index = 0;
        float _pitch = 1.0f;
        AudioClip _clip;

        DoesArcObjExist();

        if (!_arcObjExists)
        {
            if (_noRepeats)
            {
                while (_lastIndex == _index)
                {
                    _index = Random.Range(0, _audioClips.Length);
                }
            }

            _clip = _audioClips[_index];

            if (_randomPitch)
            {
                _pitch = Random.Range(_minPitch, _maxPitch);
            }
        }
        else
        {
            if (_arcObj.NoRepeats)
            {
                while (_lastIndex == _index)
                {
                    _index = Random.Range(0, _arcObj.AudioClips.Length);
                }
            }

            _clip = _arcObj.AudioClips[_index];

            if (_arcObj.RandomPitch || (_overrideArcSettings && _randomPitch))
            {
                _pitch = (_overrideArcSettings) ? Random.Range(_minPitch, _maxPitch) : Random.Range(_arcObj.MinPitch, _arcObj.MaxPitch);
            }

            if (!_initVolumeOverwritten)
            {
                _volume = _arcObj.Volume;
            }

            _initVolumeOverwritten = false;

            _mixerGroup = (_overrideArcSettings) ? _mixerGroup : _arcObj.MixerGroup;
            _loop = (_overrideArcSettings) ? _loop : _arcObj.Loop;
            _priority = (_overrideArcSettings) ? _priority : _arcObj.Priority;
            _stereoPan = (_overrideArcSettings) ? _stereoPan : _arcObj.StereoPan;
            _spatialBlend = (_overrideArcSettings) ? _spatialBlend : _arcObj.SpatialBlend;
        }

        _lastIndex = _index;

        AudioSource _newAudioSource = gameObject.AddComponent<AudioSource>();
        _newAudioSource.clip = _clip;
        _newAudioSource.pitch = _pitch;
        _newAudioSource.volume = _volume;
        _newAudioSource.outputAudioMixerGroup = _mixerGroup;
        _newAudioSource.loop = _loop;
        _newAudioSource.priority = _priority;
        _newAudioSource.panStereo = _stereoPan;
        _newAudioSource.spatialBlend = _spatialBlend;
        _newAudioSource.Play();

        if (!_loop)
        {
            Destroy(_newAudioSource, _clip.length + 0.2f);
        }
        else if (_loop && _arcObjExists && !_isRunningCheck)
        {
            if (_arcObj.AudioClips.Length > 1)
            {
                StartCoroutine(WaitForClipToFinish());
            }
        }
        else if (_loop && !_arcObjExists && !_isRunningCheck)
        {
            if (_audioClips.Length > 1)
            {
                StartCoroutine(WaitForClipToFinish());
            }
        }
    }
    IEnumerator WaitForClipToFinish()
    {
        _isRunningCheck = true;

        while (SFXPlayPosition < GetSFXLength())
        {
            yield return null;
        }
        Destroy(GetComponent<AudioSource>(), GetSFXLength());
        PlaySFX();

        _isRunningCheck = false;
    }
    public void StopSFX()
    {
        AudioSource _current = GetComponent<AudioSource>();
        _current.Stop();
    }
    public void DestroySFX()
    {
        AudioSource _current = GetComponent<AudioSource>();
        Destroy(_current);
    }
    public float SFXVolume
    {
        get => GetComponent<AudioSource>().volume;
        set => GetComponent<AudioSource>().volume = value;
    }
    public bool IsSFXPlaying()
    {
        AudioSource _current = GetComponent<AudioSource>();
        return _current.isPlaying;
    }
    public float SFXPlayPosition => GetComponent<AudioSource>().time;
    public float GetSFXLength()
    {
        AudioSource _current = GetComponent<AudioSource>();
        return _current.clip.length;
    }
    public float[] GetSFXLengths()
    {
        float[] _lengths = new float[_audioClips.Length];

        DoesArcObjExist();

        if (!_arcObjExists)
        {
            for (int i = 0; i < _audioClips.Length; i++)
            {
                _lengths[i] = _audioClips[i].length;
            }
        }
        else
        {
            for (int i = 0; i < _arcObj.AudioClips.Length; i++)
            {
                _lengths[i] = _arcObj.GetLength(i);
            }
        }

        return _lengths;
    }
    public float GetSFXAverageLength()
    {
        float averageLength = -1.0f;

        if (!_arcObjExists)
        {
            for (int i = 0; i < _audioClips.Length; i++)
            {
                averageLength += _audioClips[i].length;
            }

            averageLength /= _audioClips.Length;
        }
        else
        {
            for (int i = 0; i < _arcObj.AudioClips.Length; i++)
            {
                averageLength += _arcObj.GetLength(i);
            }

            averageLength /= _arcObj.AudioClips.Length;
        }

        return averageLength;
    }
    public float GetSFXLongestLength()
    {
        float _longestLength = 0.0f;

        if (!_arcObjExists)
        {
            for (int i = 0; i < _audioClips.Length; i++)
            {
                _longestLength = (_longestLength < _audioClips[i].length) ? _audioClips[i].length : _longestLength;
            }
        }
        else
        {
            for (int i = 0; i < _arcObj.AudioClips.Length; i++)
            {
                _longestLength = (_longestLength < _arcObj.GetLength(i)) ? _arcObj.GetLength(i) : _longestLength;
            }
        }

        return _longestLength;
    }
    void DoesArcObjExist()
    {
        if (_arcObj != null)
        {
            _arcObjExists = true;
            if (_arcObj.AudioClips.Length == 1)
            {
                _arcObj.NoRepeats = false;
            }
        }
    }
    public AudioRandomizerContainer ArcObj
    {
        get => _arcObj;
        set => _arcObj = value;
    }
    public void SetAudioClips(AudioClip[] _clips)
    {
        _audioClips = _clips;
        _arcObjExists = false;
    }
    public string GetSFXName()
    {
        AudioSource _current = GetComponent<AudioSource>();
        return _current.clip.name;
    }
    public bool OverrideArcSettings
    {
        get => _overrideArcSettings;
        set => _overrideArcSettings = value;
    }
    public bool Loop => _loop;
}
