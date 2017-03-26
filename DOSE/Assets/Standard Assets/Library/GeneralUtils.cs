using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public static class GeneralUtils
{
	/* Static Members */
	private static Main m_mainScript;
	private static AgentInput m_agentScript;
	private static HumanInput m_humanScript;
	private static SessionBehavior m_sessionScript;
	private static MenuBehavior m_menuScript;
	private static BallBehavior m_ballScript;
	private static DataManger m_dataScript;
	public static PongServer m_pongServerScript;
	public static PongClient m_pongClientScript;
	public static byte ROLE_SERVER;
	public static byte ROLE_CLIENT;
	public static byte HUMAN_ID;
	public static byte HUMAN_LEFT;
	public static byte HUMAN_RIGHT;
	public static byte AGENT_ID;
	public static byte AGENT_SKILL_RANDOM;
	public static byte AGENT_SKILL_NORMAL;
	public static byte AGENT_SKILL_FAST;
	public static byte AGENT_SKILL_PERFECT;
	public static byte RALLY_ID;
	public static float BALL_RADIUS_LARGE;
	public static float BALL_RADIUS_MEDIUM;
	public static float BALL_RADIUS_SMALL;
	public static float PADDLE_SIZE_LARGE;
	public static float PADDLE_SIZE_MEDIUM;
	public static float PADDLE_SIZE_SMALL;
	public static float PADDLE_WIDTH_MEDIUM;
	public static byte UNASSIGNED;
	public static byte INPUT_KEYBOARD;
	public static byte INPUT_MOUSE;
	public static byte INPUT_HYDRA;
	public static string ASSIGNED_PONG_CLIENT_ID;
	public static string PONG_SERVER_ID;
	public static string PONG_CLIENT1_ID;
	public static string PONG_CLIENT2_ID;
	public static string RASyN_IP;
	public static Dictionary<string,int> RASyN_PORT_MAP;
	public static JsonSerializerSettings jss;
	public static int DRAW_TIMEOUT;
	public static RallyConfiguration rallyCfg;
	
	/**
	 * Static constructor.
	 */
	public static void InitGeneralUtils()
	{
		BallUtils.InitBallUtilities ();
		NetUtils.InitNetUtils ();
		m_mainScript = GameObject.Find("ScriptHub").GetComponent<Main>();
		m_agentScript = GameObject.Find("Agent").GetComponent<AgentInput>();
		m_humanScript = GameObject.Find("P1").GetComponent<HumanInput>();
		m_sessionScript = GameObject.Find("ScriptHub").GetComponent<SessionBehavior>();
		m_menuScript = GameObject.Find("ScriptHub").GetComponent<MenuBehavior>();
		m_ballScript = GameObject.Find("Ball").GetComponent<BallBehavior>();
		m_dataScript = GameObject.Find("ScriptHub").GetComponent<DataManger>();
		ROLE_SERVER = 0;
		ROLE_CLIENT = 1;
		HUMAN_ID = 0;
		HUMAN_LEFT = 2;
		HUMAN_RIGHT = 3;
		AGENT_ID = 1;
		AGENT_SKILL_RANDOM = 0;
		AGENT_SKILL_NORMAL = 1;
		AGENT_SKILL_FAST = 2;
		RALLY_ID = 2;
		UNASSIGNED = 10;
		INPUT_KEYBOARD = 0;
		INPUT_MOUSE = 1;
		INPUT_HYDRA = 2;
		BALL_RADIUS_LARGE = 2;
		BALL_RADIUS_MEDIUM = 1;
		BALL_RADIUS_SMALL = 0.5F;
		PADDLE_SIZE_LARGE = 9.0757605F; //150%
		PADDLE_SIZE_MEDIUM = 6.050507F; //100%
		PADDLE_SIZE_SMALL = 4.53788025F; //75%
		PADDLE_WIDTH_MEDIUM = 7.41716F;
		RASyN_IP = "127.0.0.1";
		RASyN_PORT_MAP = new Dictionary<string,int> ();
		PONG_SERVER_ID = "PongServer";
		PONG_CLIENT1_ID = "PongClient1";
		PONG_CLIENT2_ID = "PongClient2";
		ASSIGNED_PONG_CLIENT_ID = "NULL";
		RASyN_PORT_MAP.Add (PONG_SERVER_ID, 19000);
		RASyN_PORT_MAP.Add (PONG_CLIENT1_ID, 19011);
		RASyN_PORT_MAP.Add (PONG_CLIENT2_ID, 19012);
		jss = new JsonSerializerSettings ();
		jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		jss.MissingMemberHandling = MissingMemberHandling.Ignore;
		//jss.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
		DRAW_TIMEOUT = 1000 * 60; //60 second timeout in milliseconds
		BallUtils.InitBallUtilities ();
		NetUtils.InitNetUtils ();
		rallyCfg = new RallyConfiguration (Application.dataPath + "/Config/RallyConfiguration.json");
	}

	/**
	 * This function returns the role of this Process (i.e., server or client).
	 */
	public static byte GetProcessRole()
	{
		return m_mainScript.ProcessRole;
	}

	/**
	 * This function returns true if the Main Driver is in the state for accepting
	 * user-input to configure the Session.
	 */
	public static bool GettingSessionInfoFromUser()
	{
		if (m_mainScript.mainAuto == null)
		{
			m_mainScript = GameObject.Find("ScriptHub").GetComponent<Main>();
			return false;
		}
		else
			return m_mainScript.mainAuto.CurrState == MainAutomaton.SERVER_GET_ENV_SESSION_INFO_FROM_USER;
	}

	/**
	 * This function returns true if the Session is in progress.
	 * Otherwise, returns false.
	 */
	public static bool SessionInProgress()
	{
		return m_mainScript.sessionInProgress;
	}
    
	/**
	 * This function returns true if the Match is in progress.
	 */
	public static bool MatchInProgress()
	{
		return m_sessionScript.matchInProgress;
	}

	/**
	 * This function returns the elapsed time since the current Session began.
	 * The value returned is in milliseconds.
	 */
	public static int TimeSinceSessionBegan()
	{
		return m_sessionScript.timeSinceSessionBegan;
	}

	/**
	 * This function returns the elapsed time since the current Match began.
	 * The value returned is in milliseconds.
	 */
	public static int TimeSinceMatchBegan()
	{
		return m_sessionScript.timeSinceMatchBegan;
	}

	/**
	 * This function returns the score of the specified player.
	 */
	public static int GetPlayerScore(string plid)
	{
		if(plid=="Left")
			return GeneralUtils.m_sessionScript.LeftScore;
		else if(plid=="Right")
			return GeneralUtils.m_sessionScript.RightScore;
		else
			return -1;
	}

	/**
	 * This function returns the score of both players when playing in Rally mode.
	 */
	public static int GetRallyScore()
	{
		//if the current is not Rally mode, return -1
		if( !IsRally( GeneralUtils.GetCurrentMatch().configuration.shorthandConfig ) )
			return -1;
		//otherwsie, return left player score since it is the same as the right player score
		else
			return GetPlayerScore("Left");
	}

	/**
	 * This function returns the specified objects pixel position on screen.
	 */
	public static Vector2 GetPixelPosition( string _objName_, Camera _cam_ )
	{
		Vector2 v = new Vector2 ();

		if( _objName_ == "ball" )
			v = _cam_.WorldToScreenPoint( m_ballScript.transform.position );
		else if( _objName_ == "leftPaddle" )
			v = _cam_.WorldToScreenPoint( m_humanScript.transform.position );
		else if( _objName_ == "rightPaddle" )
			v = _cam_.WorldToScreenPoint( m_agentScript.transform.position );

		return v;
	}

	/**
	 * This function returns the position of the agent.
	 */
	public static Vector2 GetAgentPosition()
	{
		Vector3 v = m_agentScript.transform.position;
		return new Vector2( v.x, v.y );
	}

	/**
	 * This function returns the local scale of the agent.
	 */
	public static Vector2 GetAgentScale()
	{
		Vector3 s = m_agentScript.transform.localScale;
		if(s.x==0 || s.y==0)
			return new Vector2(1,1);
		else
			return new Vector2 (s.x, s.y);
	}

	/**
	 * This function returns the position of the human player.
	 */
	public static Vector2 GetHumanPosition()
	{
		Vector3 v = m_humanScript.transform.position;
		return new Vector2( v.x, v.y );
	}

	/**
	 * This function returns the local scale of the human player.
	 */
	public static Vector2 GetHumanScale()
	{
		Vector3 s = m_humanScript.transform.localScale;
		if(s.x==0 || s.y==0)
			return new Vector2(1,1);
		else
			return new Vector2 (s.x, s.y);
	}

	/**
	 * This function returns the specified participant ID.
	 */
	public static string GetParticipantID(int parID)
	{
		if(parID==1)
			return m_menuScript.p1ID;
		else if(parID==2)
			return m_menuScript.p2ID;
		else
			return "NULL";
	}

	/**
	 * This function returns the id of the participant or
	 * participants if applicable.
	 */
	public static List<string> GetParticipantID()
	{
		List<string> participants = new List<string> ();

		if( m_menuScript.p1ID != "UNASSIGNED" )
			participants.Add( m_menuScript.p1ID );
		if( m_menuScript.p2ID != "UNASSIGNED" )
			participants.Add( m_menuScript.p2ID );

		return participants;
	}

	/**
	 * This function returns a timestamp of the format
	 * HH:MM:SS:mmm
	 */
	public static string GetTimeStamp()
	{
		string s = "";
		DateTime d = DateTime.Now;

		s += d.Hour.ToString () + ":";
		s += d.Minute.ToString () + ":";
		s += d.Second.ToString () + ":";
		s += d.Millisecond.ToString ();

		return s;
	}

	/**
	 * This function returns the current date including the time in the format
	 * MM_DD_YYYY__HH:MM:SS:mmm
	 */
	public static string GetDate()
	{
		string s = "";
		DateTime d = DateTime.Now;

		s += d.Month.ToString () + "_";
		s += d.Day.ToString () + "_";
		s += d.Year.ToString () + "__";
		s += d.Hour.ToString () + "_";
		s += d.Minute.ToString () + "_";
		s += d.Second.ToString () + "_";
		s += d.Millisecond.ToString ();

		return s;
	}

	/**
	 * This function reads the content from the specified file.
	 */
	public static string ReadContentFromFile( string filepath )
	{
		StreamReader sr = new StreamReader (filepath);
		string content = sr.ReadToEnd ();
		sr.Close ();
		return content;
	}

	/**
	 * This function writes the content to the specfied file.
	 */
	public static void WriteContentToFile( string filepath, string content )
	{
		StreamWriter sw = new StreamWriter (filepath);
		sw.Write (content);
		sw.Close ();
	}

	/**
	 * This method returns true if the menu information submission is completed.
	 */
	public static bool MenuInfoSubmitted()
	{
		return m_mainScript.GetEnvSessionInfoDone;
	}

	/**
	 * This function returns the number of human players.
	 */
	public static int GetNumHumanPlayers()
	{
		return m_menuScript.NumHumanPlayers;
	}

	/**
	 * This function returns the name of human player 1.
	 */
	public static string GetHuman1Name()
	{
		return m_menuScript.p1ID;
	}

	/**
	 * This function returns the name of human player 2.
	 */
	public static string GetHuman2Name()
	{
		return m_menuScript.p2ID;
	}

	/**
	 * This function returns the selected session path.
	 */
	public static string GetSelectedSessionPath()
	{
		return m_menuScript.GetSelectedSessionPath ();
	}

	/**
	 * This function returns the selected session name.
	 */
	public static string GetSelectedSessionName()
	{
		return m_menuScript.GetSelectedSessionNme ();
	}

	/**
	 * This function stores the current input data from the human player.
	 * Called only by PongServer.cs script. pid is in {"PongClient1", "PongClient2"}
	 */
	public static void StoreHumanInput( InputData inputData, string pid )
	{
		//only update human position if a Match is in progress
		if( GeneralUtils.MatchInProgress() )
		{
			byte config = GeneralUtils.GetCurrentConfig();

			int dir1=0,dir2=0;
			if( config == Configuration.HvH_1PC )
			{
				if( inputData.inputVal1 < 0 )
					dir1 = -1;
				else if( inputData.inputVal1 > 0 )
					dir1 = 1;
				else
					dir1 = 0;
				
				if( inputData.inputVal2 < 0 )
					dir2 = -1;
				else if( inputData.inputVal2 > 0 )
					dir2 = 1;
				else
					dir2 = 0;
				
				m_humanScript.StoreHumanInput(dir1, pid, "Left");
				m_humanScript.StoreHumanInput(dir2, pid, "Right");
			}
			else if( config == Configuration.HvH_2PC )
			{
				//if client process 1
				if( pid == GeneralUtils.PONG_CLIENT1_ID )
				{
					if( inputData.inputVal1 < 0 )
						dir1 = -1;
					else if( inputData.inputVal1 > 0 )
						dir1 = 1;
					else
						dir1 = 0;
					m_humanScript.StoreHumanInput(dir1, pid, "Left");
				}
				//if client process 2
				else if( pid == GeneralUtils.PONG_CLIENT2_ID )
				{
					if( inputData.inputVal2 < 0 )
						dir2 = -1;
					else if( inputData.inputVal2 > 0 )
						dir2 = 1;
					else
						dir2 = 0;
					m_humanScript.StoreHumanInput(dir2, pid, "Right");
				}
			}
			else if( config == Configuration.Rally_1PC )
			{
				if( inputData.inputVal1 < 0 )
					dir1 = -1;
				else if( inputData.inputVal1 > 0 )
					dir1 = 1;
				else
					dir1 = 0;
				
				if( inputData.inputVal2 < 0 )
					dir2 = -1;
				else if( inputData.inputVal2 > 0 )
					dir2 = 1;
				else
					dir2 = 0;
				
				m_humanScript.StoreHumanInput(dir1, pid, "Left");
				m_humanScript.StoreHumanInput(dir2, pid, "Right");
			}
			else if( config == Configuration.Rally_2PC )
			{
				//if client process 1
				if( pid == GeneralUtils.PONG_CLIENT1_ID )
				{
					if( inputData.inputVal1 < 0 )
						dir1 = -1;
					else if( inputData.inputVal1 > 0 )
						dir1 = 1;
					else
						dir1 = 0;
					m_humanScript.StoreHumanInput(dir1, pid, "Left");
				}
				//if client process 2
				else if( pid == GeneralUtils.PONG_CLIENT2_ID )
				{
					if( inputData.inputVal2 < 0 )
						dir2 = -1;
					else if( inputData.inputVal2 > 0 )
						dir2 = 1;
					else
						dir2 = 0;
					m_humanScript.StoreHumanInput(dir2, pid, "Right");
				}
			}
			else
			{
				if( inputData.inputVal1 < 0 )
					dir1 = -1;
				else if( inputData.inputVal1 > 0 )
					dir1 = 1;
				else
					dir1 = 0;
				
				m_humanScript.StoreHumanInput(dir1, pid, "");
			}

		}
	}

	/**
	 * This function applies changes to the position of the human's/humans' paddle.
	 * Called only by PongServer.cs script.
	 */
	public static void ApplyHumanPaddleChanges()
	{
		m_humanScript.UpdatePosition ();
	}

	/**
	 * This function returns true if the user has chosen the Process Role.
	 */
	public static bool RoleChosen()
	{
		if (m_mainScript.ProcessRole != GeneralUtils.UNASSIGNED)
			return true;
		else
			return false;
	}

	/**
	 * This function calculates the destination of the agent paddle.
	 */
	public static Vector2 GetAgentDestination()
	{
	Vector2 intersection;

		Vector2 b = BallUtils.GetBallPosition();
		Vector2 v = BallUtils.GetBallVelocity();
		float m = (v.x==0 ? 0 : (v.y/v.x));
		float x_dest = 42.47662F;
		float y_intercept = ((-1F*b.x*v.y)/v.x) + b.y;

		intersection = new Vector2( x_dest, m*x_dest + y_intercept );

		return intersection;
	}

	/**
	 * This function returns the current EnvState object.
	 */
	public static EnvState GetEnvState( string extraInfo="NULL" )
	{
		int humanScore = m_sessionScript.LeftScore;
		int agentScore = m_sessionScript.RightScore;
		Position3D ballOrientation = Position3D.Vector3ToPosition3D(m_ballScript.transform.eulerAngles);
		Position2D humanPos = Position2D.Vector2ToPosition2D(GeneralUtils.GetHumanPosition ());
		Position2D agentPos = Position2D.Vector2ToPosition2D(GeneralUtils.GetAgentPosition ());
		Position2D ballPos = Position2D.Vector2ToPosition2D(BallUtils.GetBallPosition ());
		float leftPaddleLen = m_humanScript.transform.localScale.y;
		float rightPaddleLen = m_agentScript.transform.localScale.y;
		float leftPaddleWidth = m_humanScript.transform.localScale.x;
		float rightPaddleWidth = m_agentScript.transform.localScale.x;
		Match currMatch = m_sessionScript.currMatch;
		int sessionState = m_sessionScript.sessionAuto.CurrState;

		EnvState envState = new EnvState (humanScore, agentScore, ballOrientation, ballPos,
		                                  agentPos, humanPos, leftPaddleLen, rightPaddleLen,
		                                  leftPaddleWidth, rightPaddleWidth,
		                                  sessionState, currMatch, extraInfo);
		return envState;
	}

	/**
	 * This function returns the EnvState object received from the server (used only by clients).
	 */
	public static EnvState GetServerEnvState()
	{
		if( m_pongClientScript != null && m_pongClientScript.envState != null &&
		   m_pongClientScript.envState.sessionState != SessionAutomaton.SESSION_NOT_IN_PROGRESS )
			return m_pongClientScript.envState;
		else
			return new EnvState(0,0,null,null,
			                    null,null,0,0,
			                    0,0,
			                    SessionAutomaton.SESSION_NOT_IN_PROGRESS,null);
	}

	/**
	 * This function sets the environment settings based on the EnvState object.
	 * Only Client processes call this function.
	 */
	public static void SetEnvState( EnvState envState )
	{
		m_sessionScript.LeftScore = envState.leftScore;
		m_sessionScript.RightScore = envState.rightScore;
		m_humanScript.transform.position = envState.humanPos.ToVector2();
		m_agentScript.transform.position = envState.agentPos.ToVector2();
		GeneralUtils.SetPaddleSize ("leftPaddle", envState.leftPaddleLen, envState.leftPaddleWidth);
		GeneralUtils.SetPaddleSize ("rightPaddle", envState.rightPaddleLen, envState.rightPaddleWidth);
		m_ballScript.transform.position = envState.ballPos.ToVector2();
		m_ballScript.transform.eulerAngles = envState.ballOrientation.ToVector3();
		m_sessionScript.currMatch = envState.currMatch;

		try{
			GeneralUtils.UpdateEnvironmentDifficulty (envState.currMatch.difficulty);
		}catch(Exception e){}

		if( envState.extraInfo.Contains("ballHit") )
		{
			//play pong hit sound
			m_ballScript.PlayBallHitSound();
		}
	}

	/**
	 * This function sets resets the Human Player's paddle position.
	 */
	public static void ResetHumanPosition( Vector3 v )
	{
		m_humanScript.transform.position = v;
	}

	/**
	 * This function returns the current session ID.
	 */
	public static int GetSessionID()
	{
		return m_menuScript.SelectedSession;
	}

	/**
	 * This function returns the estimated number of hours of gameplay per week
	 * of the participant based on the specified participant.
	 */
	public static float GetHrsGamesPerWeek( int pID )
	{
		if( pID == 1 )
			return m_menuScript.hrsGamePerWeekP1;
		else if( pID == 2 )
			return m_menuScript.hrsGamePerWeekP2;
		else
			return -1F;
	}
	
//	/**
//	 * This function notifies all relevant scripts that the Match has begun.
//	 */
//	public static void AnnounceMatchBegin()
//	{
//	}

	/**
	 * This function returns the human input value based on the client process id.
	 * This is only used by clients.
	 */
	public static float GetHumanInput()
	{
		if( GeneralUtils.ASSIGNED_PONG_CLIENT_ID == GeneralUtils.PONG_CLIENT1_ID )
			return m_humanScript.h1Input;
		else if( GeneralUtils.ASSIGNED_PONG_CLIENT_ID == GeneralUtils.PONG_CLIENT2_ID )
			return m_humanScript.h2Input;
		else
			return 9999F;
	}

	/**
	 * This function returns the human input value based on the supplied parameter:
	 * 	1 => human 1 input value
	 * 	2 => human 2 input value
	 * 	3 => composite human input value
	 */
	public static float GetHumanInput( int param )
	{
		if( param == 1 )
			return m_humanScript.h1Input;
		else if ( param == 2 )
			return m_humanScript.h2Input;
		else if ( param == 3 )
			return m_humanScript.compositeInput;
		else
			return 9999F;
	}

	/**
	 * This function is called by the client to package the human input data for sending.
	 */
	public static InputData PackageInputData()
	{
		byte config = GeneralUtils.GetCurrentConfig ();

		//package the data based on the configuration type
		if (config == Configuration.HvA_1PC) {
			return new InputData (GeneralUtils.HUMAN_ID, GeneralUtils.INPUT_KEYBOARD,
				GeneralUtils.GetHumanInput (1));
		} else if (config == Configuration.HHvA_1PC) {
			return new InputData (GeneralUtils.HUMAN_ID, GeneralUtils.INPUT_KEYBOARD,
				GeneralUtils.GetHumanInput (1));
		} else if (config == Configuration.HHvA_2PC) {
			return new InputData (GeneralUtils.HUMAN_ID, GeneralUtils.INPUT_KEYBOARD,
				(GeneralUtils.ASSIGNED_PONG_CLIENT_ID == GeneralUtils.PONG_CLIENT1_ID)
				? GeneralUtils.GetHumanInput (1) : GeneralUtils.GetHumanInput (2));
		} else if (config == Configuration.HvH_1PC) {
			return new InputData (GeneralUtils.HUMAN_LEFT, GeneralUtils.HUMAN_RIGHT, GeneralUtils.INPUT_KEYBOARD,
				GeneralUtils.GetHumanInput (1), GeneralUtils.GetHumanInput (2));
		} else if (config == Configuration.HvH_2PC) {
			return new InputData (GeneralUtils.HUMAN_LEFT, GeneralUtils.HUMAN_RIGHT, GeneralUtils.INPUT_KEYBOARD,
			    GeneralUtils.GetHumanInput (1), GeneralUtils.GetHumanInput (2));
		} else if (config == Configuration.Rally_1PC) {
			return new InputData (GeneralUtils.HUMAN_LEFT, GeneralUtils.HUMAN_RIGHT, GeneralUtils.INPUT_KEYBOARD,
			    GeneralUtils.GetHumanInput (1), GeneralUtils.GetHumanInput(2));
		} else if (config == Configuration.Rally_2PC) {
			return new InputData (GeneralUtils.HUMAN_LEFT, GeneralUtils.HUMAN_RIGHT, GeneralUtils.INPUT_KEYBOARD,
			    GeneralUtils.GetHumanInput (1), GeneralUtils.GetHumanInput(2));
		} else
			return null;
	}

	/**
	 * This function returns a filename for Throughput analysis based on the configuration
	 * and client id.
	 */
	public static string GetTAFileName()
	{
		return GeneralUtils.ASSIGNED_PONG_CLIENT_ID + "_" + m_menuScript.TAconfigName + ".csv";
	}

	/**
	 * This function returns the time series tuple for the current frame.
	 * formate:"ts,mID,mDiff,mType,Lx,Ly,Rx,Ry,Bx,By,Lvel,Rvel,Bvel,Plen,H1input,H2input,composInput,Lscore,Rscore\n"
	 * 		ts:				timestamp
	 * 		mID:			match ID
	 * 		mDiff:			match difficulty
	 * 		mType:			match type
	 * 		Lx,Ly:			left paddle x and y position
	 * 		Rx,Ry:			right paddle x and y position
	 * 		Bx,By:			ball x and y position
	 * 		Lvel:			left paddle velocity
	 * 		Rvel:			right paddle velocity
	 * 		Bvel:			ball velocity
	 * 		Plen:			paddle length
	 * 		H1input:		human 1 input
	 * 		H2input:		human 2 input (if not applicable, then value is 9999)
	 * 		composInput:	composite input of both human players (if not applicable, then value is 9999)
	 * 		Lscore:			left paddle score
	 * 		Rscore:			right paddle score
	 * 		
	 */
	public static string GetTimeSeriesTuple()
	{
		string t;
		Vector2 garbage = BallUtils.GetBallVelocity (); //TODO: please figure out why removing this breaks everything

		t = GeneralUtils.GetTimeStamp () + ","
						+ GeneralUtils.m_dataScript.currMatchNum.ToString () + ","
						+ GeneralUtils.GetCurrentMatch ().difficulty.ToString () + ","
						+ GeneralUtils.GetCurrentConfig().ToString () + ","
						+ GeneralUtils.GetHumanPosition ().x.ToString () + ","
						+ GeneralUtils.GetHumanPosition ().y.ToString () + ","
						+ GeneralUtils.GetAgentPosition ().x.ToString () + ","
						+ GeneralUtils.GetAgentPosition ().y.ToString () + ","
						+ BallUtils.GetBallPosition ().x.ToString () + ","
						+ BallUtils.GetBallPosition ().y.ToString () + ","
						+ VelocityUtils.GetVelocity("leftPaddle").ToString () + ","
						+ VelocityUtils.GetVelocity("rightPaddle").ToString () + ","
						+ VelocityUtils.GetVelocity("ball").ToString () + ","
						+ GeneralUtils.GetPaddleSize ("Left") + ","
						+ GeneralUtils.GetPaddleSize ("Right") + ","
						+ GeneralUtils.GetHumanInput (1).ToString () + ","
						+ GeneralUtils.GetHumanInput (2).ToString () + ","
						+ GeneralUtils.GetHumanInput (3).ToString () + ","
						+ GeneralUtils.GetPlayerScore ("Left").ToString () + ","
						+ GeneralUtils.GetPlayerScore ("Right").ToString () + "\n";

		return t;
	}

	/**
	 * This function updates the environment to reflect the selected
	 * difficulty level.
	 */
	public static void UpdateEnvironmentDifficulty( byte _diff_ )
	{
		byte currConfig = GeneralUtils.GetCurrentConfig ();

		//if easy
		if( _diff_ == Match.EASY )
		{
			if( !GeneralUtils.IsRally(currConfig) )
				SetPaddleSize(PADDLE_SIZE_LARGE, PADDLE_SIZE_LARGE/PADDLE_SIZE_MEDIUM*PADDLE_WIDTH_MEDIUM);
			BallUtils.SetBallSize(BALL_RADIUS_LARGE);
			m_agentScript.mySkillLevel = AGENT_SKILL_RANDOM;
		}
		//if medium
		else if( _diff_ == Match.MEDIUM )
		{
			if( !GeneralUtils.IsRally(currConfig) )
				SetPaddleSize(PADDLE_SIZE_MEDIUM, PADDLE_WIDTH_MEDIUM);
			BallUtils.SetBallSize(BALL_RADIUS_MEDIUM);
			m_agentScript.mySkillLevel = AGENT_SKILL_NORMAL;
		}
		//if hard
		else if( _diff_ == Match.HARD )
		{
			if( !GeneralUtils.IsRally(currConfig) )
				SetPaddleSize(PADDLE_SIZE_SMALL, PADDLE_SIZE_SMALL/PADDLE_SIZE_MEDIUM*PADDLE_WIDTH_MEDIUM);
			BallUtils.SetBallSize(BALL_RADIUS_SMALL);
			m_agentScript.mySkillLevel = AGENT_SKILL_FAST;
		}
	}

	/**
	 * This function returns the current Match's configuration type.
	 */
	public static byte GetCurrentConfig()
	{
		try{
			return GeneralUtils.GetEnvState().currMatch.configuration.shorthandConfig;
		}catch(Exception e){return GeneralUtils.UNASSIGNED;}
	}

	/**
	 * This function returns the current match object.
	 */
	public static Match GetCurrentMatch()
	{
		try{
			return GeneralUtils.GetEnvState().currMatch;
		}catch(Exception e){return null;}
	}

	/**
	 * This function returns true if the clients are collaborating, otherwise returns false.
	 */
	public static bool IsCollaboration( byte matchType )
	{
		if( matchType == Configuration.HHvA_1PC || matchType == Configuration.HHvA_2PC ||
		    matchType == Configuration.Rally_1PC || matchType == Configuration.Rally_2PC )
			return true;
		else
			return false;
	}

	/**
	 * This function returns true if the a computer is being shared by two people.
	 */
	public static bool SharingComputer( byte matchType )
	{
		if (matchType == Configuration.Rally_1PC ||
						matchType == Configuration.HHvA_1PC ||
						matchType == Configuration.HvH_1PC)
						return true;
				else
						return false;
	}

	/**
	 * This function returns true if the agent is involved in the matches.
	 */
	public static bool AgentPlaying( byte matchType )
	{
		if (matchType == Configuration.HvA_1PC ||
						matchType == Configuration.HHvA_1PC ||
						matchType == Configuration.HHvA_2PC)
						return true;
				else
						return false;
	}

	/**
	 * This function returns true if the match type is Rally (either 1 or 2 PCs).
	 */
	public static bool IsRally( byte matchType )
	{
		if (matchType == Configuration.Rally_1PC ||
						matchType == Configuration.Rally_2PC)
						return true;
				else
						return false;
	}

	/**
	 * This function updates both paddles' size with the specified value.
	 */
	private static void SetPaddleSize( float _newLen_, float _newWidth_ )
	{
		Vector3 newScale = m_humanScript.transform.localScale;
		newScale.y = _newLen_;
		newScale.x = _newWidth_;

		//update both the agent and the human player paddle size
		m_agentScript.transform.localScale = newScale;
		m_humanScript.transform.localScale = newScale;
	}

	/**
	 * This function updates the specified paddle's size with the new specified value (Rally mode only).
	 */
	public static void SetPaddleSize( string _paddleID_, float _newLen_, float _newWidth_ )
	{
		//if left paddle
		if( _paddleID_ == "leftPaddle" )
		{
			Vector3 newScale = m_humanScript.transform.localScale;
			newScale.y = _newLen_;
			newScale.x = _newWidth_;
			m_humanScript.transform.localScale = newScale;
		}
		//if right paddle
		else if( _paddleID_ == "rightPaddle" )
		{
			Vector3 newScale = m_agentScript.transform.localScale;
			newScale.y = _newLen_;
			newScale.x = _newWidth_;
			m_agentScript.transform.localScale = newScale;
		}
	}

	/**
	 * This function returns the current paddle size.
	 */
	public static float GetPaddleSize( string paddleID )
	{
		Bounds bounds = paddleID == "Left"
			? m_humanScript.myRenderer.bounds : m_agentScript.myRenderer.bounds;
		Vector3 v = bounds.max;
		Vector3 c = bounds.center;
		return 2F * Vector3.Distance (v, c);
	}
}

public class Automaton {
	/* Member Data */
	private int m_currState;
	private int m_prevState;
	
	/**
     * Constructor.
     */
	public Automaton() {
		m_currState = 0;
		m_prevState = 0;
	}
	
	/**
     * This method enacts a transition from CurrState to NewState.
     */
	public void Transition(int NewState) {
		m_prevState = m_currState;
		m_currState = NewState;
	}
	
	/**
     * Accessors.
     */
	public int CurrState
	{ get { return m_currState; } }
	public int PrevState
	{ get { return m_prevState; } }
}