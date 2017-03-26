using System;
using UnityEngine;
using System.Collections;
using System.IO;
using Newtonsoft.Json;

public class SessionBehavior : MonoBehaviour
{
	[HideInInspector]
	public SessionAutomaton sessionAuto;
	[HideInInspector]
	public int LeftScore;
	[HideInInspector]
	public int RightScore;
	[HideInInspector]
	public bool SessionBegin;
	[HideInInspector]
	public bool BallOffScreen;
	[HideInInspector]
	public bool RightPoint;
	[HideInInspector]
	public bool LeftPoint;
	private DateTime sessionBeginTime;
	private DateTime matchBeginTime;
	[HideInInspector]
	public Session session;
	[HideInInspector]
	public Match currMatch;
	private int CurrMatchIndex;
	private DateTime startOfCountdown;
	private DateTime startOfMatchFeedback;
	private DateTime startOfSessionFeedback;
	private Vector3 initPlayerPosition;
	private int myCurrentScore;
	private int myPreviousScore;
	private int opCurrentScore;
	private int opPreviousScore;
	private string mySideOfBoard; //either "Left" or "Right"
	private bool firstMatchRecvd;
	private Configuration config;
	private string matchFeedbackString;
	private AudioClip matchFeedbackAudio;
	private Texture feedbackImg;

	void Start ()
	{
		sessionAuto = new SessionAutomaton ();
		LeftScore = 0;
		RightScore = 0;
		SessionBegin = false;
		BallOffScreen = false;
		RightPoint = false;
		LeftPoint = false;
		session = null;
		currMatch = null;
		CurrMatchIndex = 0;
		myCurrentScore = 0;
		myPreviousScore = 0;
		opCurrentScore = 0;
		opPreviousScore = 0;
		initPlayerPosition = GeneralUtils.GetHumanPosition ();
		firstMatchRecvd = false;
		mySideOfBoard = "NULL";
	}

