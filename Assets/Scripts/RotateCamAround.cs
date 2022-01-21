using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region namespace BattleCity start
namespace BattleCity
{
public class RotateCamAround : MonoBehaviour
{
    private bool isRotating = false;
    private Vector3 targetPosition = Vector3.zero;
    private Vector3 offset = new Vector3(0, -6, -10);

    // Start is called before the first frame update
    void Start() {}

    // Update is callede per frame
    void Update() {
        Player playerInstance = GameManager.Instance.GetPlayerInstance();
        if (playerInstance) {
            targetPosition = playerInstance.transform.position;
            transform.position = targetPosition + offset;
        }

        if (!isRotating && Input.GetKeyDown(KeyCode.Q))
            StartCoroutine(RotateAround(-45f, 0.2f));
        else if (!isRotating && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(RotateAround(45f, 0.2f));
    }

    IEnumerator RotateAround(float angle, float time) {
        float count = 60 * time;
        float dPhi = angle / count;
        isRotating = true;
        for (int i = 0; i < count; i++) {
            transform.RotateAround(targetPosition, Vector3.forward, dPhi);
            offset = transform.position - targetPosition;
            yield return new WaitForFixedUpdate();
        }
        isRotating = false;
    }
}
}
#endregion
