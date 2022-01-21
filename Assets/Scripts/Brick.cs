using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region namespace BattleCity start
namespace BattleCity
{
public class Brick : MonoBehaviour
{
    public Sprite[] sprites;
    private int hp = 3;

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }

    private void GetDamaged() {
        --hp;
        if (hp == 1)
            GetComponent<SpriteRenderer>().sprite = sprites[1];
        else if (hp == 0) {
            Die();
        }
    }

    private void Die() {
        Destroy(gameObject);
        MapGenerator.Instance.SetMapData(
            MapGenerator.Instance.PositionToCursor(transform.localPosition),
            MapGenerator.MapElement.NULL
        );
    }
}
}
#endregion
