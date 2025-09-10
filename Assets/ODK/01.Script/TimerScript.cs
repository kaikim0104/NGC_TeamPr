using NUnit.Framework;
using TMPro;
using UnityEngine;

public class TimerScript : MonoBehaviour
{
    private TextMeshProUGUI timerText;
    private float maxTime = 180f;
    [SerializeField] private float elapsedTime;
    private bool isRunning;
    private bool isReversed = true;
    private void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        
        UpdateTimerDisplay();
    }

    private void Update()
    {

        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
        UpdateTimerDisplay();
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        if (elapsedTime > maxTime - 30)
        {
            timerText.color = Color.red;
        }
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


}
