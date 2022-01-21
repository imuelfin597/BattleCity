using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region namespace BattleCity start
namespace BattleCity
{
public class FacingCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);
            child.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
}
#endregion
