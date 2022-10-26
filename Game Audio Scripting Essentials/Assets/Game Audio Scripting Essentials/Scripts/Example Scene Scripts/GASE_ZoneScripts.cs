using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Game Audio Scripting Essentials/Example Scripts/Zone Script", 9999)]
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