using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class CircleHpBarScript : MonoBehaviour
{
    [SerializeField] private Image[] hpBarImage;
    private Tween hpTween;
    private float health;
    private const float tweenDuration = 0.3f;
    [SerializeField] private TextMeshProUGUI hpText;
    private void Start()
    {
        StartSetting();
    }

    public void StartSetting()
    {
        SetHP(0);
    }

    public void SetHP(float newHealth)
    {
        hpText.text = newHealth.ToString("F1") + "%";

        int segments = hpBarImage.Length;
        float maxHp = segments * 100f;
        this.health = Mathf.Clamp(newHealth, 0f, maxHp);

        // 풀 체력일 때 예외 처리
        if (Mathf.Approximately(this.health, maxHp))
        {
            hpTween?.Kill();
            for (int i = 0; i < segments; i++)
            {
                hpBarImage[i].DOKill();
                hpBarImage[i].fillAmount = 1f;
            }
            return;
        }

        int index = Mathf.Clamp((int)(this.health / 100f), 0, segments - 1);
        float fillValue = (this.health % 100f) / 100f;

        hpTween?.Kill();

        for (int i = 0; i < segments; i++)
        {
            hpBarImage[i].DOKill();

            if (i < index - 1)
            {
                hpBarImage[i].fillAmount = 0f;
            }
            else if (i == index - 1)
            {
                hpBarImage[i].fillAmount = 1f;
            }
            else if (i == index)
            {
                hpTween = hpBarImage[i].DOFillAmount(fillValue, tweenDuration)
                                        .SetEase(Ease.OutCubic);
            }
            else
            {
                hpBarImage[i].fillAmount = 0f;
            }
        }
    }
}
