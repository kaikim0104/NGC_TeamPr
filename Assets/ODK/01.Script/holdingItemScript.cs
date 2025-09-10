using System.Collections;
using UnityEngine;
public abstract class Item : MonoBehaviour
{
    public bool iscooldown = false;
    public bool isshooting = false;
    [SerializeField] protected GameObject[] effect;
    protected Rigidbody2D rigidbody;
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected LayerMask groundLayer;
    public GameObject owner;
    public Transform preowner;
    public Vector2 shootingdir;
    public bool thisisnoforceobject = false;
    public bool thisownerfading = true;
    public virtual void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;
        if (((1 << layer) & groundLayer) != 0 && isshooting && !iscooldown)
        {
            isshooting = false;
            owner = null;
            Instantiate(effect[0], transform.position, Quaternion.identity);
            StartCoroutine(Attacking(collision.gameObject));
        }
    }
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;

        

        if (((1 << layer) & targetLayer) != 0 &&
            collision.gameObject != owner && isshooting)
        {
            isshooting = false;
            owner = null;
            Instantiate(effect[0], transform.position, Quaternion.identity);
            StartCoroutine(Attacking(collision.gameObject));
        }
    }
    
    public virtual void Launching()
    {

    }
    public virtual void Grab()
    {

    }
    public virtual IEnumerator Attacking(GameObject target)
    {
        foreach (var item in effect)
        {
            item.SetActive(true);
        }
        yield return null;
    }

    public virtual void Eat()
    {
        owner.GetComponent<Entity>().Attack(transform, 10, 0f);
        isshooting = false;
        Instantiate(effect[0], owner.transform.position, Quaternion.identity);
        owner = null;
        Destroy(gameObject);
    }
    public void CooldownActive()
    {
        StartCoroutine(HoldCooldown());
    }

    private IEnumerator HoldCooldown()
    {
        iscooldown = true;
        yield return new WaitForSeconds(0.1f);
        if (thisownerfading) owner = null;

        iscooldown = false;
    }
}