	void Update ()
	{
		/** Session Automaton Behavior **/
		//ROLE: SERVER
		if( GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_SERVER )
		{
			//CURRENT STATE: SESSION NOT YET IN PROGRESS
			if( sessionAuto.CurrState == SessionAutomaton.SESSION_NOT_IN_PROGRESS )
			{
				//if the SessionBegin event arrives
				if( SessionBegin )
				{
					//reset flag
					SessionBegin = false;

					//announce that the session has begun
					EventUtils.LogEvent( _Event.SESSION_BEGIN );

					//capture the current time
					sessionBeginTime = DateTime.Now;

					//load the session
					string sessionFileContent = GeneralUtils.ReadContentFromFile(GeneralUtils.GetSelectedSessionPath());
					session = new Session (sessionFileContent);
					
					//start the Session's SessionTimer
					session.m_sessTimer.StartTimer();

					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.LOAD_MATCH );
				}
			}
			//CURRENT STATE: LOAD THE MATCH TO PLAY
			else if( sessionAuto.CurrState == SessionAutomaton.LOAD_MATCH )
			{
				//reset player position
				GeneralUtils.ResetHumanPosition( initPlayerPosition );

				//if no more matches remain
				if( session.m_sessTimer.SessionComplete() )
				{
					//capture the current time
					startOfSessionFeedback = DateTime.Now;

					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.GIVING_CLOSING_FEEDBACK );
				}
				//otherwise
				else
				{
					//load the Match
					currMatch = new Match( session.m_config, session.GetDifficulty() );

					//setup environment for Match
					GeneralUtils.UpdateEnvironmentDifficulty( currMatch.difficulty );

					//increment match index for next loop
					CurrMatchIndex++;

					//capture the current time
					startOfCountdown = DateTime.Now;

					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.COUNTDOWN_TO_START );
				}
			}
			//CURRENT STATE: COUNTING DOWN TO START OF MATCH
			else if( sessionAuto.CurrState == SessionAutomaton.COUNTDOWN_TO_START )
			{
				//if the countdown is complete (3.5 seconds)
				if( DateTime.Now.Subtract(startOfCountdown).TotalMilliseconds >= 3500 )
				{
					//capture the current time
					matchBeginTime = DateTime.Now;

					//announce that the match has begun
					EventUtils.LogEvent( _Event.MATCH_BEGIN );

					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.IN_PLAY );
				}
			}
			//CURRENT STATE: MATCH IN PLAY
			else if( sessionAuto.CurrState == SessionAutomaton.IN_PLAY )
			{
				//if either the human or agent scores, then the Match ends
				if( BallOffScreen )
				{
					//reset flag
					BallOffScreen = false;

					//capture the current time
					startOfMatchFeedback = DateTime.Now;

					//announce that the match has ended
					EventUtils.LogEvent( _Event.MATCH_END );

					//announce that feedback has begun
					EventUtils.LogEvent( _Event.FEEDBACK_BEGIN );

					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.GIVING_MATCH_FEEDBACK );
				}
				//if the match has gone on too long, call it a DRAW
				else if( DateTime.Now.Subtract( matchBeginTime ).TotalMilliseconds > GeneralUtils.DRAW_TIMEOUT )
				{
					//capture the current time
					startOfMatchFeedback = DateTime.Now;

					//announce that the match has ended
					EventUtils.LogEvent( _Event.MATCH_END );

					//announce that the match was a draw
					EventUtils.AnnounceDraw();

					//announce that feedback has begun
					EventUtils.LogEvent( _Event.FEEDBACK_BEGIN );

					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.GIVING_MATCH_FEEDBACK );
				}
			}
			//CURRENT STATE: GIVING FEEDBACK ABOUT MATCH
			else if( sessionAuto.CurrState == SessionAutomaton.GIVING_MATCH_FEEDBACK )
			{
				//give feedback based on the Match outcome for n-seconds
				if( DateTime.Now.Subtract(startOfMatchFeedback).TotalMilliseconds > 5000 )
				{
					//announce that feedback has ended
					EventUtils.LogEvent( _Event.FEEDBACK_END );

					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.LOAD_MATCH );
				}
			}
			//CURRENT STATE: GIVING CLOSING FEEDBACK ABOUT SESSION
			else if( sessionAuto.CurrState == SessionAutomaton.GIVING_CLOSING_FEEDBACK )
			{
				//give feedback based on Session outcome for n-seconds
				if( DateTime.Now.Subtract(startOfSessionFeedback).TotalMilliseconds > 5000 )
				{
					//announce that feedback has ended
					EventUtils.LogEvent( _Event.FEEDBACK_END );

					//announce end of session
					EventUtils.AnnounceSessionEnd();
					EventUtils.LogEvent( _Event.SESSION_END );

					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.SESSION_NOT_IN_PROGRESS );
				}
			}
		}
		//ROLE: CLIENT
		else
		{
			EnvState currEnvState = GeneralUtils.GetServerEnvState();

			//CURRENT STATE: SESSION NOT YET IN PROGRESS
			if( sessionAuto.CurrState == SessionAutomaton.SESSION_NOT_IN_PROGRESS )
			{
				//if the session state has changed
				if( currEnvState.sessionState != SessionAutomaton.SESSION_NOT_IN_PROGRESS )
				{
					//capture the current time
					sessionBeginTime = DateTime.Now;
					
					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.LOAD_MATCH );
				}
			}
			//CURRENT STATE: LOAD THE MATCH TO PLAY
			else if( sessionAuto.CurrState == SessionAutomaton.LOAD_MATCH )
			{
				//if the session has ended
				if( currEnvState.sessionState == SessionAutomaton.GIVING_CLOSING_FEEDBACK )
				{
					//capture the current time
					startOfSessionFeedback = DateTime.Now;
					
					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.GIVING_CLOSING_FEEDBACK );
				}
				//if the session state has changed
				else if( currEnvState.sessionState == SessionAutomaton.COUNTDOWN_TO_START )
				{
					//load the Match
					currMatch = currEnvState.currMatch;
					if(firstMatchRecvd==false)
					{
						//store the configuration
						config = currMatch.configuration;
						//Debug.Log ( "Match configuration is " + config.shorthandConfig.ToString() );

						//store my side of the board
						StoreMySide();

						//reset flag
						firstMatchRecvd = true;
					}
					
					//setup environment for Match
					GeneralUtils.UpdateEnvironmentDifficulty( currMatch.difficulty );
					
					//increment match index for next loop
					CurrMatchIndex++;
					
					//capture the current time
					startOfCountdown = DateTime.Now;
					
					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.COUNTDOWN_TO_START );
				}
			}
			//CURRENT STATE: COUNTING DOWN TO START OF MATCH
			else if( sessionAuto.CurrState == SessionAutomaton.COUNTDOWN_TO_START )
			{
				//if the countdown is complete
				if( currEnvState.sessionState == SessionAutomaton.IN_PLAY )
				{
					//capture the current time
					matchBeginTime = DateTime.Now;
					
					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.IN_PLAY );
				}
			}
			//CURRENT STATE: MATCH IN PLAY
			else if( sessionAuto.CurrState == SessionAutomaton.IN_PLAY )
			{
				//if the Match ends
				if( currEnvState.sessionState == SessionAutomaton.GIVING_MATCH_FEEDBACK )
				{
					//update the local score variables
					myPreviousScore = myCurrentScore;
					myCurrentScore = GeneralUtils.GetPlayerScore( mySideOfBoard );
					opPreviousScore = opCurrentScore;
					opCurrentScore = GeneralUtils.GetPlayerScore ( mySideOfBoard == "Left" ? "Right" : "Left" );

					//store the feedback to give in the next state
					int randomIndex = FeedbackUtils.GetRandFeedbackIndex();
					string outcome = myCurrentScore > myPreviousScore ? "S" : "F";
					if(myCurrentScore == myPreviousScore && opCurrentScore == opPreviousScore)
						outcome = "D";
					matchFeedbackString = FeedbackUtils.GetFeedbackString(outcome, randomIndex);
					matchFeedbackAudio = FeedbackUtils.GetFeedbackAudio(outcome, randomIndex);
					feedbackImg = FeedbackUtils.GetFeedbackImage(outcome);

					//play the audio feedback
					if(GetComponent<AudioSource>() != null)
						GetComponent<AudioSource>().PlayOneShot(matchFeedbackAudio);

					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.GIVING_MATCH_FEEDBACK );
				}
			}
			//CURRENT STATE: GIVING FEEDBACK ABOUT MATCH
			else if( sessionAuto.CurrState == SessionAutomaton.GIVING_MATCH_FEEDBACK )
			{
				//if feedback display is done
				if( currEnvState.sessionState == SessionAutomaton.LOAD_MATCH )
				{
					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.LOAD_MATCH );
				}
				//it is possible that a TCP packet was lost, so check if server is now in countdown state
				else if( currEnvState.sessionState == SessionAutomaton.COUNTDOWN_TO_START )
				{
					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.LOAD_MATCH );
				}
				//if the session has ended, begin giving closing feedback
				else if( currEnvState.sessionState == SessionAutomaton.SESSION_NOT_IN_PROGRESS )
				{
					//enact transition to the next state
					sessionAuto.Transition( SessionAutomaton.GIVING_CLOSING_FEEDBACK );
				}
			}
			//CURRENT STATE: GIVING CLOSING FEEDBACK ABOUT SESSION
			else if( sessionAuto.CurrState == SessionAutomaton.GIVING_CLOSING_FEEDBACK )
			{

			}
		}
	}

	/**
	 * Clients only: This function stores which side of the board I am playing on.
	 */
	private void StoreMySide()
	{
		//if only one human player in config, then I'm the left paddle
		if( config.shorthandConfig == Configuration.HvA_1PC )
			mySideOfBoard = "Left";
		//if HvH or Rally, regardless of PC#
		else if( config.shorthandConfig == Configuration.HvH_1PC || config.shorthandConfig == Configuration.HvH_2PC ||
		         config.shorthandConfig == Configuration.Rally_1PC || config.shorthandConfig == Configuration.Rally_2PC )
		{
			//if client 1, then I'm left
			if( GeneralUtils.ASSIGNED_PONG_CLIENT_ID == GeneralUtils.PONG_CLIENT1_ID )
				mySideOfBoard = "Left";
			//if client 2, then I'm right
			else if( GeneralUtils.ASSIGNED_PONG_CLIENT_ID == GeneralUtils.PONG_CLIENT2_ID )
				mySideOfBoard = "Right";
		}
		//if HHvA, regardless of PC#, then I'm left
		else if( config.shorthandConfig == Configuration.HHvA_1PC || config.shorthandConfig == Configuration.HHvA_2PC )
			mySideOfBoard = "Left";
	}

	/**
	 * Clients only: This function returns this client's score.
	 * If the server process calls this function, then it returns -1.
	 */
	private int GetMyScore()
	{
		//if process is not a client, do nothing
		if( GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_SERVER )
			return -1;
		//otherwise
		else
			return GeneralUtils.GetPlayerScore(mySideOfBoard);
	}


	/**
	 * GUI for displaying information during Sessions such as score, states, and feedback.
	 */
	void OnGUI()
	{
		//client display score
		if( sessionAuto.CurrState != SessionAutomaton.SESSION_NOT_IN_PROGRESS && GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_CLIENT )
		{
			//draw the participant's scores based on the current configuration type
			int i;
			byte config = GeneralUtils.GetCurrentConfig();
			if( GeneralUtils.IsRally(config) )
			{
				//update the score appropriately
				_GUI_.rallyScoreRect.UpdateScore( GeneralUtils.GetRallyScore() );

				//iterate over each sub-Rect
				for( i = 0; i < ScoreRect.NUM_SUBRECTS-2; i++ )
					GUI.DrawTexture(_GUI_.rallyScoreRect.GetSubRect(i), _GUI_.rallyScoreRect.GetSubRectImage(i));
				GUI.Label (_GUI_.rallyScoreRect.GetSubRect(ScoreRect.NUM_SUBRECTS-2),
				           _GUI_.rallyScoreRect.FormatString("X"));
				GUI.Label (_GUI_.rallyScoreRect.GetSubRect(ScoreRect.NUM_SUBRECTS-1),
				         _GUI_.rallyScoreRect.GetScoreFormatted());
			}
			else
			{
				//update the scores appropriately
				_GUI_.leftScoreRect.UpdateScore( GeneralUtils.GetPlayerScore("Left") );
				_GUI_.rightScoreRect.UpdateScore( GeneralUtils.GetPlayerScore("Right") );

				//iterate over each sub-Rect for left paddle
				for( i = 0; i < ScoreRect.NUM_SUBRECTS-2; i++ )
					GUI.DrawTexture(_GUI_.leftScoreRect.GetSubRect(i), _GUI_.leftScoreRect.GetSubRectImage(i));
				GUI.Label (_GUI_.leftScoreRect.GetSubRect(ScoreRect.NUM_SUBRECTS-2),
				           _GUI_.leftScoreRect.FormatString("X"));
				GUI.Label (_GUI_.leftScoreRect.GetSubRect(ScoreRect.NUM_SUBRECTS-1),
				         _GUI_.leftScoreRect.GetScoreFormatted());
				//iterate over each sub-Rect for right paddle
				for( i = 0; i < ScoreRect.NUM_SUBRECTS-2; i++ )
					GUI.DrawTexture(_GUI_.rightScoreRect.GetSubRect(i), _GUI_.rightScoreRect.GetSubRectImage(i));
				GUI.Label (_GUI_.rightScoreRect.GetSubRect(ScoreRect.NUM_SUBRECTS-2),
				           _GUI_.rightScoreRect.FormatString("X"));
				GUI.Label (_GUI_.rightScoreRect.GetSubRect(ScoreRect.NUM_SUBRECTS-1),
				         _GUI_.rightScoreRect.GetScoreFormatted());
			}


			//GUI.Label( _GUI_.leftScoreRect, _GUI_.FormatScoreText(LeftScore.ToString()) );
			//GUI.Label( _GUI_.rightScoreRect, _GUI_.FormatScoreText(RightScore.ToString()) );
		}

		/** Session Automaton Behavior **/
		//ROLE: SERVER
		if( GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_SERVER )
		{
//			//CURRENT STATE: SESSION NOT YET IN PROGRESS
//			if( sessionAuto.CurrState == SessionAutomaton.SESSION_NOT_IN_PROGRESS ) {}
//			//CURRENT STATE: LOAD THE MATCH TO PLAY
//			else if( sessionAuto.CurrState == SessionAutomaton.LOAD_MATCH ) {}
//			//CURRENT STATE: COUNTING DOWN TO START OF MATCH
//			else if( sessionAuto.CurrState == SessionAutomaton.COUNTDOWN_TO_START ) {}
//			//CURRENT STATE: MATCH IN PLAY
//			else if( sessionAuto.CurrState == SessionAutomaton.IN_PLAY )
//			{
//				//if this process is the server
//				if( GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_SERVER )
//				{
//					//debug GUI
//					string debugStr = "[ballVel=" + VelocityUtils.GetVelocity("ball").ToString("F3") + " cm/s]\n[leftVel="
//						+ VelocityUtils.GetVelocity("leftPaddle").ToString("F3") + " cm/s]\n[rightvel="
//						+ VelocityUtils.GetVelocity("rightPaddle").ToString("F3") + " cm/s]";
//					GUI.Label ( _GUI_.ClientDebugRect, _GUI_.FormatClientDebugText( debugStr ) );
//				}
//			}
//			//CURRENT STATE: GIVING FEEDBACK ABOUT MATCH
//			else if( sessionAuto.CurrState == SessionAutomaton.GIVING_MATCH_FEEDBACK ){}
//			//CURRENT STATE: GIVING CLOSING FEEDBACK ABOUT SESSION
//			else if( sessionAuto.CurrState == SessionAutomaton.GIVING_CLOSING_FEEDBACK ){}
		}
		//ROLE: CLIENT
		else
		{
			//CURRENT STATE: SESSION NOT YET IN PROGRESS
			if( sessionAuto.CurrState == SessionAutomaton.SESSION_NOT_IN_PROGRESS ) {}
			//CURRENT STATE: LOAD THE MATCH TO PLAY
			else if( sessionAuto.CurrState == SessionAutomaton.LOAD_MATCH ) {}
			//CURRENT STATE: COUNTING DOWN TO START OF MATCH
			else if( sessionAuto.CurrState == SessionAutomaton.COUNTDOWN_TO_START )
			{
				int remainMS = (int)(DateTime.Now.Subtract(startOfCountdown).TotalMilliseconds);
				string remaining = (Math.Ceiling( (double)((3000F-remainMS)/1000F) )).ToString();
				GUI.Label ( _GUI_.CountDownRect, "<size=700>"+remaining+"</size>" );
			}
			//CURRENT STATE: MATCH IN PLAY
			else if( sessionAuto.CurrState == SessionAutomaton.IN_PLAY ) {}
			//CURRENT STATE: GIVING FEEDBACK ABOUT MATCH
			else if( sessionAuto.CurrState == SessionAutomaton.GIVING_MATCH_FEEDBACK )
			{
				string feedback = matchFeedbackString;
				GUI.DrawTexture (_GUI_.fullscreenRect, feedbackImg );
				GUI.Label ( _GUI_.FeedbackRect, _GUI_.FormatFeedbackText( feedback, "white" ) );
			}
			//CURRENT STATE: GIVING CLOSING FEEDBACK ABOUT SESSION
			else if( sessionAuto.CurrState == SessionAutomaton.GIVING_CLOSING_FEEDBACK )
			{
				//"You won! Great Job!!" =>positive outcome
				//"Almost. Better luck next time!" => otherwise
				string feedback = "Thanks for playing!";
				GUI.DrawTexture (_GUI_.fullscreenRect, FeedbackUtils.posFeedbackImg );
				GUI.Label ( _GUI_.FeedbackRect, _GUI_.FormatFeedbackText( feedback, "white" ) );
			}
		}
	}

	/**
	 * This accessor returns true if a Match is in progress. Otherwise, returns false.
	 */
	public bool matchInProgress
	{
		get {
			if(sessionAuto==null)
				return false;
			else
				return sessionAuto.CurrState == SessionAutomaton.IN_PLAY;
		}
	}

	/**
	 * This accessor returns the elapsed time since the current Session began.
	 * The value returned is in milliseconds.
	 */
	public int timeSinceSessionBegan
	{
		get { return (int)DateTime.Now.Subtract(sessionBeginTime).TotalMilliseconds; }
	}

	/**
	 * This accessor returns the elapsed time since the current Match began.
	 * The value returned is in milliseconds.
	 */
	public int timeSinceMatchBegan
	{
		get { return (int)DateTime.Now.Subtract(matchBeginTime).TotalMilliseconds; }
	}
}

