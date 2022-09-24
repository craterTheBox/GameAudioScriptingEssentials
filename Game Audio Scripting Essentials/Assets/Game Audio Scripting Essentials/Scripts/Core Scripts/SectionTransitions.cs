using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game Audio Scripting Essentials/Section Transitions")]
public class SectionTransitions : MonoBehaviour
{
    [Header("Section Transition")]
    [Tooltip("This is the section that is transitioned into")]
    [SerializeField] GameObject _transitionInto;
    enum TransitionFade { NoFade, Crossfade }
    [Tooltip("Sets whether the transition crossfades the tracks or not")]
    [SerializeField] TransitionFade _fadeType;
    enum TransitionTrigger { OnEnd, OnTrigger }
    [Tooltip("Sets whether the transition happens when the audio clip ends or on a trigger")]
    [SerializeField] TransitionTrigger _triggerType;
    enum TransitionQuantization { Immediate, OnNextBeat, OnNextSecondBeat, OnNextBar }
    [Tooltip("Sets whether the transition occurs immediately or on time with the tempo")]
    [SerializeField] TransitionQuantization _quantization;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
