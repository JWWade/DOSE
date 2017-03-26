using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SessionTimer
{
	/* Member Data */
	public int m_duration; //intended duration of session in milliseconds
	public DateTime m_EasyStartTime;
	public DateTime m_MedStartTime;
	public DateTime m_HardStartTime;
	public DateTime m_EndTime;

	/**
	 * Default constructor.
	 */
	public SessionTimer()
	{
		m_duration = 0;
		m_EasyStartTime = DateTime.Now;
		m_MedStartTime = DateTime.Now;
		m_HardStartTime = DateTime.Now;
		m_EndTime = DateTime.Now;
	}

	/**
	 * Instance constructor.
	 */
	public SessionTimer(int _duration_)
	{
		m_duration = _duration_;
		m_EasyStartTime = DateTime.Now;
		m_MedStartTime = DateTime.Now;
		m_HardStartTime = DateTime.Now;
		m_EndTime = DateTime.Now;
	}

	/**
	 * This method is called to "set" the timer and initializes
	 * all of the member DateTime objects.
	 */
	public void StartTimer()
	{
		int thirdDuration = m_duration / 3;
		m_EasyStartTime = DateTime.Now;
		m_MedStartTime = DateTime.Now.AddMilliseconds ((double)thirdDuration);
		m_HardStartTime = DateTime.Now.AddMilliseconds ((double)thirdDuration * 2);
		m_EndTime = DateTime.Now.AddMilliseconds ((double)thirdDuration * 3);
	}

	/**
	 * This method returns the elapsed number of milliseconds between the current time
	 * and the start time.
	 */
	public int ElapsedTime()
	{
		return (int)DateTime.Now.Subtract (m_EasyStartTime).TotalMilliseconds;
	}

	/**
	 * This method returns true if the specified third of the session has finished.
	 */
	public bool NthThirdCompleted(int _NthThird_)
	{
		//if querying about the first third of the session (i.e., EASY)
		if( _NthThird_ == 1 )
		{
			//calculate the timespan from now to the EASY end time
			int ts = (int)DateTime.Now.Subtract(m_MedStartTime).TotalMilliseconds;

			//if the value is positive, then the EASY third is complete
			return ts > 0;
		}
		//if querying about the first third of the session (i.e., MEDIUM)
		else if( _NthThird_ == 2 )
		{
			//calculate the timespan from now to the MEDIUM end time
			int ts = (int)DateTime.Now.Subtract(m_HardStartTime).TotalMilliseconds;
			
			//if the value is positive, then the MEDIUM third is complete
			return ts > 0;
		}
		//if querying about the first third of the session (i.e., HARD)
		else if( _NthThird_ == 3 )
		{
			//calculate the timespan from now to the HARD end time
			int ts = (int)DateTime.Now.Subtract(m_EndTime).TotalMilliseconds;
			
			//if the value is positive, then the HARD third is complete
			return ts > 0;
		}
		//otherwise
		else
			throw new Exception("Invalid query for segment of session: " +
			                    _NthThird_.ToString() + ". Valid range = {1,2,3}.");
	}

	/**
	 * This method returns the current Match difficulty based on the current time.
	 */
	public byte GetCurrentMatchDifficulty()
	{
		//if the session is complete
		if( NthThirdCompleted(3) )
			return Match.INVALID;
		//if the final third is in progress (HARD)
		else if( NthThirdCompleted(2) )
			return Match.HARD;
		//if the second third is in progress (MEDIUM)
		else if( NthThirdCompleted(1) )
			return Match.MEDIUM;
		//otherwise, the first third is in progress (EASY)
		else
			return Match.EASY;
	}

	/**
	 * This method returns true if the Session is complete. Otherwise returns false.
	 */
	public bool SessionComplete()
	{
		//calculate the timespan from now until the end of the session
		int ts = (int)DateTime.Now.Subtract (m_EndTime).TotalMilliseconds;

		//if the value is positive, then the end of the session has already passed
		return ts > 0;
	}
}