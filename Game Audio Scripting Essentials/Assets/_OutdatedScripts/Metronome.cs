using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAudioScriptingEssentials;

//[AddComponentMenu("Game Audio Scripting Essentials/Metronome", 40)]
public class Metronome : MonoBehaviour
{
    [Header("Fields")]
    [Tooltip("Sound played on beat 1")]
    [SerializeField] AudioRandomizerContainer _beatOneARC;
    [HideInInspector] public AudioClipRandomizer _beatOne;
    [Tooltip("Sound played on every beat except beat 1")]
    [SerializeField] AudioRandomizerContainer _beatARC;
    [HideInInspector] public AudioClipRandomizer _beat;
    [Tooltip("Tempo of the clip in beats per minute")]
    [SerializeField] int _bpm = 120;
    [Tooltip("Top number of the time signature - the number of beats in one bar")]
    [SerializeField] int _topTimeSignature = 4;
    [Tooltip("Bottom number of the time signature - the note value of the beats")]
    [SerializeField] int _bottomTimeSignature = 4;

    float _timeToNextBeat;
    float _timeToNextBar;
    int _beatCount = 0;

    void Start()
    {
        //_beatOne.SetAudioRandomizerContainer(_beatOneARC);
        //_beat.SetAudioRandomizerContainer(_beatARC);

        if (_bottomTimeSignature == (1 ^ 2 ^ 4 ^ 8 ^ 16 ^ 32))
        {
            _timeToNextBeat = (60.0f / _bpm) / (_bottomTimeSignature / 4);
            _timeToNextBar = _timeToNextBeat * _topTimeSignature;
            
            //StartCoroutine(MetronomeTick());
        }
        //else
            //Debug.LogError("WARNING: Metronome's note value is invalid");
    }

    void Update()
    {
        if (_bottomTimeSignature % 2 == 0 ^ _bottomTimeSignature == 1)
        {
            _timeToNextBeat = (60.0f / _bpm) / (_bottomTimeSignature / 4);
            _timeToNextBar = _timeToNextBeat * _topTimeSignature;
        }
        //else
            //Debug.LogError("WARNING: Metronome's note value is invalid: " + _bottomTimeSignature);
    }

    IEnumerator MetronomeTick()
    {
        while (true)
        {
            _beatCount = 1;
            //_beatOne.PlaySFX();

            yield return new WaitForSeconds(_timeToNextBeat);

            for (int i = 0; i < _topTimeSignature - 1; i++)
            {
                _beatCount++;
                //_beat.PlaySFX();
                yield return new WaitForSeconds(_timeToNextBeat);
            }
        }
    }

    public int BeatsPerMinute
    {
        get => _bpm;
        set => _bpm = value;
    }
    public int TopTimeSignature
    {
        get => _topTimeSignature;
        set => _topTimeSignature = value;
    }
    public int BottomTimeSignature
    {
        get => _bottomTimeSignature;
        set => _bottomTimeSignature = value;
    }
    public float TimeToNextBeat
    {
        get => _timeToNextBeat;
    }
    public float TimeToNextBar
    {
        get => _timeToNextBar;
    }
    public int BeatCount
    {
        get => _beatCount;
    }
}
