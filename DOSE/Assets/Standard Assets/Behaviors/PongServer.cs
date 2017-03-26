using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SocketDLL;

public class PongServer : MonoBehaviour
{
	private PongServerAutomaton servAuto;
	private ServerSocket serverSocket1;
	private ServerSocket serverSocket2;
	private SystemAnalysis sysAn;
	private double throughput;
	[HideInInspector]
	public string serverIP;
	[HideInInspector]
	public int client1Port;
	[HideInInspector]
	public int client2Port;
	[HideInInspector]
	public bool ConfigInfoSubmitted;
	[HideInInspector]
	public InputData inputData;
	[HideInInspector]
	public bool ballHitEvent;
	int recvdPackets;
	private bool recvdInitClient1=false;
	private bool recvdInitClient2=false;
	private string extraInfo;

	void Start ()
	{
		servAuto = new PongServerAutomaton ();
		ConfigInfoSubmitted = false;
		ballHitEvent = false;
		recvdPackets = 0;
	}

	void Update ()
	{
		//---------------------------------------------------------------------------------
		//CURRENT STATE: WAIT FOR THE USER TO ENTER NETWORK CONFIGURATION INFO
		//---------------------------------------------------------------------------------
		if( servAuto.CurrState == PongServerAutomaton.WAIT_USER_ENTER_CONFIG_INFO )
		{
			//if the user enters the network config info
			if( ConfigInfoSubmitted )
			{
				//enact transition to the next state
				servAuto.Transition( PongServerAutomaton.WAIT_CONNECTION_ESTABLISHED );
			}
		}
		//---------------------------------------------------------------------------------
		//CURRENT STATE: WAIT FOR ESTABLISHED CONNECTION WITH CLIENT(S)
		//---------------------------------------------------------------------------------
		else if( servAuto.CurrState == PongServerAutomaton.WAIT_CONNECTION_ESTABLISHED )
		{
			EnvState envState = GeneralUtils.GetEnvState();
			string message = "EnvState,"+envState.ToJsonString();
			StreamData dataToSend = new StreamData(message);

			//initiate first connection
			serverSocket1 = new ServerSocket( NetUtils.GetClientPort1(), 1, NetUtils.GetIP() );
			serverSocket1.start();
			serverSocket1.sendData(serverSocket1.Client, dataToSend);

			//if there are two clients
			if( NetUtils.GetNumClients() > 1 )
			{
				//initialize second connection
				serverSocket2 = new ServerSocket( NetUtils.GetClientPort2(), 1, NetUtils.GetIP() );
				serverSocket2.start();
				serverSocket2.sendData(serverSocket2.Client, dataToSend);
			}

			//announce that all clients are connected
			NetUtils.AnnounceAllClientsConnected();

			//enact transition to the next state
			servAuto.Transition( PongServerAutomaton.WAIT_INIT_ACK_FROM_CLIENT );
		}
		//---------------------------------------------------------------------------------
		//CURRENT STATE: WAIT FOR INIT ACKNOWLEDGE FROM THE CLIENT
		//---------------------------------------------------------------------------------
		else if( servAuto.CurrState == PongServerAutomaton.WAIT_INIT_ACK_FROM_CLIENT )
		{
			//create a dummy object for receiving data
			StreamData dummy = new StreamData("00:00:00");

			//get data from client 1
			if( !recvdInitClient1 && serverSocket1.pollAndReceiveData(serverSocket1.Client, dummy, 10) >= 1 )
			{
				//respond with the environment data
				extraInfo = ballHitEvent ? "ballHit" : "NULL";
				if( NetUtils.GetNumClients() == 1 ) ballHitEvent = false; //reset flag
				EnvState envState = GeneralUtils.GetEnvState(extraInfo);
				string message = "EnvState,"+envState.ToJsonString();
				StreamData dataToSend = new StreamData(message);
				serverSocket1.sendData( serverSocket1.Client, dataToSend );
				recvdInitClient1=true;
			}

			//get data from second client if applicable
			if(NetUtils.GetNumClients() > 1)
			{
				//get data from client 2
				if( !recvdInitClient2 && serverSocket2.pollAndReceiveData(serverSocket2.Client, dummy, 10) >= 1 )
				{
					//respond with the environment data
					extraInfo = ballHitEvent ? "ballHit" : "NULL";
					ballHitEvent = false; //reset flag
					EnvState envState = GeneralUtils.GetEnvState(extraInfo);
					string message = "EnvState,"+envState.ToJsonString();
					StreamData dataToSend = new StreamData(message);
					serverSocket2.sendData( serverSocket2.Client, dataToSend );
					recvdInitClient2=true;
				}
			}

			//if using two clients and both clients are connected
			if( NetUtils.GetNumClients() > 1 && (recvdInitClient1 && recvdInitClient2) )
			{
				//enact transition to the next state
				servAuto.Transition( PongServerAutomaton.NORMAL_COMMUNICATION );
			}
			//if only using one client and it is connected
			else if( NetUtils.GetNumClients() == 1 && recvdInitClient1 )
			{
				//enact transition to the next state
				servAuto.Transition( PongServerAutomaton.NORMAL_COMMUNICATION );
			}
		}
		//---------------------------------------------------------------------------------
		//CURRENT STATE: NORMAL COMMUNICATION WITH THE CLIENT
		//---------------------------------------------------------------------------------
		else if( servAuto.CurrState == PongServerAutomaton.NORMAL_COMMUNICATION )
		{
			StreamData recvdData = new StreamData("00:00:00");

			//check to see if some data is ready from client 1
			if( serverSocket1.pollAndReceiveData(serverSocket1.Client, recvdData, 10) >= 1 )
			{
				//increment packet count
				recvdPackets += 1;

				//respond with the environment data
				extraInfo = ballHitEvent ? "ballHit" : "NULL";
				if( NetUtils.GetNumClients() == 1 ) ballHitEvent = false; //reset flag
				EnvState envState = GeneralUtils.GetEnvState(extraInfo);
				string message = "EnvState,"+envState.ToJsonString();
				StreamData dataToSend = new StreamData(message);
				serverSocket1.sendData( serverSocket1.Client, dataToSend );

				//extract the json message from the received data
				string jsonString = recvdData.timeStamp.Substring(recvdData.timeStamp.IndexOf(",")+1);
				inputData = JsonConvert.DeserializeObject<InputData>(jsonString);
				GeneralUtils.StoreHumanInput( inputData, GeneralUtils.PONG_CLIENT1_ID );
			}

			//get data from second client if applicable
			if(NetUtils.GetNumClients() > 1)
			{
				//check to see if some data is ready from client 2
				if( serverSocket2.pollAndReceiveData(serverSocket2.Client, recvdData, 10) >= 1 )
				{
					//respond with the environment data
					extraInfo = ballHitEvent ? "ballHit" : "NULL";
					ballHitEvent = false; //reset flag
					EnvState envState = GeneralUtils.GetEnvState(extraInfo);
					string message = "EnvState,"+envState.ToJsonString();
					StreamData dataToSend = new StreamData(message);
					serverSocket2.sendData( serverSocket2.Client, dataToSend );

					//extract the json message from the received data
					string jsonString = recvdData.timeStamp.Substring(recvdData.timeStamp.IndexOf(",")+1);
					inputData = JsonConvert.DeserializeObject<InputData>(jsonString);
					GeneralUtils.StoreHumanInput( inputData, GeneralUtils.PONG_CLIENT2_ID );
				}
			}

			//apply changes to human paddle(s)
			GeneralUtils.ApplyHumanPaddleChanges();

			//if connection is lost
			if( !serverSocket1.Connected )
			{
				//enact transition to the next state
				servAuto.Transition( PongServerAutomaton.ERROR_STATE );
			}
		}
		//---------------------------------------------------------------------------------
		//CURRENT STATE: ERROR OCCURRED
		//---------------------------------------------------------------------------------
		else if( servAuto.CurrState == PongServerAutomaton.ERROR_STATE )
		{
		}
	}

