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
    [SerializeField] private float invisibleTime = 0.5f;
    private float realinvisibleTime;
    public override void Awake()
    {

        base.Awake();
        

    }
    public void Update()
    {
        if (invisibleTime > 0f)
        {
            realinvisibleTime += Time.deltaTime;
            if (realinvisibleTime >= invisibleTime)
            {
                realinvisibleTime = 0f;
                alreadyHit.Clear();
            }
        }

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
        realinvisibleTime = 0f;
        currentlifeTime = lifeTime;
        isboomeranged = true;
        originVector = transform.position;
        GetComponent<Rigidbody2D>().gravityScale = 0f;
        GetComponent<BoxCollider2D>().isTrigger = true;
        seq = DOTween.Sequence();
        isshooting = false;
        alreadyHit.Clear();
        seq.Append(transform.DOMove((Vector2)transform.position + (shootingdir * range), firstmovetime).SetEase(easeType));
        seq.AppendCallback(retuning);
    }
    public override void Grab()
    {
        base.Grab();
        isboomeranged = false;
        realinvisibleTime = 0f;
        seq?.Kill(true);
        transform.DOKill();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isboomeranged) return;

        int layer = collision.gameObject.layer;
        if (((1 << layer) & targetLayer) != 0 && collision.transform != preowner)
        {
            Entity entity = collision.GetComponent<Entity>();
            if (entity != null)
            {
                if (alreadyHit.Contains(entity))
                    return;

                // 이펙트 생성
                Instantiate(effect[0], collision.transform.position, Quaternion.identity);

                // 히트 처리
                alreadyHit.Add(entity);
                StartCoroutine(Attacking(collision.gameObject));
            }
        }
    }

    public override IEnumerator Attacking(GameObject target)
    {
        base.Attacking(target);
        Entity entity = target.GetComponent<Entity>();

        Debug.Log($"Try Attack {gameObject.name} -> {entity.name}");

        if (entity != null)
        {
            Debug.Log($"{gameObject.name} is attakcing {entity.name}");
            entity.Attack(preowner, damage, knockbackmulti);
        }

        yield return null;
    }
    void retuning()
    {
        alreadyHit.Clear();
        transform.DOMove(originVector - (shootingdir * range), lastmovetime).SetEase(lasteaseType);
    }

}