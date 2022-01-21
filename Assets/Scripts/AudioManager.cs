using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

#region namespace BattleCity start
namespace BattleCity
{
public class AudioManager : GenericSingleton<AudioManager>
{
    public AudioListener audioListener;
    private AudioSource audioSrc;

    protected override void Awake() {
        base.Awake();
        audioSrc = GetComponent<AudioSource>();
    }

    public void PlayClip(AudioClip clip, float volumeScale = 1f) {
        if (clip == null) return;
        audioSrc.PlayOneShot(clip, volumeScale);
    }

    public void SetAudioListenerTarget(GameObject target) {
        PositionConstraint constraint = audioListener.gameObject.GetComponent<PositionConstraint>();
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = target.transform;
        source.weight = 1;
        constraint.SetSource(0, source);
    }
}
}
#endregion
