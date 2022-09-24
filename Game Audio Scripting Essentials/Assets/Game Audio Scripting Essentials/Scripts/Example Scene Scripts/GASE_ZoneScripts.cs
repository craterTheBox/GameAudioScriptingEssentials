using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GASE_ZoneScripts : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] UnityEvent OnZoneEnter;
    [SerializeField] UnityEvent OnZoneLeave;

    private void OnTriggerEnter(Collider other)
    {
        OnZoneEnter?.Invoke();
    }
    private void OnTriggerExit(Collider other)
    {
        OnZoneLeave?.Invoke();
    }
}
