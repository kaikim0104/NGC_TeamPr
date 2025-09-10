using UnityEngine;

public class Return : MonoBehaviour
{
    // ¿”Ω√¿”
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer != LayerMask.NameToLayer("Player")) Destroy(collision.gameObject);
        else
            collision.transform.position = new Vector2(-3, 2);
    }
}
