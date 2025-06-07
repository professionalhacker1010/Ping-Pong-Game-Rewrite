using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPC_Opponent : BallPathCalculator
{
    public BPC_Opponent(List<Vector2> rangeY, float normalizer) : base(rangeY, normalizer) { }
    public virtual void CalcBallPath(BallPath ballPath, out BallPathInfo pathInfo, Vector3 opponentBallPath, float startX, float startY)
    {
        //Debug.Log("calc opponent ball path");
        //temporarily reverse trueMaxY and tableY
        //trueMaxY.Reverse(); tableY.Reverse();
        
        currBallPath = ballPath;
        finalX = new List<float>(rangeY.Count);
        finalY = new List<float>(rangeY.Count);

        //get end and max from opponent variable
        float endX = opponentBallPath.x;
        float maxY = opponentBallPath.y + normalizer;

        //predetermined hits will input the end point of the ball - however the calcopponentballpath function take the Y as the max height, not the end point
        //so I'm fixing that here lol. AND ASSUMING THE SECOND BALL FALLS 75% OF MAX HEIGHT. THAT MIGHT CHANGE IN THE FUTURE.

        //LOW ball or HIGH ball?
        bool isLowBall = ((GetScaledY(0, rangeY.Count - 1, maxY) - rangeY[0].x) * 4 / 3f) + rangeY[0].x <= startY; //lowball when 
        if (isLowBall)
        {
            currBallPath.SwitchPath(BallPathType.LOW); //JUST TESTING
            //Debug.Log("LOW BALL------------");
        }
        else
        {
            currBallPath.SwitchPath(BallPathType.HIGH);
            //Debug.Log("HIGH BALL------------");
        }

        //Debug.Log("Start: " + startY.ToString() + " Max: " + GetScaledY(0, tableY.Count - 1, (maxY * 4/3f)).ToString());

        //calculate ball heights for each frame
        finalY = new List<float>(currBallPath.endFrame);
        CalcAllY(currBallPath.highestFrame, currBallPath.endFrame, startY, maxY, isLowBall);

        //calculate X position for each frame
        finalX = new List<float>(currBallPath.endFrame);
        LerpX(currBallPath.endFrame,endX, startX);

        pathInfo.x = finalX;
        pathInfo.y = finalY;
    }

    private void CalcAllY(int highestFrame, int endFrame, float startY, float maxY, bool lowBall)
    {
        //Debug.Log("End Y before scaled: " + maxY.ToString());
        //float endY = maxY;
        if (!lowBall) maxY = ((GetScaledY(0, rangeY.Count - 1, maxY) - rangeY[0].x) * 4 / 3f) + rangeY[0].x;
        else
        {
            maxY = startY;
            //endY = startY * .75f;
        }

        if (maxY > rangeY[0].y)
        {
            maxY = rangeY[0].y;
           // endY = maxY * .75f;
        }

        //assumes maxY and startY are RELATIVE TO FIRST FRAME
        //before reaching max height
        for (int i = 0; i < endFrame; i++)
        {
            float scaledMaxY = GetScaledY(i, endFrame - 1, maxY);
            if (i < highestFrame)
            {
                float scaledStartY = GetScaledY(i, 0, startY);
                float maxHeightGain = scaledMaxY - scaledStartY; //range from the starting height of hit to max height
                float height = (CalcArc1(i) * maxHeightGain) + scaledStartY; //scale the height to the arc, then add back starting height
                finalY.Add(height - normalizer); //subtract normalizer for final Y coordinate
            }
            else if (i < currBallPath.bounceFrame)
            {
                float maxHeightGain = scaledMaxY - rangeY[i].x; //range from max height to table
                float height = (CalcArc1(i) * maxHeightGain) + rangeY[i].x; //scale the height to the arc, then add back table height
                finalY.Add(height - normalizer);
            }
            else
            {
                float maxHeightGain = (scaledMaxY - rangeY[i].x) * 0.75f; //jank?
                float height = (CalcArc2(i) * maxHeightGain) + rangeY[i].x;
                finalY.Add(height - normalizer);
            }
        }
    }
}
