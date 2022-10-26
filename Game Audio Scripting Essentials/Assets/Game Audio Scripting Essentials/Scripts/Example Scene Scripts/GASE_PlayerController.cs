using System.Collections;
using UnityEngine;
using GameAudioScriptingEssentials;

[AddComponentMenu("Game Audio Scripting Essentials/Example Scripts/Player Controller", 9998)]
public class GASE_PlayerController : MonoBehaviour
{
    [Header("Section Transition")]
    [Tooltip("The speed of the player cube")]
    [SerializeField] float playerSpeed = 4.0f;
    [Tooltip("This is the Audio Clip Randomizer object for the footsteps container")]
    [SerializeField] AudioClipRandomizer footsteps;
    [Header("Inputs")]
    [Tooltip("Input to move up")]
    [SerializeField] KeyCode up = KeyCode.W;
    [Tooltip("Input to move down")]
    [SerializeField] KeyCode down = KeyCode.S;
    [Tooltip("Input to move left")]
    [SerializeField] KeyCode left = KeyCode.A;
    [Tooltip("Input to move right")]
    [SerializeField] KeyCode right = KeyCode.D;

    bool isMoving = false;
    bool isCoroutineRunning = false;

    void Update()
    {
        bool isMovingVert = false;
        bool isMovingHori = false;

        if (Input.GetKey(up))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + (playerSpeed * Time.deltaTime));
            isMovingVert = true;
        }
        else if (Input.GetKey(down))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - (playerSpeed * Time.deltaTime));
            isMovingVert = true;
        }
        else
            isMoving = false;
        if (Input.GetKey(left))
        {
            transform.position = new Vector3(transform.position.x - (playerSpeed * Time.deltaTime), transform.position.y, transform.position.z);
            isMovingHori = true;
        }
        else if (Input.GetKey(right))
        {
            transform.position = new Vector3(transform.position.x + (playerSpeed * Time.deltaTime), transform.position.y, transform.position.z);
            isMovingHori = true;
        }
        else
            isMovingHori = false;

        isMoving = isMovingHori || isMovingVert;

        if (!isCoroutineRunning && isMoving)
            StartCoroutine(Footsteps());
    }

    IEnumerator Footsteps()
    {
        isCoroutineRunning = true;
        if (isMoving)
        {
            footsteps.PlaySFX();

            yield return new WaitForSeconds(playerSpeed / 12.0f);
        }
        isCoroutineRunning = false;
    }
}