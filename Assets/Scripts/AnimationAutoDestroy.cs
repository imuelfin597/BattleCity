using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region namespace BattleCity start
namespace BattleCity
{
public class AnimationAutoDestroy : StateMachineBehaviour
{
    public float time = -1;
    private Coroutine coroutine;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (coroutine != null) {
            GameManager.Instance.StopCoroutine(coroutine);
        }
        time = time > 0 ? time : stateInfo.length;
        GameManager.Instance.StartCoroutine(WaitAndDestroy(animator.gameObject, time));
    }

    private IEnumerator WaitAndDestroy(GameObject obj, float waitSeconds) {
        yield return new WaitForSeconds(waitSeconds);
        Destroy(obj);
    }
}
}
#endregion
