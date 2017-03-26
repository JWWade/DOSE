using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocketDLL;
using Newtonsoft.Json;

public class PongClient : MonoBehaviour
{
	private PongClientAutomaton clientAuto;
	private ClientSocket clientSocket;
	private StreamData recvdData;
	[HideInInspector]
	public EnvState envState;
	private string serverIP;

	//Network analysis variables
	private bool doAnalysis = false;
	private DateTime Tb, Te;
	private int N;
	private float T;
	
	void Start ()
	{
		recvdData = new StreamData ("00:00:00");
		envState = new EnvState ();
		clientAuto = new PongClientAutomaton ();
		N = 0;
		T = 0F;
		serverIP = GeneralUtils.ReadContentFromFile(Application.dataPath+"/Config/IPConfig.cfg");
	}
	
	void Update ()
	{
		//CURRENT STATE: WAIT FOR THE USER TO ENTER SETUP INFORMATION
		if( clientAuto.CurrState == PongClientAutomaton.WAIT_USER_SETUP_INFO )
		{
			//if the Role has been assigned
			if( GeneralUtils.RoleChosen() )
			{
				//enact transition to the next state
				clientAuto.Transition( PongClientAutomaton.TRY_CONNECT_SERVER );
			}
		}
		//CURRENT STATE: TRY TO CONNECT TO THE SERVER
		else if( clientAuto.CurrState == PongClientAutomaton.TRY_CONNECT_SERVER )
		{
			//initiate connection
			clientSocket = new ClientSocket( NetUtils.GetMyClientPort(), 1, serverIP);

			//if connected successfully
			if( clientSocket.Connected )
			{
				//enact transition to the next state
				clientAuto.Transition( PongClientAutomaton.WAIT_RECV_ENV_SETTINGS );
			}
		}
		//CURRENT STATE: WAIT TO RECV ENVIRONMENT SETTINGS FROM SERVER
		else if( clientAuto.CurrState == PongClientAutomaton.WAIT_RECV_ENV_SETTINGS )
		{
			//if received something from server
			if( clientSocket.pollAndReceiveData(clientSocket.Client, recvdData, 10) >= 1 )
			{
				//extract the json part of the message
				string jsonString = recvdData.timeStamp.Substring(recvdData.timeStamp.IndexOf(",")+1);
				envState = JsonConvert.DeserializeObject<EnvState>(jsonString);

				//respond with acknowledgement
				clientSocket.sendData( new StreamData ("00:00:00") );

				//store communication time begin for Throughput analysis
				if(doAnalysis)
					Tb = DateTime.Now;

				//enact transition to the next state
				clientAuto.Transition( PongClientAutomaton.NORMAL_COMMUNICATION );
			}
		}
		//CURRENT STATE: NORMAL COMMUNICATION WITH THE SERVER
		else if( clientAuto.CurrState == PongClientAutomaton.NORMAL_COMMUNICATION )
		{
			//if received something from server
			if( clientSocket.pollAndReceiveData(clientSocket.Client, recvdData, 10) >= 1 )
			{
				//extract the json part of the message
				//Debug.Log ( "recvd = " + recvdData.timeStamp );
				string jsonString = recvdData.timeStamp.Substring(recvdData.timeStamp.IndexOf(",")+1);
				envState = JsonConvert.DeserializeObject<EnvState>(jsonString);

				//update the environment to reflect the contents of the EnvState object
				GeneralUtils.SetEnvState( envState );

				//get human input
				InputData inputData = GeneralUtils.PackageInputData();
				if(inputData == null) inputData = new InputData();

				//send InputData to the server
				string message = "InputData,"+inputData.ToJsonString();
				StreamData dataToSend = new StreamData(message);
				clientSocket.sendData( dataToSend );

				//increment Throughpput counter N
				if(doAnalysis)
					N += 1;
			}

			//if user stops, stop Throughput analysis
			if( doAnalysis && Input.GetKeyDown(KeyCode.C) )
			{
				Te = DateTime.Now;

				T = ((float)N/(float)( Te-Tb ).TotalSeconds);
				string outputFileName = GeneralUtils.GetTAFileName();

				StreamWriter sw = new StreamWriter(outputFileName, true);
				sw.Write ( T.ToString() + "," + N.ToString() + "," + Tb.ToString().Replace(",","_") + "," + Te.ToString().Replace(",","_"));
				sw.Close();
			}

			//if the connection is lost
			if( !clientSocket.Connected )
			{
				//enact transition to the next state
				clientAuto.Transition( PongClientAutomaton.ERROR_STATE );
			}
		}
		//CURRENT STATE: ERROR OCCURRED
		else if( clientAuto.CurrState == PongClientAutomaton.ERROR_STATE )
		{
		}
	}

	void OnGUI()
	{
		string status = "";
		
		if( clientAuto.CurrState == PongClientAutomaton.NORMAL_COMMUNICATION )
			status = "Normal Comm.";
		else if( clientAuto.CurrState == PongClientAutomaton.TRY_CONNECT_SERVER )
			status = "Try conn.";
		else if( clientAuto.CurrState == PongClientAutomaton.WAIT_RECV_ENV_SETTINGS )
			status = "Wait recv sett";
		else if( clientAuto.CurrState == PongClientAutomaton.WAIT_USER_SETUP_INFO )
			status = "Wait user info";
		else if( clientAuto.CurrState == PongClientAutomaton.ERROR_STATE )
			status = "ERROR";
		else
			status = "NA";
		
		GUI.Label ( _GUI_.NetComm_StatusRect, "<color=black>"+status+"</color>" );
	}
}

public class PongClientAutomaton : Automaton
{
	/* State Constants */
	public const int WAIT_USER_SETUP_INFO = 0;
	public const int TRY_CONNECT_SERVER = 1;
	public const int WAIT_RECV_ENV_SETTINGS = 2;
	public const int NORMAL_COMMUNICATION = 3;
	public const int ERROR_STATE = 4;
	
	/**
	 * Constructor
	 */
	public PongClientAutomaton() : base()
	{
		Transition (WAIT_USER_SETUP_INFO);
	}
}