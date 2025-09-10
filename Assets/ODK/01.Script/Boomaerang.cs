using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Unity.VisualScripting;
public class Boomaerang : Item
{
    [SerializeField] private Ease easeType = Ease.OutCubic;
    [SerializeField] private Ease lasteaseType = Ease.InCubic;
    public float damage = 5;
    public float knockbackmulti = 1;
    public float firstmovetime = 1f;
    public float lastmovetime = 1f;
    public float lifeTime = 2f;
    public float range = 5f;
    private Vector2 originVector;
    private bool isboomeranged = false;
    [SerializeField] private bool thisnotalreadyHit = false;
    private float currentlifeTime = 2f;
    public DG.Tweening.Sequence seq;
    private HashSet<Entity> alreadyHit = new HashSet<Entity>();
    public override void Awake()
    {

        base.Awake();

    }
    public void Update()
    {
        if (isboomeranged)
        {
            transform.Rotate(Vector3.forward * -1500 * Time.deltaTime);
            currentlifeTime -= Time.deltaTime;
            if (currentlifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }
        //if (isshooting && !isboomeranged)
        //{
        //    isboomeranged = true;
        //    originVector = transform.position;
        //    Destroy(gameObject, lifeTime);
        //    GetComponent<Rigidbody2D>().gravityScale = 0f;
        //    GetComponent<BoxCollider2D>().isTrigger = true;
        //    Sequence seq = DOTween.Sequence();


        //    seq.Append(transform.DOMove((Vector2)transform.position + (shootingdir * range), firstmovetime).SetEase(Ease.OutCubic));
        //    seq.AppendCallback(retuning);
        //}
    }

    public override void Launching()
    {
        if (isboomeranged)
        {
            return;
        }
        alreadyHit.Clear();
        currentlifeTime = lifeTime;
        isboomeranged = true;
        originVector = transform.position;
        GetComponent<Rigidbody2D>().gravityScale = 0f;
        GetComponent<BoxCollider2D>().isTrigger = true;
        seq = DOTween.Sequence();
        isshooting = false;

        seq.Append(transform.DOMove((Vector2)transform.position + (shootingdir * range), firstmovetime).SetEase(easeType));
        seq.AppendCallback(retuning);
    }
    public override void Grab()
    {
        base.Grab();
        isboomeranged = false;

        seq?.Kill(true);
        transform.DOKill();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        int layer = collision.gameObject.layer;
        if (((1 << layer) & targetLayer) != 0 && collision.gameObject.transform != preowner && isboomeranged)
        {
            Instantiate(effect[0], collision.transform.position, Quaternion.identity);
            StartCoroutine(Attacking(collision.gameObject));
        }
    }
    void retuning()
    {
        alreadyHit.Clear();
        transform.DOMove(originVector - (shootingdir * range), lastmovetime).SetEase(lasteaseType);
    }

    public override IEnumerator Attacking(GameObject target)
    {
        base.Attacking(target);

        if (target.GetComponent<Entity>() != null)
        {
            if (alreadyHit.Contains(target.GetComponent<Entity>()))
            {
                yield break;
            }
            target.GetComponent<Entity>().Attack(preowner, damage, knockbackmulti);

            if (thisnotalreadyHit)
                alreadyHit.Add(target.GetComponent<Entity>());
        }

        yield return null;

    }
}