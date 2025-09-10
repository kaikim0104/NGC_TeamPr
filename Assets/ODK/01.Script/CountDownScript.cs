using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.Events;

public class CountDownScript : MonoBehaviour
{
    [SerializeField] private GameObject countDownPanel;
    [SerializeField] private GameObject fadingSlide;
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private float countDownTime = 5f;
    [SerializeField] private UnityEvent onCountDownFinished;
    private float currentTime;
    private bool isCounting = false;

    private void Awake()
    {
        countDownPanel.SetActive(true);
        fadingSlide.SetActive(true);
    }

    private IEnumerator FadingSlideOpen()
    {
        isCounting = true;
        yield return new WaitForSeconds(1f);
        fadingSlide.transform.DOMoveY(3000f, 2f).SetEase(Ease.OutExpo);
    }

    private void Start()
    {
        StartCountDown();
    }

    public void StartCountDown()
    {
        currentTime = countDownTime;
        
        UpdateCountDownText();
        StartCoroutine(FadingSlideOpen());
    }

    private void Update()
    {
        if (!isCounting) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isCounting = false;

            
            StartCoroutine(CountDownSlideClose());
        }

        UpdateCountDownText();
    }
    private IEnumerator CountDownSlideClose()
    {
        onCountDownFinished?.Invoke();
        yield return new WaitForSeconds(1f);
        countDownPanel.transform.DOMoveY(3000f, 2f).SetEase(Ease.OutExpo);
    }
    private void UpdateCountDownText()
    {
        if (currentTime <= 0f)
        {
            countDownText.text = "GO!";
            return;
        }
        countDownText.text = Mathf.CeilToInt(currentTime).ToString();
    }
}