public class SessionAutomaton : Automaton
{
	/* State Constants */
	public const int SESSION_NOT_IN_PROGRESS = 0;
	public const int LOAD_MATCH = 1;
	public const int COUNTDOWN_TO_START = 2;
	public const int IN_PLAY = 3;
	public const int GIVING_MATCH_FEEDBACK = 4;
	public const int GIVING_CLOSING_FEEDBACK = 5;

	/**
	 * Constructor.
	 */
	public SessionAutomaton() : base ()
	{
		Transition (SESSION_NOT_IN_PROGRESS);
	}

	/**
	 * This method returns the current state as a string.
	 */
	public string StateString()
	{
		switch(this.CurrState)
		{
		case SESSION_NOT_IN_PROGRESS:
			return "SESSION_NOT_IN_PROGRESS";
		case LOAD_MATCH:
			return "LOAD_MATCH";
		case COUNTDOWN_TO_START:
			return "COUNTDOWN_TO_START";
		case IN_PLAY:
			return "IN_PLAY";
		case GIVING_MATCH_FEEDBACK:
			return "GIVING_MATCH_FEEDBACK";
		case GIVING_CLOSING_FEEDBACK:
			return "GIVING_CLOSING_FEEDBACK";
		default:
			return "UNKNOWN_STATE";
		}
	}

	/**
	 * This method returns the supplied state as a string.
	 */
	public static string StateString( int state )
	{
		switch(state)
		{
		case SESSION_NOT_IN_PROGRESS:
			return "SESSION_NOT_IN_PROGRESS";
		case LOAD_MATCH:
			return "LOAD_MATCH";
		case COUNTDOWN_TO_START:
			return "COUNTDOWN_TO_START";
		case IN_PLAY:
			return "IN_PLAY";
		case GIVING_MATCH_FEEDBACK:
			return "GIVING_MATCH_FEEDBACK";
		case GIVING_CLOSING_FEEDBACK:
			return "GIVING_CLOSING_FEEDBACK";
		default:
			return "UNKNOWN_STATE";
		}
	}
}