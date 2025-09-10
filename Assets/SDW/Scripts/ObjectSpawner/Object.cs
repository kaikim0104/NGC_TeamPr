using UnityEngine;

public class Object : MonoBehaviour
{
    // ¿”Ω√¿”
    private bool isCollected = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Spawner.itemX = (sbyte)Mathf.RoundToInt(transform.position.x);
        }

        if (isCollected == false) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Spawner.itemCount--;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            //∂•ø°º≠ ∂≥æÓ¡ˆ∏È »πµÊ
            isCollected = true;
        }
    }

    private void OnDestroy()
    {
        Spawner.itemCount--;
    }
}
