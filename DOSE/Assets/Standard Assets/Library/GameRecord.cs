using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Newtonsoft.Json;

[Serializable]
public class _Event
{
	/* Static Members */
	public static readonly byte MATCH_BEGIN=0;
	public static readonly byte MATCH_END=1;
	public static readonly byte SESSION_BEGIN=2;
	public static readonly byte SESSION_END=3;
	public static readonly byte FEEDBACK_BEGIN=4;
	public static readonly byte FEEDBACK_END=5;
	public static readonly byte BALL_LAUNCH=6;
	public static readonly byte BALL_HIT=7;
	public static readonly byte UNASSIGNED = 20;

	/* Member Data */
	public string time;
	public byte type;

	/**
	 * Default constructor.
	 */
	public _Event()
	{
		time = "NULL";
		type = _Event.UNASSIGNED;
	}

	/**
	 * Instance constructor.
	 */
	public _Event( byte _type_ )
	{
		time = GeneralUtils.GetTimeStamp ();
		type = _type_;
	}
}

[Serializable]
public class BallHitEvent : _Event
{
	/* Member Data */
	public Vector2 ballPosition;
	public Vector2 leftPosition;
	public Vector2 rightPosition;
	public float paddleSize;

	/**
	 * Default constructor.
	 */
	public BallHitEvent ( string _paddleID_ ) : base (_Event.BALL_HIT)
	{
		ballPosition = BallUtils.GetBallPosition ();
		leftPosition = GeneralUtils.GetHumanPosition();
		rightPosition = GeneralUtils.GetAgentPosition ();
		paddleSize = GeneralUtils.GetPaddleSize (_paddleID_);
	}
}

[Serializable]
public class BallLaunchEvent : _Event
{
	/* Member Data */
	
	/**
	 * Default constructor.
	 */
	public BallLaunchEvent() : base (_Event.BALL_LAUNCH)
	{
	}
}

[Serializable]
public class MatchBeginEvent : _Event
{
	/* Member Data */
	public int LeftScore;
	public int RightScore;

	/**
	 * Default constructor.
	 */
	public MatchBeginEvent() : base (_Event.MATCH_BEGIN)
	{
		LeftScore = GeneralUtils.GetPlayerScore ("Left");
		RightScore = GeneralUtils.GetPlayerScore ("Right");
	}
}

[Serializable]
public class MatchEndEvent : _Event
{
	/* Member Data */
	public Vector2 ballPosition;
	public Vector2 leftPosition;
	public Vector2 rightPosition;
	public float paddleSize;
	public float matchDuration;
	public int LeftScore;
	public int RightScore;
	
	/**
	 * Default constructor.
	 */
	public MatchEndEvent() : base (_Event.MATCH_END)
	{
		ballPosition = BallUtils.GetBallPosition ();
		leftPosition = GeneralUtils.GetHumanPosition();
		rightPosition = GeneralUtils.GetAgentPosition ();
		paddleSize = GeneralUtils.GetPaddleSize ("Left");
		matchDuration = GeneralUtils.TimeSinceMatchBegan ();
		LeftScore = GeneralUtils.GetPlayerScore ("Left");
		RightScore = GeneralUtils.GetPlayerScore ("Right");
	}
}

[Serializable]
public class FeedbackBeginEvent : _Event
{
	/* Member Data */
	public string feedbackContent;
	public string feedbackType;
	
	/**
	 * Default constructor.
	 */
	public FeedbackBeginEvent() : base (_Event.FEEDBACK_BEGIN)
	{
		feedbackContent = "NULL";
		feedbackType = "NULL";
	}
	
	/**
	 * Instance constructor.
	 */
	public FeedbackBeginEvent(string _feedbackContent_, string _feedbackType_) : base (_Event.FEEDBACK_BEGIN)
	{
		feedbackContent = _feedbackContent_;
		feedbackType = _feedbackType_;
	}
}

[Serializable]
public class FeedbackEndEvent : _Event
{
	/* Member Data */
	public string feedbackContent;
	public string feedbackType;
	
	/**
	 * Default constructor.
	 */
	public FeedbackEndEvent() : base (_Event.FEEDBACK_END)
	{
		feedbackContent = "NULL";
		feedbackType = "NULL";
	}
	
	/**
	 * Instance constructor.
	 */
	public FeedbackEndEvent(string _feedbackContent_, string _feedbackType_) : base (_Event.FEEDBACK_END)
	{
		feedbackContent = _feedbackContent_;
		feedbackType = _feedbackType_;
	}
}

[Serializable]
public class SessionBeginEvent : _Event
{
	/* Member Data */
	
	/**
	 * Default constructor.
	 */
	public SessionBeginEvent() : base (_Event.SESSION_BEGIN)
	{
	}
}

[Serializable]
public class SessionEndEvent : _Event
{
	/* Member Data */
	public int LeftScore;
	public int RightScore;
	
	/**
	 * Default constructor.
	 */
	public SessionEndEvent() : base (_Event.SESSION_END)
	{
		LeftScore = GeneralUtils.GetPlayerScore ("Left");
		RightScore = GeneralUtils.GetPlayerScore ("Right");
	}
}

[Serializable]
public class SerializedEvent
{
	/* Member Data */
	public byte type;
	public string jsonString;

	/**
	 * Default constructor.
	 */
	public SerializedEvent()
	{
		type = _Event.UNASSIGNED;
		jsonString = "";
	}

	/**
	 * Instance constructor.
	 */
	public SerializedEvent( byte _type_, string _jsonString_ )
	{
		type = _type_;
		jsonString = _jsonString_;
	}
}

[Serializable]
public class EventLog
{
	public List<SerializedEvent> events;

	public EventLog()
	{
		events = new List<SerializedEvent> ();
	}

	public void AddEvent( byte eventType, string eventSerialized )
	{
		SerializedEvent se = new SerializedEvent (eventType, eventSerialized);
		events.Add (se);
	}

	public void Clear()
	{
		events.Clear ();
	}

	public void Save( string filename )
	{
		string content = JsonConvert.SerializeObject (this, Formatting.Indented, GeneralUtils.jss);
		GeneralUtils.WriteContentToFile (filename, content);
	}
}

[Serializable]
public class Metadata
{
	/* Member Data */
	public string p1ID;
	public string p2ID;
	public string date;
	public int sessionID;
	public bool collaborative;
	public bool therapistPlaying;
	public byte interactionType;
	public float hrsGamesPerWeekP1;
	public float hrsGamesPerWeekP2;
	public string startOfSession;
	public string endOfSession;

	/**
	 * Default constructor.
	 */
	public Metadata()
	{
		p1ID = "NULL";
		p2ID = "NULL";
		date = "NULL";
		sessionID = -1;
		collaborative = false;
		therapistPlaying = false;
		interactionType = GeneralUtils.UNASSIGNED;
		hrsGamesPerWeekP1 = -1F;
		hrsGamesPerWeekP2 = -1F;
		startOfSession = "NULL";
		endOfSession = "NULL";
	}

	public void Save( string filename )
	{
		string content = JsonConvert.SerializeObject (this, Formatting.Indented, GeneralUtils.jss);
		GeneralUtils.WriteContentToFile (filename, content);
	}
}
