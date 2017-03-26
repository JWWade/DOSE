using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Newtonsoft.Json;

[Serializable]
public class Session
{
	/* Member Data */
	public SessionTimer m_sessTimer;
	public Configuration m_config;

	/**
	 * Default constructor.
	 */
	public Session()
	{
		m_sessTimer = new SessionTimer (0);
		m_config = null;
	}

	/**
	 * Instance constructor.
	 */
	public Session( SessionTimer _sessTimer_, Configuration _config_ )
	{
		m_sessTimer = _sessTimer_;
		m_config = _config_;
	}

	/**
	 * Instance constructor.
	 * 	Create a new session from a JSON serialized representation of the object.
	 */
	public Session( string jsonString )
	{
		Session temp = JsonConvert.DeserializeObject<Session> (jsonString);
		this.m_sessTimer = temp.m_sessTimer;
		this.m_config = temp.m_config;
	}

	/**
	 * This method returns the current match difficulty.
	 */
	public byte GetDifficulty()
	{
		return m_sessTimer.GetCurrentMatchDifficulty ();
	}

	/**
	 * This method returns the JSON-serialized representation of the object.
	 */
	public string ToJsonString()
	{
		return JsonConvert.SerializeObject (this);
	}

	/**
	 * This method returns a nicely formatted string representation of the object.
	 */
	public override string ToString ()
	{
		return string.Format ("[Session]");
	}
}