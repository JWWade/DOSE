using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjPositionSample
{
	/* Member Data */
	public long t; //time in milliseconds
	public int x; //x-component in pixels
	public int y; //y-component in pixels

	/* Static Members */

	/**
	 * Static constructor.
	 */

	/**
	 * Default constructor.
	 */
	public ObjPositionSample()
	{
		t = 0;
		x = 0;
		y = 0;
	}

	/**
	 * Instance constructor.
	 */
	public ObjPositionSample( int _x_, int _y_ )
	{
		t = VelocityUtils.GetLongMillisecond ();
		x = _x_;
		y = _y_;
	}

	/**
	 * Instance constructor.
	 */
	public ObjPositionSample( float _x_, float _y_ )
	{
		t = VelocityUtils.GetLongMillisecond ();
		x = (int)_x_;
		y = (int)_y_;
	}

	/**
	 * This method returns the position as a Vector2 object.
	 */
	public Vector2 GetVector2()
	{
		return new Vector2 ( (float)x, (float)y );
	}

	/**
	 * This method returns a nicely formatted string representation of the object.
	 */
	public override string ToString ()
	{
		return string.Format ("[PositionSamples]");
	}
}

public class ObjPositionSamples
{
	/* Member Data */
	public ObjPositionSample[] samples;
	private int W; //window size
	private int lastInsertIndex;
	private int oldestIndex;
	
	/* Static Members */
	
	/**
	 * Static constructor.
	 */

	/**
	 * Default constructor.
	 */
	public ObjPositionSamples()
	{
		samples = null;
		W = 0;
		lastInsertIndex = -1;
		oldestIndex = 0;
	}

	/**
	 * Instance constructor.
	 */
	public ObjPositionSamples( int _W_ )
	{
		samples = new ObjPositionSample[_W_];
		for(int i = 0; i < _W_; i++ )
			samples[i] = new ObjPositionSample(0,0);
		W = _W_;
		lastInsertIndex = W-1;
		oldestIndex = 0;
	}
	
	/**
	 * This method adds a PositionSample to the set of samples.
	 */
	public void AddSample( ObjPositionSample _s_ )
	{
		//calculate the index to insert the sample into
		int insertIndex = lastInsertIndex + 1;
		if(insertIndex == W)
			insertIndex = 0;

		//insert the sample into the array
		samples [insertIndex] = _s_;

		//update the last inserted sample
		lastInsertIndex = insertIndex;

		//update the oldest inserted sample
		oldestIndex = lastInsertIndex + 1;
		if(oldestIndex == W || samples[oldestIndex] == null)
			oldestIndex = 0;
	}

	/**
	 * This method returns the elapsed time in seconds between the newest and oldest samples.
	 */
	private float GetElapsedSeconds()
	{
		long elapsedMS = samples [lastInsertIndex].t - samples [oldestIndex].t;

		return (float)( elapsedMS/1000F );
	}

	/**
	 * This method returns the change in pixels as a vector v where
	 * 	v.x = samples[newest].x - samples[oldest].x and
	 * 	v.y = samples[newest].y - samples[oldest].y
	 */
	private Vector2 GetChangeInPixels()
	{
		Vector2 v = new Vector2 ();

		v.x = (float)(samples [lastInsertIndex].x - samples [oldestIndex].x);
		v.y = (float)(samples [lastInsertIndex].y - samples [oldestIndex].y);

		return v;
	}

	/**
	 * This method returns the change in position as the magnitude of a vector v where the
	 * component values' units are relative to cm/s.
	 */
	public float GetChangeInPosCmPerSec()
	{
		Vector2 v = new Vector2 ();

		//1st: get the vector relative to change in pixels
		Vector2 u = this.GetChangeInPixels ();

		//2nd: get the elapsed time
		float elapsedTime = this.GetElapsedSeconds ();

		//3rd: convert the pixel components to cm components
		v.x = u.x / VelocityUtils.k;
		v.y = u.y / VelocityUtils.k;

		//4th: return the magnitude scaled by elapsed time (e.g., cm/s)
		return v.magnitude / elapsedTime;
	}
	
	/**
	 * This method returns a nicely formatted string representation of the object.
	 */
	public override string ToString ()
	{
		return string.Format ("[PositionSamples]");
	}
}

public static class VelocityUtils
{
	
	/* Static Members */
	public static readonly float K_24INCH_MONITOR;
	public static readonly float K_23INCH_MONITOR;
	public static readonly float k;
	private static Camera CAM;
	private static ObjPositionSamples leftPaddleSamps;
	private static ObjPositionSamples rightPaddleSamps;
	private static ObjPositionSamples ballSamps;
	private static int W;
	
	/**
	 * Static constructor.
	 */
	static VelocityUtils()
	{
		K_24INCH_MONITOR = 36.226F;
		K_23INCH_MONITOR = 36.923F;
		k = K_24INCH_MONITOR; //TODO come up with way to get this dynamically
		CAM = Camera.main;
		W = 25; //TODO play with this value to find optimal
		leftPaddleSamps = new ObjPositionSamples (W);
		rightPaddleSamps = new ObjPositionSamples (W);
		ballSamps = new ObjPositionSamples (W);
	}

	/**
	 * This function returns the current time of day in milliseconds as the sum of
	 * 	ms + (1000*sec) + (1000*60*min) + (1000*60*60*hour)
	 */
	public static long GetLongMillisecond()
	{
		DateTime t = DateTime.Now;
		long longTime = (long)(t.Millisecond +
						(t.Second * 1000) +
						(t.Minute * 1000 * 60) +
						(t.Hour * 1000 * 60 * 60));
		return longTime;
	}

	/**
	 * This function adds a ObjPositionSample instance to all of the moving object's
	 * 	ObjPositionSamples list.
	 */
	public static void UpdateObjPositionSamples()
	{
		Vector2 leftPlayerPos = GeneralUtils.GetPixelPosition("leftPaddle", CAM);
		Vector2 rightPlayerPos = GeneralUtils.GetPixelPosition("rightPaddle", CAM);
		Vector2 ballPos = GeneralUtils.GetPixelPosition("ball", CAM);

		leftPaddleSamps.AddSample ( new ObjPositionSample(leftPlayerPos.x, leftPlayerPos.y) );
		rightPaddleSamps.AddSample ( new ObjPositionSample(rightPlayerPos.x, rightPlayerPos.y) );
		ballSamps.AddSample ( new ObjPositionSample(ballPos.x, ballPos.y) );
	}

	/**
	 * This function returns the velocity of the specified object.
	 */
	public static float GetVelocity( string _objName_ )
	{
		if( _objName_ == "ball" )
			return ballSamps.GetChangeInPosCmPerSec();
		else if( _objName_ == "leftPaddle" )
			return leftPaddleSamps.GetChangeInPosCmPerSec();
		else if( _objName_ == "rightPaddle" )
			return rightPaddleSamps.GetChangeInPosCmPerSec();
		else
			return 0;
	}
}



