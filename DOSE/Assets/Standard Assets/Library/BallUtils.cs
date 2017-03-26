using UnityEngine;
using System.Collections;

public static class BallUtils
{
	private static BallBehavior m_ballScript;
	private static BallObservation m_ballObservationScript;

	/**
	 * Static constructor.
	 */
	public static void InitBallUtilities()
	{
		m_ballScript = GameObject.Find("Ball").GetComponent<BallBehavior>();
		m_ballObservationScript = GameObject.Find("Ball").GetComponent<BallObservation>();
	}

	/**
	 * This function returns the position of the ball.
	 */
	public static Vector2 GetBallPosition()
	{
		Vector2 v = m_ballScript.transform.position;
		return new Vector2( v.x, v.y );
	}

	/**
	 * This function returns the velocity of the ball.
	 */
	private static Vector2 cachedVelocity=Vector2.zero;
	public static Vector2 GetBallVelocity()
	{
		Vector2 v = m_ballObservationScript.GetBallVelocity ();
		if(v == Vector2.zero)
			return cachedVelocity;
		else
		{
			cachedVelocity = v;
			return v;
		}
	}

	/**
	 * This function updates the ball size with the specified value.
	 */
	public static void SetBallSize( float v )
	{
		Vector3 currSize = m_ballScript.transform.localScale;
		Vector3 newSize = currSize;
		newSize.x = v;
		newSize.y = v;
		
		//set ball size
		m_ballScript.transform.localScale = newSize;
	}
}