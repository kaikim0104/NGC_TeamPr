//using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace SDW
{
    public class BreakablePlatform : Platform
    {
        [SerializeField] private bool breakable = true; // 블록 파괴 가능 여부
        [SerializeField] private Rigidbody2D childBlock;  // 바로 아래 블록

        private Joint2D joint;

        // network data
        public bool IsBroken = false;
        private void Awake()
        {
            joint = GetComponent<Joint2D>();
        }

        private void Hit()
        {
            if (!breakable) return;
            // 아래 블록의 모든 Joint 삭제
            if (childBlock != null)
            {
                // 하위 블록의 Block 스크립트 가져오기
                Joint2D[] joints = childBlock.GetComponents<Joint2D>();
                foreach (var j in joints)
                    Destroy(j);
            }

            // 자기 자신의 Joint 삭제
            if (joint != null)
                Destroy(joint);

            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log($"충돌: {collision.gameObject.name}, 레이어: {collision.gameObject.layer}");

            if (collision.gameObject.layer != LayerMask.NameToLayer("Item")) return;

            //부서졌는가?
            IsBroken = true;
            //SendData();

            Hit();
        }
    }
}