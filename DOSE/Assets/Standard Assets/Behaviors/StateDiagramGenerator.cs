using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class StateDiagramGenerator : MonoBehaviour
{
	/* SDG Behavior Variables */
	private SDGAutomaton sdgAuto;
	private KeyCode beginLog;
	private KeyCode stopLog;

	/* Script Connections */
	private Main mainComm;
	private SessionBehavior sessionComm;
	private AgentInput agentComm;

	/* Component Cache */
	private Transform agentTrans;

	/* State Variables */
	private int mainAutoState;
	private int sessionAutoState;
	private int agentAutoState;
	private float agentYComp;

	/* File Output Variables */
	private string outputFile;
	private string fileHeader;
	private StreamWriter sw;
	private StringBuilder sb;

	void Awake ()
	{
		//establish script connections
		mainComm = GameObject.Find("ScriptHub").GetComponent<Main>();
		sessionComm = GameObject.Find("ScriptHub").GetComponent<SessionBehavior>();
		agentComm = GameObject.Find("Agent").GetComponent<AgentInput>();

		//cache component lookup
		agentTrans = agentComm.transform;
	}

	void Start ()
	{
		//init variables
		outputFile = "pong_StateDiagramOutput.csv";
		fileHeader = "M, S, As, Ay\n";
		sb = new StringBuilder ();
		sb.Append (fileHeader);
		sdgAuto = new SDGAutomaton ();
		beginLog = KeyCode.Q;
		stopLog = KeyCode.W;
	}
	
	void Update ()
	{
		//CURRENT STATE: NET YET STARTED LOGGING
		if( sdgAuto.CurrState == SDGAutomaton.NOT_YET_STARTED_LOGGING )
		{
			//if user begins logging by pressing key
			if( Input.GetKeyDown(beginLog) )
			{
				Debug.Log( "Beginning to log state tuples <----" );

				//enact transition to the next state
				sdgAuto.Transition( SDGAutomaton.LOGGING_IN_PROGRESS );
			}
		}
		//LOGGING IN PROGRESS
		else if( sdgAuto.CurrState == SDGAutomaton.LOGGING_IN_PROGRESS )
		{
			//if user ends logging by pressing key
			if( Input.GetKeyDown(stopLog) )
			{
				Debug.Log( "Finished logging state tuples <----" );

				//save the log to the file
				sw = new StreamWriter(outputFile);
				sw.Write( sb.ToString() );
				sw.Close();

				//enact transition to the next state
				sdgAuto.Transition( SDGAutomaton.FINISHED_LOGGING );
			}
			//otherwise, log state information
			else
			{
				//gather information about states
				mainAutoState = mainComm.mainAuto.CurrState;
				sessionAutoState = sessionComm.sessionAuto.CurrState;
				agentAutoState = agentComm.agentAuto.CurrState;
				agentYComp = agentTrans.position.y;

				//assemble into a tuple
				string tuple =
						mainAutoState.ToString() + "," +
						sessionAutoState.ToString() + "," +
						agentAutoState.ToString() + "," +
						agentYComp.ToString() + "\n";

				//add the tuple to the string builder object
				sb.Append( tuple );
			}
		}
		//FINISHED LOGGING
		else if( sdgAuto.CurrState == SDGAutomaton.FINISHED_LOGGING )
		{
			//do nothing for now
		}
	}


}

public class SDGAutomaton : Automaton
{
	/* State Constants */
	public const int NOT_YET_STARTED_LOGGING = 0;
	public const int LOGGING_IN_PROGRESS = 1;
	public const int FINISHED_LOGGING = 2;

	/**
	 * Default constructor.
	 */
	public SDGAutomaton() : base()
	{
		this.Transition ( NOT_YET_STARTED_LOGGING );
	}
}



