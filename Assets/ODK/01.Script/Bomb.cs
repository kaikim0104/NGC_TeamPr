using System.Collections;
using UnityEngine;

public class Bomb : Item
{
    public float damage = 2;
    public float knockbackmulti = 0.5f;

    public GameObject explosion;
    public override void Eat()
    {
        GameObject ex = Instantiate(explosion, transform.position, Quaternion.identity);
        Instantiate(effect[0], owner.transform.position, Quaternion.identity);
        ex.GetComponent<ExplosionScript>().preowner = preowner;
        base.Eat();
    }
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;



        if (((1 << layer) & targetLayer) != 0 &&
            collision.gameObject != owner && isshooting)
        {
            isshooting = false;
            owner = null;
            
            StartCoroutine(Attacking(collision.gameObject));
        }
    }
    public override IEnumerator Attacking(GameObject target)
    {
        base.Attacking(target);
        GameObject ex = Instantiate(explosion, transform.position, Quaternion.identity);
        ex.GetComponent<ExplosionScript>().preowner = preowner;
        if (target.GetComponent<Entity>() != null)
        {
            Instantiate(effect[0], transform.position, Quaternion.identity);
            target.GetComponent<Entity>().Attack(transform, damage, knockbackmulti);

        }
        Destroy(gameObject);
        yield return null;
        
    }
}
