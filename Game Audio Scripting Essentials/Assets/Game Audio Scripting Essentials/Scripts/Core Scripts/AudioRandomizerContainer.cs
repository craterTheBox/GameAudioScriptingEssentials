using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "Audio Randomizer Container", menuName = "Game Audio Scripting Essentials/Audio Randomizer Container", order = 51)]

public class AudioRandomizerContainer : ScriptableObject
{
    [Header("Audio")]
    [Tooltip("Audio clip assets played by the container")]
    [SerializeField] AudioClip[] _audioClips;
    [Tooltip("Audio Mixer Group that these audio clips will be routed to")]
    [SerializeField] AudioMixerGroup _mixerGroup;

    [Header("Settings")]
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

    public AudioClip[] AudioClips
    {
        get => _audioClips;
        set => _audioClips = value;
    }
    public float[] GetLengths()
    {
        float[] _lengths = new float[_audioClips.Length];

        for (int i = 0; i < _audioClips.Length; i++)
        {
            _lengths[i] = _audioClips[i].length;
        }

        return _lengths;
    }
    public float GetLength(int index)
    {
        return _audioClips[index].length;
    }
    public bool NoRepeats
    {
        get => _noRepeats;
        set => _noRepeats = value;
    }
    public bool RandomPitch
    {
        get => _randomPitch;
    }
    public float MinPitch
    {
        get => _minPitch;
    }
    public float MaxPitch
    {
        get => _maxPitch;
    }
    public bool Loop
    {
        get => _loop;
    }
    public float Volume
    {
        get => _volume;
        set => _volume = value;
    }
    public AudioMixerGroup MixerGroup
    {
        get => _mixerGroup;
        set => _mixerGroup = value;
    }
    public int Priority
    {
        get => _priority;
        set => _priority = value;
    }
    public float StereoPan
    {
        get => _stereoPan;
        set => _stereoPan = value;
    }
    public float SpatialBlend
    {
        get => _spatialBlend; 
        set => _spatialBlend = value;
    }
}