	void OnGUI()
	{
//		string status = "";
//
//		if( servAuto.CurrState == PongServerAutomaton.NORMAL_COMMUNICATION )
//			status = "Normal Comm.";
//		else if( servAuto.CurrState == PongServerAutomaton.WAIT_CONNECTION_ESTABLISHED )
//			status = "Wait conn. est.";
//		else if( servAuto.CurrState == PongServerAutomaton.WAIT_USER_ENTER_CONFIG_INFO )
//			status = "Wait config info";
//		else if( servAuto.CurrState == PongServerAutomaton.WAIT_INIT_ACK_FROM_CLIENT )
//			status = "Wait init ack.";
//		else if( servAuto.CurrState == PongServerAutomaton.ERROR_STATE )
//			status = "ERROR";
//		else
//			status = "NA";
//
//		GUI.Label ( _GUI_.NetComm_StatusRect, "<color=black>"+status+"</color>" );
	}
}

public class PongServerAutomaton : Automaton
{
	/* State Constants */
	public const int WAIT_USER_ENTER_CONFIG_INFO = 0;
	public const int WAIT_CONNECTION_ESTABLISHED = 1;
	public const int WAIT_INIT_ACK_FROM_CLIENT = 2;
	public const int NORMAL_COMMUNICATION = 3;
	public const int ERROR_STATE = 4;

	/**
	 * Constructor
	 */
	public PongServerAutomaton() : base()
	{
		Transition (WAIT_USER_ENTER_CONFIG_INFO);
	}
}

public class SystemAnalysis {
	/* Member Data */
	private double T, N;
	private DateTime startTime;
	
	/**
	 * Default constructor.
	 */
	public SystemAnalysis() {
		T = 0; N = 0;
		startTime = DateTime.Now;
	}
	
	/**
	 * This method increments the packet number.
	 */
	public void IncrementPacketCount() {
		N += 1D;
	}
	
	/**
	 * This method sets the start time for the analysis.
	 */
	public void Start() {
		startTime = DateTime.Now;
	}
	
	/**
	 * This method stops the analysis and returns the throughtput frequency.
	 */
	public double Stop() {
		T = DateTime.Now.Subtract (startTime).TotalSeconds;
		double throughput = T / N;
		T = 0; N = 0;
		return throughput;
	}
}