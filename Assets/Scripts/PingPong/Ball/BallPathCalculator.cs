using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BallPathInfo
{
    public List<float> y;
    public List<float> x;
}

public abstract class BallPathCalculator
{
    //passed in externally
    protected List<Vector2> rangeY;
    protected List<float> heightY;
    protected float normalizer;

    protected BallPath currBallPath;

    //final transform calculations - change during game's runtime
   // protected float maxY; //this is a raw maxY
   // protected float endX;
    protected List<float> finalY;
    protected List<float> finalX;

    public BallPathCalculator(List<Vector2> rangeY, float normalizer)
    {
        this.rangeY = new List<Vector2>(rangeY);
        this.normalizer = normalizer;

        heightY = new List<float>();
        foreach (var item in rangeY)
        {
            heightY.Add(item.y - item.x);
        }
    }

    #region Sandbox functions
    /// <summary>
    /// calc X by lerping between endX and startX
    /// </summary>
    /// <param name="frames"></param>
    protected void LerpX(int frames, float endX, float startX)
    {
        float increment = (endX - startX) / (frames);
        for (int i = 0; i < frames; i++)
        {
            finalX.Add(startX + (increment * i));
        }
    }

    /// <summary>
    /// Y of first bounce
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    protected float CalcArc1(int frame)
    {
        return currBallPath.a1 * (frame - currBallPath.x1) * (frame - currBallPath.x2);
    }

    /// <summary>
    /// Y of second bounce
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    protected float CalcArc2(int frame)
    {
        return currBallPath.a2 * (frame - currBallPath.x2) * (frame - currBallPath.x3);
    }

    /// <summary>
    /// Y scaled for 'depth'
    /// </summary>
    /// <param name="desiredFrame"></param>
    /// <param name="currFrame"></param>
    /// <param name="currY"></param>
    /// <returns></returns>
    protected float GetScaledY(int desiredFrame, int currFrame, float currY)
    {
        float currMinY = rangeY[currFrame].x;
        float currHeight = heightY[currFrame];

        float desiredMinY = rangeY[desiredFrame].x;
        float desiredHeight = heightY[desiredFrame];

        //what is the % height of currY?
        float percentY = (currY - currMinY) / (currHeight);

        //apply that percent to desired frame
        float Y = (percentY * (desiredHeight)) + desiredMinY;
        return Y;
    }
    #endregion
}
