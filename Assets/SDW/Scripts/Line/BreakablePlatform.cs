//using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace SDW
{
    public class BreakablePlatform : Platform
    {
        [SerializeField] private LayerMask breakableLayer; // 블록이 파괴될 수 있는 레이어
        [SerializeField] private bool breakable = true; // 블록 파괴 가능 여부
        [SerializeField] private Rigidbody2D childBlock;  // 바로 아래 블록

        private int blockLayer; // 블록 레이어
        private Joint2D joint;

        // network data
        public bool IsBreaking = false;
        private void Awake()
        {
            blockLayer = Mathf.RoundToInt(Mathf.Log(breakableLayer.value, 2));
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer != blockLayer) return;

            //부서졌는가?
            IsBreaking = true;
            SendData();

            Hit();
        }
    }
}