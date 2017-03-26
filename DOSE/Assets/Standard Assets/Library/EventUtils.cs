using UnityEngine;
using System.Collections;

public static class EventUtils
{
	private static Main m_mainScript;
	private static SessionBehavior m_sessionScript;
	private static AgentInput m_agentScript;
	private static BallBehavior m_ballScript;
	private static PongServer m_pongServerScript;
	private static PongClient m_pongClientScript;
	private static BallObservation m_ballObservationScript;
	private static DataManger m_dataScript;

	/**
	 * Static constructor.
	 */
	static EventUtils()
	{
		m_mainScript = GameObject.Find("ScriptHub").GetComponent<Main>();
		m_agentScript = GameObject.Find("Agent").GetComponent<AgentInput>();
		m_sessionScript = GameObject.Find("ScriptHub").GetComponent<SessionBehavior>();
		m_ballScript = GameObject.Find("Ball").GetComponent<BallBehavior>();
		m_dataScript = GameObject.Find("ScriptHub").GetComponent<DataManger>();
		m_ballObservationScript = GameObject.Find("Ball").GetComponent<BallObservation>();
	}

	/**
	 * Announce Menu information submission complete.
	 */
	public static void AnnounceMenuInfoSubmissionComplete()
	{
		m_mainScript.GetEnvSessionInfoDone = true;
		m_pongServerScript.ConfigInfoSubmitted = true;
	}

	/**
	 * This function announces that the ball was launched.
	 */
	public static void AnnounceBallLaunch()
	{
		m_agentScript.ballLaunched = true;
		m_ballObservationScript.ballLaunced = true;
	}

	/**
	 * This function sets the PongServer or PongClient script static reference
	 * based on the role of this process.
	 */
	public static void AnnounceRoleSelection( byte _ROLE_ )
	{
		if( _ROLE_ == GeneralUtils.ROLE_SERVER )
		{
			m_pongServerScript = GameObject.Find("ScriptHub").GetComponent<PongServer>();
			GeneralUtils.m_pongServerScript = m_pongServerScript;
			m_ballScript.ServerBallBehavior = true;
			m_agentScript.ServerAgentBehavior = true;
			GameObject.Find("ScriptHub").AddComponent<StateDiagramGenerator>();
		}
		else if( _ROLE_ == GeneralUtils.ROLE_CLIENT )
		{
			m_pongClientScript = GameObject.Find("ScriptHub").GetComponent<PongClient>();
			GeneralUtils.m_pongClientScript = m_pongClientScript;
			m_ballScript.ServerBallBehavior = false;
			m_agentScript.ServerAgentBehavior = false;
		}
	}

	/**
	 * Announce that the ball hit something.
	 */
	public static void AnnounceBallHitEvent(string hitTag="")
	{
		m_pongServerScript.ballHitEvent = true;
		m_ballObservationScript.ballCollidedPaddle = true;
	}
	
	/**
	 * This function notifies all relevant scripts that the Session has begun.
	 */
	public static void AnnounceSessionBegin()
	{
		m_sessionScript.SessionBegin = true;
	}
	
	/**
	 * This function notifies all relevant scripts that the Session has ended.
	 */
	public static void AnnounceSessionEnd()
	{
		m_mainScript.SessionEnd = true;
	}

	/**
	 * This function notifies all relevant scripts that a score has occurred
	 * and for which player.
	 */
	public static void AnnouncePoint( byte _player_ )
	{
		//announce that ball is off screen if this is not Rally mode
		if (!GeneralUtils.IsRally (GeneralUtils.GetCurrentConfig ()))
			AnnounceMatchEnd ();
		
		//if the human player scored a point
		if( _player_ == GeneralUtils.HUMAN_ID )
			m_sessionScript.LeftScore += 1;
		//if the agent player scored a point
		else if( _player_ == GeneralUtils.AGENT_ID )
			m_sessionScript.RightScore += 1;
		//if a point was scored during Rally mode
		else if( _player_ == GeneralUtils.RALLY_ID ) {
			m_sessionScript.LeftScore += 1;
			m_sessionScript.RightScore += 1;
		}
	}

	/**
	 * This function notifies all relevant scripts that a match has ended.
	 */
	public static void AnnounceMatchEnd()
	{
		m_sessionScript.BallOffScreen = true;
	}

	/**
	 * This function notivies all relevant scripts that the match has
	 * ended in a DRAW.
	 */
	public static void AnnounceDraw()
	{
		m_ballScript.endedInDraw = true;
	}

	/**
	 * This function announces events to the data manager for logging.
	 */
	public static void LogEvent( byte eventType )
	{
		m_dataScript.GenEvent (eventType);
	}
}