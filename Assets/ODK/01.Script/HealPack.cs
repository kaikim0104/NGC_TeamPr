using System.Collections;
using UnityEngine;

public class HealPack : Item
{
    public float damage = 5;
    public float knockbackmulti = 1;

    public override void Eat()
    {
        owner.GetComponent<Entity>().Attack(transform, damage * 1.5f, 0f);
        isshooting = false;
        Instantiate(effect[0], owner.transform.position, Quaternion.identity);
        owner = null;
        Destroy(gameObject);
    }
    public override IEnumerator Attacking(GameObject target)
    {
        base.Attacking(target);

        if (target.GetComponent<Entity>() != null)
        {
            target.GetComponent<Entity>().Attack(preowner, damage, knockbackmulti);
        }
        Destroy(gameObject);
        yield return null;
        
    }
}
