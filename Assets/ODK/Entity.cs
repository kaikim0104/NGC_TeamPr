using UnityEngine;

public class Entity : MonoBehaviour
{
    public float MaxHP { get; private set; } = 1000f;
    [field: SerializeField] public float CurrentHp { get; private set; } = 0f;
    [SerializeField] private CircleHpBarScript hpbar;

    public void Attack(Transform tra, float damage, float knockback)
    {
        // HP 감소
        CurrentHp += damage;
        CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHP);
        hpbar.SetHP(CurrentHp);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            Vector2 knockbackDir = (transform.position - tra.position).normalized;

            // y좌표 차이에 따라 강제 보정
            float yDiff = transform.position.y - tra.position.y;

            if (yDiff >= 0f)
                knockbackDir.y = 1f;
            else if (yDiff <= -0.5f)
                knockbackDir.y = -1f; 

            knockbackDir.Normalize();

            float baseForceX = knockback * 7f;
            float baseForceY = knockback * 4f;

            float hpRatio = CurrentHp / MaxHP; // 0~1
            float hpScale = 1f + hpRatio * 2.5f;

            Vector2 force = new Vector2(
                knockbackDir.x * baseForceX,
                knockbackDir.y * baseForceY
            ) * hpScale;

            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}
