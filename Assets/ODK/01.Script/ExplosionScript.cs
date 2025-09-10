using System.Collections.Generic;
using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    public float damage = 20;
    public float knockbackmulti = 3;

    public float lifetime = 0.2f;
    public LayerMask layermask;
    public Transform preowner;
    private HashSet<Entity> alreadyHit = new HashSet<Entity>();
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & layermask) != 0)
        {
            Entity player = collision.gameObject.GetComponent<Entity>();
            if (player != null && !alreadyHit.Contains(player))
            {
                alreadyHit.Add(player); // 중복 방지

                if (collision.transform == preowner)
                {
                    player.Attack(preowner, damage / 1.5f, knockbackmulti);
                }
                else
                {
                    player.Attack(preowner, damage, knockbackmulti);
                }
            }
        }
    }

}
