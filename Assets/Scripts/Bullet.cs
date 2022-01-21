using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region namespace BattleCity start
namespace BattleCity
{
public class Bullet : MonoBehaviour
{
    public float speed;
    public GameObject pfExplosion;
    private Rigidbody2D rb;
    private bool isPlayer = false;
    private static int num = 0;

    private void Awake() {
        num++;
    }
    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {
        rb.velocity = transform.up * speed;
    }

    private void OnDestroy() {
        num--;
    }

    public static int GetNum() {
        return num;
    }

    public void SetDirection(Vector2 direction) {
        float ang = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, ang);
    }

    public void SetIsPlayer(bool isPlayer) {
        this.isPlayer = isPlayer;
    }

    public bool GetIsPlayer() {
        return isPlayer;
    }

    public void Die() {
        Destroy(gameObject);
        Instantiate(pfExplosion, transform.position, transform.rotation)
            .GetComponent<Animator>().SetBool("isLarge", false);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        switch (other.gameObject.tag) {
        case "Player":
            // Debug.Log("==========>Player");
            if (!isPlayer) {
                Die();
                other.gameObject.SendMessage("GetDamaged");
            }
            break;
        case "Enemy":
            // Debug.Log("==========>Enemy");
            if (isPlayer) {
                Die();
                other.gameObject.SendMessage("GetDamaged");
            }
            break;
        case "Base":
            // Debug.Log("==========>Base");
            Die();
            other.gameObject.SendMessage("GetDamaged");
            break;
        case "Brick":
            // Debug.Log("==========>Brick");
            Die();
            other.gameObject.SendMessage("GetDamaged");
            break;
        case "Grass":
            // Debug.Log("==========>Grass");
            break;
        case "Road":
            // Debug.Log("==========>Road");
            break;
        case "Steel":
            // Debug.Log("==========>Steel");
            Die();
            break;
        case "Water":
            // Debug.Log("==========>Water");
            break;
        case "Bullet":
            // Debug.Log("==========>Bullet");
            if (other.gameObject.TryGetComponent<Bullet>(out Bullet bullet)) {
                if (isPlayer != bullet.GetIsPlayer()) {
                    Die();
                }
            }
            break;
        default:
            // Debug.Log("==========>default");
            Die();
            break;
        }
    }
}
}
#endregion
