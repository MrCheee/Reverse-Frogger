using TMPro;
using UnityEngine;

public class GameLogWindow : SlidingWindow
{
    [SerializeField] TextMeshProUGUI UIText;
    string gameLog;
    int logCount = 0;
    int maxCount = 50;
    string lineBreak = "<br>";

    public void AddToGameLog(string log)
    {
        if (logCount == maxCount - 1)
        {
            int lastLineBreakIndex = gameLog.LastIndexOf(lineBreak);
            gameLog = gameLog.Substring(0, lastLineBreakIndex);
        }
        else
        {
            logCount += 1;
        }
        gameLog = log + lineBreak + gameLog;
        UIText.text = gameLog;
    }
}