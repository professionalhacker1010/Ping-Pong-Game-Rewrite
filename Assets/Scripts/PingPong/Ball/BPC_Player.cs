using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPC_Player : BallPathCalculator
{
    public BPC_Player(List<Vector2> rangeY, float normalizer) : base(rangeY, normalizer) { }

    public void CalcBallPath(BallPath ballPath, out BallPathInfo pathInfo, float playerHitHeight, float playerHitLateral, float startX, float startY, 
        float opponentX, float opponentY)
    {
        currBallPath = ballPath;
        finalY = new List<float>(rangeY.Count);
        finalX = new List<float>(rangeY.Count);

        //calculate max ball height
        float endRefY = GetScaledY(rangeY.Count - 1, 0, startY);
        float maxY = CalcMaxY(playerHitHeight, endRefY);

        //LOW ball or HIGH ball?
        if (maxY <= startY)
        {
            maxY = startY;
            currBallPath.SwitchPath(BallPathType.LOW);
        }
        else
        {
            currBallPath.SwitchPath(BallPathType.HIGH);
        }

        //calculate scaled ball heights for each frame
        finalY = new List<float>(currBallPath.endFrame);
        CalcAllY(currBallPath.highestFrame, currBallPath.endFrame, startY, maxY);

        //calculate end X
        float endX = CalcEndX(playerHitLateral, startX);

        //calculate X position for each frame
        finalX = new List<float>(currBallPath.endFrame);
        LerpX(currBallPath.endFrame, endX, startX);

        pathInfo.x = finalX;
        pathInfo.y = finalY;
    }

    private void CalcAllY(int highestFrame, int endFrame, float startY, float maxY)
    {
        //assumes maxY and startY are RELATIVE TO FIRST FRAME
        for (int i = 0; i < endFrame; i++)
        {
            float scaledMaxY = GetScaledY(i, 0, maxY);

            if (i < highestFrame) //before reaching max height
            {
                float scaledStartY = GetScaledY(i, 0, startY);
                float maxHeightGain = scaledMaxY - scaledStartY; //range from the starting height of hit to max height
                float height = (CalcArc1(i) * maxHeightGain) + scaledStartY; //scale the height to the arc, then add back starting height
                finalY.Add(height - normalizer); //subtract normalizer for final Y coordinate
            }
            else if (i < currBallPath.bounceFrame) //after max height, before bounce
            {
                float maxHeightGain = scaledMaxY - rangeY[i].x; //range from max height to table
                float height = (CalcArc1(i) * maxHeightGain) + rangeY[i].x; //scale the height to the arc, then add back table height
                finalY.Add(height - normalizer);
            }
            else //after bounce
            {
                float maxHeightGain = scaledMaxY - rangeY[i].x; //range from max height to table
                float height = (CalcArc2(i) * maxHeightGain) + rangeY[i].x;
                finalY.Add(height - normalizer);
            }
        }
    }

    /// <summary>
    /// calculate max height of ball, relative to first position in animation
    /// </summary>
    /// <returns></returns>
    private float CalcMaxY(float playerHitHeight, float refY)
    {
        float max = refY + playerHitHeight; //GetScaledY(0, finalY.Count-1, finalY[finalY.Count-1])
        float trueMax = rangeY[0].y;
        return Mathf.Clamp(max, 0, trueMax);
    }

    /// <summary>
    /// needed for calculating all X
    /// </summary>
    /// <returns></returns>
    private float CalcEndX(float playerHitLateral, float refX)
    {
        float end = refX + playerHitLateral;//(paddleX * factorPaddleX);
        return end;
    }
}
