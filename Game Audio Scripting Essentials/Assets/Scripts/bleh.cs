using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class bleh : MonoBehaviour
{
    [SerializeField] UnityEvent Play;

    // Start is called before the first frame update
    void Start()
    {
        Play?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
