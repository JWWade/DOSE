using System;
using UnityEngine;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Main : MonoBehaviour
{
	[HideInInspector]
	public MainAutomaton mainAuto;
	[HideInInspector]
	public bool GetEnvSessionInfoDone;
	[HideInInspector]
	public bool AllClientsConnected;
	[HideInInspector]
	public bool SessionEnd;
	[HideInInspector]
	public byte ProcessRole;
	[HideInInspector]
	public bool ClientConnectedToServer;
	public Texture backgroundTexture;
	private DateTime clientBeginWaitTime;
	private bool shrinkServerScreen = false;
    private SpriteRenderer backgroundEnv;

	//temporary code for Session creation
	private string sPath;

	void Awake ()
	{
		sPath = Application.dataPath + @"\Config\Sessions\S_###.json";
		Screen.SetResolution (1600, 900, false);
		GeneralUtils.InitGeneralUtils ();
		mainAuto = new MainAutomaton ();
		ProcessRole = GeneralUtils.UNASSIGNED;
		GetEnvSessionInfoDone = false;
		AllClientsConnected = false;
		SessionEnd = false;
		ClientConnectedToServer = false;
        backgroundEnv = GameObject.Find("Background").GetComponent<SpriteRenderer>();

		//temporary code for RallyConfiguration object creation
//		RallyConfiguration rcfg = new RallyConfiguration ( 1.5F, 0.5F, 20F );
//		GeneralUtils.WriteContentToFile(Application.dataPath + "/Config/RallyConfiguration.json", rcfg.ToJsonString());

		//temporary code for Session creation
//		Create__Practice_Session (sPath);
//		Create__PrePostTest_Rally_Session (sPath);
//		Create__MidTest_Collab_Session (sPath);
//		Create__MidTest_Compet_Session (sPath);
	}

	void OnGUI ()
	{
		//for some reason, mainAuto is sometimes null so guarantee this doesn't happen
		if (mainAuto == null)
		{
			mainAuto = new MainAutomaton ();
			//UnityEngine.Debug.Log ("reinitializing mainAuto");
		}

		/** MAIN AUTOMATON **/
		//CURRENT STATE: WAITING FOR THE USER TO DESIGNATE ROLE OF PROCESS
		if( mainAuto.CurrState == MainAutomaton.WAIT_USER_DESIGNATE_PROCESS_ROLE )
		{
			//draw background box and buttons for selecting Process Role
			GUI.DrawTexture(_GUI_.fullscreenRect, backgroundTexture);
			if( GUI.Button( _GUI_.PongServerButtonRect, "Pong Server" ) ) {
				//set Role
				ProcessRole = GeneralUtils.ROLE_SERVER;
				DynamicallyAssignRoleScripts();

				//enact transition to the next state
				mainAuto.Transition( MainAutomaton.SERVER_GET_ENV_SESSION_INFO_FROM_USER );
			} else if( GUI.Button( _GUI_.HumanClient1ButtonRect, "Client 1" ) ) {
				//set Role
				ProcessRole = GeneralUtils.ROLE_CLIENT;
				GeneralUtils.ASSIGNED_PONG_CLIENT_ID = GeneralUtils.PONG_CLIENT1_ID;
                DynamicallyAssignRoleScripts();

                //set game background image
                backgroundEnv.sprite = Resources.Load("Player One Background Image", typeof(Sprite)) as Sprite;

				//capture current time
				clientBeginWaitTime = DateTime.Now;
				
				//enact transition to the next state
				mainAuto.Transition( MainAutomaton.CLIENT_WAIT_N_SECONDS );
			} else if( GUI.Button( _GUI_.HumanClient2ButtonRect, "Client 2" ) ) {
				//set Role
				ProcessRole = GeneralUtils.ROLE_CLIENT;
				GeneralUtils.ASSIGNED_PONG_CLIENT_ID = GeneralUtils.PONG_CLIENT2_ID;
                DynamicallyAssignRoleScripts();

                //set game background image
                backgroundEnv.sprite = Resources.Load("Player Two Background Image", typeof(Sprite)) as Sprite;
				
				//capture current time
				clientBeginWaitTime = DateTime.Now;
				
				//enact transition to the next state
				mainAuto.Transition( MainAutomaton.CLIENT_WAIT_N_SECONDS );
			}
		}
		//CURRENT STATE: (SERVER) GET ENV/SESSION INFO FROM THE USER
		else if( mainAuto.CurrState == MainAutomaton.SERVER_GET_ENV_SESSION_INFO_FROM_USER )
		{
			//if the information submission is complete
			if( GetEnvSessionInfoDone )
			{
				//reset flag
				GetEnvSessionInfoDone = false;

				//enact transition to the next state
				mainAuto.Transition( MainAutomaton.SERVER_WAIT_ALL_CLIENTS_CONNECTED );
			}
		}
		//CURRENT STATE: (SERVER) WAIT UNTIL ALL CLIENTS ARE CONNECTED
		else if( mainAuto.CurrState == MainAutomaton.SERVER_WAIT_ALL_CLIENTS_CONNECTED )
		{
			//if all clients are now connected
			if( AllClientsConnected )
			{
				//announce session begin
				EventUtils.AnnounceSessionBegin();

				//reduce the resolution of the server GUI to improve performance
				if( shrinkServerScreen )
					Screen.SetResolution(600,400,false);

				//enact transition to the next state
				mainAuto.Transition( MainAutomaton.SERVER_SESSION_IN_PROGRESS );
			}
		}
		//CURRENT STATE: (SERVER) SESSION IN PROGRESS
		else if( mainAuto.CurrState == MainAutomaton.SERVER_SESSION_IN_PROGRESS )
		{
			//if the session ends
			if( SessionEnd )
			{
				//reset flag
				SessionEnd = false;

				//reset resolution
				if( shrinkServerScreen )
					Screen.SetResolution (1600, 900, false);

				//enact transition to the next state
				mainAuto.Transition( MainAutomaton.SESSION_FINISHED );
			}
		}
		//CURRENT STATE: (CLIENT) WAIT N-SECONDS
		else if( mainAuto.CurrState == MainAutomaton.CLIENT_WAIT_N_SECONDS )
		{
			//if n-seconds elapsed
			if( DateTime.Now.Subtract(clientBeginWaitTime).TotalMilliseconds >= 100 )
			{
				//enact transition to the next state
				mainAuto.Transition( MainAutomaton.CLIENT_TRYING_TO_CONNECT_TO_SERVER );
			}
		}
		//CURRENT STATE: (CLIENT) TRYING TO CONNECT TO THE SERVER
		else if( mainAuto.CurrState == MainAutomaton.CLIENT_TRYING_TO_CONNECT_TO_SERVER )
		{
			//if the client is successfully connected
			if( ClientConnectedToServer )
			{
				//reset flag
				ClientConnectedToServer = false;

				//enact transition to the next state
				mainAuto.Transition( MainAutomaton.CLIENT_SESSION_IN_PROGRESS );
			}
		}
		//CURRENT STATE: (CLIENT) SESSION IN PROGRESS
		else if( mainAuto.CurrState == MainAutomaton.CLIENT_SESSION_IN_PROGRESS )
		{
			//if the session ends
			if( SessionEnd )
			{
				//reset flag
				SessionEnd = false;
				
				//reset resolution
				Screen.SetResolution (1600, 900, false);

				//enact transition to the next state
				mainAuto.Transition( MainAutomaton.SESSION_FINISHED );
			}
		}
		//CURRENT STATE: SESSION FINISHED
		else if( mainAuto.CurrState == MainAutomaton.SESSION_FINISHED )
		{
			//draw background box and message
			GUI.DrawTexture(_GUI_.fullscreenRect, backgroundTexture);
			GUI.Label ( _GUI_.fullscreenRect, "<size=40>SESSION COMPLETED</size>" );
		}
	}

	/**
	 * This method dynamically assigns a network communication script to the process depending
	 * on the role assigned.
	 */
	private void DynamicallyAssignRoleScripts()
	{
		//if the role is server
		if( ProcessRole == GeneralUtils.ROLE_SERVER )
		{
			//attach a PongServer.cs component to the ScriptHub GameObject
			GameObject go = GameObject.Find("ScriptHub");
			go.AddComponent<PongServer>();
		}
		//if the role is client
		else if( ProcessRole == GeneralUtils.ROLE_CLIENT )
		{
			//attach a PongClient.cs component to the ScriptHub GameObject
			GameObject go = GameObject.Find("ScriptHub");
			go.AddComponent<PongClient>();
		}

		//announce role selection
		EventUtils.AnnounceRoleSelection (ProcessRole);
	}

	/**
	 * This accessor returns true if the game is currently in progress.
	 * Otherwise, returns false.
	 */
	public bool sessionInProgress
	{
		get
		{
			if( mainAuto == null )
				return false;
			else
				return mainAuto.CurrState == MainAutomaton.SERVER_SESSION_IN_PROGRESS ||
					mainAuto.CurrState == MainAutomaton.CLIENT_SESSION_IN_PROGRESS;
		}
	}
	
	/**
	 * This function creates a practice session.
	 */
	private void Create__Practice_Session(string outputFile)
	{
		SessionTimer st = new SessionTimer ((int)(3F * 60 * 1000)); //3 minutes
		Configuration c = new Configuration (Configuration.ONE_PC,
		                                     Configuration.THERAPIST_NOT_PLAYING,
		                                     Configuration.NOT_COLLABORATIVE,
		                                     Configuration.NOT_RALLY);
		Session s = new Session (st, c);
		string serialized = JsonConvert.SerializeObject (s);
		string destFile = outputFile.Replace ("###", "Practice_Session_1PC");
		GeneralUtils.WriteContentToFile (destFile, serialized);
	}

	/**
	 * This function creates a Pre/Post Test Rally session.
	 */
	private void Create__PrePostTest_Rally_Session(string outputFile)
	{
		SessionTimer st = new SessionTimer ((int)(5F * 60 * 1000)); //5 minutes
		Configuration c = new Configuration (Configuration.ONE_PC,
		                                     Configuration.THERAPIST_PLAYING,
		                                     Configuration.COLLABORATIVE,
		                                     Configuration.RALLY);
		Session s = new Session (st, c);
		string serialized = JsonConvert.SerializeObject (s);
		string destFile = outputFile.Replace ("###", "PrePostTest_Rally_Session_1PC");
		GeneralUtils.WriteContentToFile (destFile, serialized);
	}

	/**
	 * This function creates a MidTest Collaborative session.
	 */
	private void Create__MidTest_Collab_Session(string outputFile)
	{
		SessionTimer st = new SessionTimer ((int)(7.5F * 60 * 1000)); //7.5 minutes
		Configuration c = new Configuration (Configuration.TWO_PC,
		                                     Configuration.THERAPIST_PLAYING,
		                                     Configuration.COLLABORATIVE,
		                                     Configuration.NOT_RALLY);
		Session s = new Session (st, c);
		string serialized = JsonConvert.SerializeObject (s);
		string destFile = outputFile.Replace ("###", "MidTest_Collab_Session_2PC");
		GeneralUtils.WriteContentToFile (destFile, serialized);
	}

	/**
	 * This function creates a MidTest Competitive session.
	 */
	private void Create__MidTest_Compet_Session(string outputFile)
	{
		SessionTimer st = new SessionTimer ((int)(7.5F * 60 * 1000)); //7.5 minutes
		Configuration c = new Configuration (Configuration.TWO_PC,
		                                     Configuration.THERAPIST_PLAYING,
		                                     Configuration.NOT_COLLABORATIVE,
		                                     Configuration.NOT_RALLY);
		Session s = new Session (st, c);
		string serialized = JsonConvert.SerializeObject (s);
		string destFile = outputFile.Replace ("###", "MidTest_Compet_Session_2PC");
		GeneralUtils.WriteContentToFile (destFile, serialized);
	}

	/**
	 * This function is called when the application is exited.
	 */
	void OnApplicationExit()
	{
		//reset resolution
		if( shrinkServerScreen )
			Screen.SetResolution (1600, 900, false);
	}

}

public class MainAutomaton : Automaton
{
	public const int WAIT_USER_DESIGNATE_PROCESS_ROLE = 0;
	public const int SERVER_GET_ENV_SESSION_INFO_FROM_USER = 1;
	public const int SERVER_WAIT_ALL_CLIENTS_CONNECTED = 2;
	public const int SERVER_SESSION_IN_PROGRESS = 3;
	public const int CLIENT_WAIT_N_SECONDS = 4;
	public const int CLIENT_TRYING_TO_CONNECT_TO_SERVER = 5;
	public const int CLIENT_SESSION_IN_PROGRESS = 6;
	public const int SESSION_FINISHED = 7;

	public MainAutomaton() : base()
	{
		Transition (WAIT_USER_DESIGNATE_PROCESS_ROLE);
	}
}



