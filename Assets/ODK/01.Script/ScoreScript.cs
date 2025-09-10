using TMPro;
using UnityEngine;

public class ScoreScript : MonoBehaviour
{
    public int redScore = 0;
    public int blueScore = 0;

    public TextMeshProUGUI redScoreText;
    public TextMeshProUGUI blueScoreText;

    void Start()
    {
        
    }

    void RedScoreUp()
    {
        redScore++;
        redScoreText.text = redScore.ToString();
    }
    void BlueScoreUp()
    {
        blueScore++;
        blueScoreText.text = blueScore.ToString();
    }

}
