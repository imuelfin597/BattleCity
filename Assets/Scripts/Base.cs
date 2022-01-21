using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region namespace BattleCity start
namespace BattleCity
{
public class Base : MonoBehaviour
{
    public Sprite[] sprites;
    public GameObject pfExplosion;

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }

    private void GetDamaged() {
        Die();
    }

    private void Die() {
        Instantiate(pfExplosion, transform.position, transform.rotation)
            .GetComponent<Animator>().SetBool("isLarge", true);
        GetComponent<SpriteRenderer>().sprite = sprites[1];
        GameManager.Instance.GameOver();
    }
}
}
#endregion
