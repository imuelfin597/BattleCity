using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region namespace BattleCity start
namespace BattleCity
{
public class Player : Tank
{
    public GameObject pfShield;
    private const float shieldTime = 5f;
    private float shieldTimer = -1f;
    private GameObject shieldInstance = null;

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();

        AudioManager.Instance.SetAudioListenerTarget(gameObject);
        GenerateShield();
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
        shieldTimer -= Time.deltaTime;

        HandleInput();
        HoldingShield();
    }

    private void HandleInput() {
        // 移动输入
        Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // 开火输入
        if (Input.GetAxisRaw("Fire1") == 1) {
            Fire(true);
        }
    }

    protected override void GetDamaged() {
        if (isShielded()) return;
        base.GetDamaged();
    }

    protected override void Die() {
        AudioManager.Instance.SetAudioListenerTarget(Camera.main.gameObject);

        base.Die();
        GameManager.Instance.PlayerDied();
    }

    private void GenerateShield() {
        if (shieldInstance == null) {
            shieldInstance = Instantiate(pfShield, transform);
        }
        shieldTimer = shieldTime;
    }

    private void HoldingShield() {
        if (isShielded()) return;
        if (shieldInstance) {
            Destroy(shieldInstance);
        }
    }

    public bool isShielded() {
        return shieldTimer > 0;
    }
}
}
#endregion
