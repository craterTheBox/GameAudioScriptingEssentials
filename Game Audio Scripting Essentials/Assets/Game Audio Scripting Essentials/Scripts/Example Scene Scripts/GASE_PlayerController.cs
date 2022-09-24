using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GASE_PlayerController : MonoBehaviour
{
    //Speed of the player cube
    [SerializeField] float playerSpeed = 2.0f;
    //Audio Clip Randomizer object for the footsteps container
    [SerializeField] AudioClipRandomizer footsteps;
    [Header("Inputs")]
    [SerializeField] KeyCode up = KeyCode.W;
    [SerializeField] KeyCode down = KeyCode.S;
    [SerializeField] KeyCode left = KeyCode.A;
    [SerializeField] KeyCode right = KeyCode.D;

    bool isMoving = false;
    bool isCoroutineRunning = false;

    void Update()
    {
        if (Input.GetKey(up))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + (playerSpeed * Time.deltaTime));
            isMoving = true;
        }
        else if (Input.GetKey(down))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - (playerSpeed * Time.deltaTime));
            isMoving = true;
        }
        else
            isMoving = false;
        if (Input.GetKey(left))
        {
            transform.position = new Vector3(transform.position.x - (playerSpeed * Time.deltaTime), transform.position.y, transform.position.z);
            isMoving = true;
        }
        else if (Input.GetKey(right))
        {
            transform.position = new Vector3(transform.position.x + (playerSpeed * Time.deltaTime), transform.position.y, transform.position.z);
            isMoving = true;
        }
        else
            isMoving = false;

        if (!isCoroutineRunning)
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
