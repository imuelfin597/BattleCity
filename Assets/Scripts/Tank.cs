using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region namespace BattleCity start
namespace BattleCity
{
public class Tank : MonoBehaviour
{
    public Tank pfTank;
    public float speed = 2f;
    public float fireCd = 0.5f;
    public Bullet pfBullet;
    public GameObject pfExplosion;
    // 0 idle  1 moving  2 fire  3 die
    public AudioClip[] audioClips;
    private Rigidbody2D rb = null;
    private Animator animtr = null;
    private AudioSource audioSrc = null;
    private Vector2 localDir = Vector2.up;
    private Vector2 worldDir = Vector2.up;
    private bool isMoving = false;
    private float fireCdTimer = -1f;

    // Start is called before the first frame update
    protected virtual void Start() {
        rb = GetComponent<Rigidbody2D>();
        animtr = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    protected virtual void Update() {
        fireCdTimer -= Time.deltaTime;

        updateMove();
    }

    private void updateMove() {
        animtr.SetBool("isMoving", isMoving);
        animtr.SetFloat("inputX", localDir.x);
        animtr.SetFloat("inputY", localDir.y);

        rb.velocity = isMoving ? (Vector2)worldDir * speed : Vector2.zero;
    }

    public void Move(float x, float y) {
        y = x == 0 ? y : 0;
        isMoving = x != 0 || y != 0;
        if (isMoving) {
            localDir.Set(x, y);
            audioSrc.clip = audioClips[1];
        } else {
            audioSrc.clip = audioClips[0];
        }
        if (!audioSrc.isPlaying) {
            audioSrc.Play();
        }

        // 创建俯视角的变换矩阵矩阵
        float angle = Vector3.SignedAngle(Vector3.right, Vector3.ProjectOnPlane(transform.right, Vector3.forward), Vector3.forward);
        Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, angle), Vector3.one);
        // 将localDir变换到世界坐标系，worldDir = Mat * localDir;
        worldDir = mat.MultiplyVector((Vector3)localDir);
    }

    public void Stop() {
        Move(0f, 0f);
    }

    public bool Fire(bool isPlayer = true) {
        if (fireCdTimer > 0) return false;
        Bullet bullet = Instantiate(pfBullet, transform.position, transform.rotation);
        bullet.SetDirection(worldDir);
        bullet.SetIsPlayer(isPlayer);

        AudioManager.Instance.PlayClip(audioClips[2]);

        fireCdTimer = fireCd;
        return true;
    }

    public Vector2 GetLocalDirection() {
        return localDir;
    }

    public Vector2 GetWorldDirection() {
        return worldDir;
    }

    protected virtual void GetDamaged() {
        Die();
    }

    protected virtual void Die() {
        AudioManager.Instance.PlayClip(audioClips[3]);
        Destroy(gameObject);
        Instantiate(pfExplosion, transform.position, transform.rotation)
            .GetComponent<Animator>().SetBool("isLarge", true);
    }
}
}
#endregion
