using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class DataManger : MonoBehaviour
{
	private bool roleIsServer;
	private EventLog eventLog;
	private string eventOutputFilePath;
	private string eventOutputFileName;
	private string timeSeriesOutputFilePath;
	private string timeSeriesOutputFileName;
	private string metadataOutputFilePath;
	private string metadataOutputFileName;
	private TimeSeriesLogAutomaton timeSeriesAuto;
	private MetadataLogAutomaton metadataAuto;
	private DateTime lastWriteTime;
	private StringBuilder sb;
	[HideInInspector]
	public int currMatchNum;
	[HideInInspector]
	public Metadata metadata;

	void Start ()
	{
		eventLog = new EventLog ();
		eventOutputFilePath = Application.dataPath + "/OutputData/Events/";
		timeSeriesOutputFilePath = Application.dataPath + "/OutputData/Time Series/";
		metadataOutputFilePath = Application.dataPath + "/OutputData/Metadata/";
		eventOutputFileName = "NONAME.json";
		timeSeriesOutputFileName = "NONAME.csv";
		metadataOutputFileName = "NONAME.json";
		timeSeriesAuto = new TimeSeriesLogAutomaton ();
		metadataAuto = new MetadataLogAutomaton ();
		lastWriteTime = DateTime.Now;
		sb = new StringBuilder ();
		currMatchNum = 0;
		metadata = new Metadata ();
	}

	void Update ()
	{
		//only log data if role is server
		if( GeneralUtils.GetProcessRole () == GeneralUtils.ROLE_SERVER )
		{
			/** TIME SERIES LOGGER AUTOMATON **/
			//CURRENT STATE: SESSION NOT YET STARTED
			if( timeSeriesAuto.CurrState == TimeSeriesLogAutomaton.SESSION_NOT_STARTED )
			{
				//if a session begins
				if( GeneralUtils.SessionInProgress() )
				{
					//set the filenames to be produced
					string mainFileName = "";
					if( GeneralUtils.GetNumHumanPlayers() == 2 )
						mainFileName = GeneralUtils.GetHuman1Name() + "__" + GeneralUtils.GetHuman2Name();
					else
						mainFileName = GeneralUtils.GetHuman1Name();
					mainFileName += "__" + GeneralUtils.GetSelectedSessionName();
					eventOutputFileName = eventOutputFileName.Replace("NONAME",mainFileName);
					timeSeriesOutputFileName = timeSeriesOutputFileName.Replace("NONAME",mainFileName);
					metadataOutputFileName = metadataOutputFileName.Replace("NONAME",mainFileName);

					//enact transition to the next state
					timeSeriesAuto.Transition( TimeSeriesLogAutomaton.MATCH_NOT_STARTED );
				}
			}
			//CURRENT STATE: MATCH NOT YET STARTED
			else if( timeSeriesAuto.CurrState == TimeSeriesLogAutomaton.MATCH_NOT_STARTED )
			{
				//if a match begins
				if( GeneralUtils.MatchInProgress() )
				{
					//clear the string builder object
					sb = new StringBuilder();

					//append the file header
					sb.Append( "ts,mNum,mDiff,mCfg,Lx,Ly,Rx,Ry,Bx,By,Lvel,Rvel,Bvel,Llen,RLen,H1input,H2input,composInput,Lscore,Rscore\n" );

					//enact transition to the next state
					timeSeriesAuto.Transition( TimeSeriesLogAutomaton.ACCUMULATING_SAMPLES );
				}
				
				//if the session is completed
				if( !GeneralUtils.SessionInProgress() )
				{
					//enact transition to the next state
					timeSeriesAuto.Transition( TimeSeriesLogAutomaton.SESSION_COMPLETED );
				}
			}
			//CURRENT STATE: ACCUMULATING SAMPLES
			else if( timeSeriesAuto.CurrState == TimeSeriesLogAutomaton.ACCUMULATING_SAMPLES )
			{
				
				//if the session is completed
				if( !GeneralUtils.SessionInProgress() )
				{
					//log the accumulated samples
					StreamWriter sw = new StreamWriter( timeSeriesOutputFilePath+timeSeriesOutputFileName, true );
					sw.Write (sb.ToString());
					sw.Close();

					//enact transition to the next state
					timeSeriesAuto.Transition( TimeSeriesLogAutomaton.SESSION_COMPLETED );
				}
				//otherwise
				else
				{
					//if the match ends
					if( !GeneralUtils.MatchInProgress() )
					{
						//log the accumulated samples
						StreamWriter sw = new StreamWriter( timeSeriesOutputFilePath+timeSeriesOutputFileName, true );
						sw.Write (sb.ToString());
						sw.Close();

						//increment the match count
						currMatchNum += 1;

						//enact transition to the next state
						timeSeriesAuto.Transition( TimeSeriesLogAutomaton.MATCH_NOT_STARTED );
					}
					//otherwise
					else
					{
						//if five seconds have passed
						if( DateTime.Now.Subtract( lastWriteTime ).TotalMilliseconds > 5000 )
						{
							//log the accumulated samples
							StreamWriter sw = new StreamWriter( timeSeriesOutputFilePath+timeSeriesOutputFileName, true );
							sw.Write (sb.ToString());
							sw.Close();

							//capture the current time
							lastWriteTime = DateTime.Now;

							//clear the string builder
							sb = new StringBuilder();
						}
						//otherwise, log the sample
						else
						{
							//store samples for moving objects in order to track velocity
							VelocityUtils.UpdateObjPositionSamples();

							//append the sample to the string builder
							sb.Append( GeneralUtils.GetTimeSeriesTuple() );
						}
					}
				}
			}
			//CURRENT STATE: SESSION COMPLETED
			else if( timeSeriesAuto.CurrState == TimeSeriesLogAutomaton.SESSION_COMPLETED )
			{
			}

			
			/** METADATA LOGGER AUTOMATON **/
			//CURRENT STATE: SESSION INFO HAS NOT YET BEEN ENTERED
			if( metadataAuto.CurrState == MetadataLogAutomaton.NOT_ENTERED_SESSION_INFO )
			{
				//if the session information begins to be entered
				if( GeneralUtils.GettingSessionInfoFromUser() )
				{
					//enact transition to the next state
					metadataAuto.Transition( MetadataLogAutomaton.GETTING_SESSION_INFO );
				}
			}
			//CURRENT STATE: GETTING SESSION INFO
			else if( metadataAuto.CurrState == MetadataLogAutomaton.GETTING_SESSION_INFO )
			{
				Match currMatch = GeneralUtils.GetCurrentMatch();

				//if session info has been successfully entered and the session begins
				if( GeneralUtils.SessionInProgress() && currMatch != null )
				{
					//store the entered information in the metadata object
					metadata.date = DateTime.Now.Date.ToString();
					metadata.p1ID = GeneralUtils.GetParticipantID(1);
					metadata.p2ID = GeneralUtils.GetParticipantID(2);
					metadata.sessionID = GeneralUtils.GetSessionID();
					metadata.startOfSession = GeneralUtils.GetTimeStamp();
					metadata.hrsGamesPerWeekP1 = GeneralUtils.GetHrsGamesPerWeek(1);
					metadata.hrsGamesPerWeekP2 = GeneralUtils.GetHrsGamesPerWeek(2);
					metadata.collaborative = GeneralUtils.IsCollaboration( currMatch.configuration.shorthandConfig );
					metadata.therapistPlaying = currMatch.configuration.therapistPlaying;
					metadata.interactionType = currMatch.configuration.shorthandConfig;

					//enact transition to the next state
					metadataAuto.Transition( MetadataLogAutomaton.SESSION_INFO_ENTERED );
				}
			}
			//CURRENT STATE: FINISHED GETTING SESSION INFO
			else if( metadataAuto.CurrState == MetadataLogAutomaton.SESSION_INFO_ENTERED )
			{
				//if the session ends
				if( !GeneralUtils.SessionInProgress() )
				{
					//log metadata to a file
					metadata.endOfSession = GeneralUtils.GetTimeStamp();
					metadata.Save(metadataOutputFilePath+metadataOutputFileName);
					
					//enact transition to the next state
					metadataAuto.Transition( MetadataLogAutomaton.SESSION_COMPLETE );
				}
			}
			//CURRENT STATE: SESSION COMPLETE
			else if( metadataAuto.CurrState == MetadataLogAutomaton.SESSION_COMPLETE )
			{
			}

		}
	}

	public void GenEvent( byte eventType, string _args_="" )
	{
		if( eventType == _Event.BALL_HIT )
			GenBallHitEvent(_args_);
		else if( eventType == _Event.BALL_LAUNCH )
			GenBallLaunchEvent();
		else if( eventType == _Event.FEEDBACK_BEGIN )
			GenFeedbackBeginEvent();
		else if( eventType == _Event.FEEDBACK_END )
			GenFeedbackEndEvent();
		else if( eventType == _Event.MATCH_BEGIN )
			GenMatchBeginEvent();
		else if( eventType == _Event.MATCH_END )
			GenMatchEndEvent();
		else if( eventType == _Event.SESSION_BEGIN )
			GenSessionBeginEvent();
		else if( eventType == _Event.SESSION_END )
			GenSessionEndEvent();
	}

	private void GenBallHitEvent(string _paddleID_)
	{
		BallHitEvent e = new BallHitEvent (_paddleID_);
		eventLog.AddEvent (e.type, JsonConvert.SerializeObject (e, Formatting.Indented, GeneralUtils.jss));
	}

	private void GenBallLaunchEvent()
	{
		BallLaunchEvent e = new BallLaunchEvent ();
		eventLog.AddEvent (e.type, JsonConvert.SerializeObject (e, Formatting.Indented, GeneralUtils.jss));
	}

	private void GenMatchBeginEvent()
	{
		MatchBeginEvent e = new MatchBeginEvent ();
		eventLog.AddEvent (e.type, JsonConvert.SerializeObject (e, Formatting.Indented, GeneralUtils.jss));
	}

	private void GenMatchEndEvent()
	{
		MatchEndEvent e = new MatchEndEvent ();
		eventLog.AddEvent (e.type, JsonConvert.SerializeObject (e, Formatting.Indented, GeneralUtils.jss));
	}

	private void GenFeedbackBeginEvent()
	{
		FeedbackBeginEvent e = new FeedbackBeginEvent ();
		eventLog.AddEvent (e.type, JsonConvert.SerializeObject (e, Formatting.Indented, GeneralUtils.jss));
	}

	private void GenFeedbackEndEvent()
	{
		FeedbackEndEvent e = new FeedbackEndEvent ();
		eventLog.AddEvent (e.type, JsonConvert.SerializeObject (e, Formatting.Indented, GeneralUtils.jss));
	}
	
	private void GenSessionBeginEvent()
	{
		SessionBeginEvent e = new SessionBeginEvent ();
		eventLog.AddEvent (e.type, JsonConvert.SerializeObject (e, Formatting.Indented, GeneralUtils.jss));
	}
	
	private void GenSessionEndEvent()
	{
		SessionEndEvent e = new SessionEndEvent ();
		eventLog.AddEvent (e.type, JsonConvert.SerializeObject (e, Formatting.Indented, GeneralUtils.jss));
		eventLog.Save (eventOutputFilePath+eventOutputFileName);
		eventLog.Clear ();
	}
}

public class TimeSeriesLogAutomaton : Automaton
{
	public const int SESSION_NOT_STARTED = 0;
	public const int MATCH_NOT_STARTED = 1;
	public const int ACCUMULATING_SAMPLES = 2;
	public const int SESSION_COMPLETED = 3;

	public TimeSeriesLogAutomaton() : base()
	{
		this.Transition (SESSION_NOT_STARTED);
	}
}

public class MetadataLogAutomaton : Automaton
{
	public const int NOT_ENTERED_SESSION_INFO=0;
	public const int GETTING_SESSION_INFO=1;
	public const int SESSION_INFO_ENTERED=2;
	public const int SESSION_COMPLETE = 3;

	public MetadataLogAutomaton() :base()
	{
		this.Transition (NOT_ENTERED_SESSION_INFO);
	}
}