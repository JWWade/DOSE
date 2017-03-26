using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuBehavior : MonoBehaviour
{
	[HideInInspector]
	public int NumHumanPlayers;
	[HideInInspector]
	public string p1ID; //human player 1
	[HideInInspector]
	public string p2ID; //human player 2 (NOT agent)
	[HideInInspector]
	public float hrsGamePerWeekP1;
	[HideInInspector]
	public float hrsGamePerWeekP2;
	public Texture2D backgroundImage;
	[HideInInspector]
	public string AgentSkillLevelString;
	[HideInInspector]
	public int SelectedSession;
	private string SelectedSessionPath;
	[HideInInspector]
	public int NumPCs;
	[HideInInspector]
	public string InputMethodString;
	[HideInInspector]
	public string ServerIPString;
	[HideInInspector]
	public int ClientPort1;
	[HideInInspector]
	public int ClientPort2;
	[HideInInspector]
	public string TAconfigName;
	[HideInInspector]
	public string TAiteration;
	private Dictionary<string, string> sessNumMap;
	private bool clientIPLoaded;


	void Start ()
	{
		p1ID = "UNASSIGNED";
		p2ID = "UNASSIGNED";
		NumHumanPlayers = 0;
		SelectedSession = 1;
		SelectedSessionPath = Application.dataPath + @"\Config\Sessions\S_###.json";
		hrsGamePerWeekP1 = -1F;
		hrsGamePerWeekP2 = -1F;
		NumPCs = 1;
		AgentSkillLevelString = "easy";
		InputMethodString = "keyboard";
		ServerIPString = "127.0.0.1";
		ClientPort1 = 19011;
		ClientPort2 = 19022;
		TAconfigName = "TAconfigName";
		TAiteration = "TAiteration";
		sessNumMap = new Dictionary<string,string> ();
		sessNumMap.Add ( "0", "HvA_1PC" );
		sessNumMap.Add ( "1", "HvA_1PC" );
		sessNumMap.Add ( "2", "HHvA_2PC" );
		sessNumMap.Add ( "3", "HHvA_1PC" );
		sessNumMap.Add ( "4", "HvH_1PC" );
		sessNumMap.Add ( "5", "HvH_2PC" );
		sessNumMap.Add ("6", "Rally_1PC");
		sessNumMap.Add ("7", "Rally_2PC");
		sessNumMap.Add ("10", "Practice_Session_1PC");
		sessNumMap.Add ("11", "PrePostTest_Rally_Session_1PC");
		sessNumMap.Add ("12", "MidTest_Collab_Session_2PC");
		sessNumMap.Add ("13", "MidTest_Compet_Session_2PC");
		clientIPLoaded = false;
	}

	/**
	 * This method returns the selected session name.
	 */
	public string GetSelectedSessionNme()
	{
		//if the name is valid
		if( sessNumMap.ContainsKey( SelectedSession.ToString() ) )
			return sessNumMap[SelectedSession.ToString()];
		else
			return "NULL";

	}

	/**
	 * This method returns the selected session path.
	 */
	public string GetSelectedSessionPath()
	{
		return SelectedSessionPath.Replace ("###", sessNumMap[SelectedSession.ToString()]);
	}

	void OnGUI ()
	{
		//-------------------------------------
		//if server
		//-------------------------------------
		if( GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_SERVER )
		{
			//if the system is in the state for getting Session info from the experiment runner
			if( GeneralUtils.GettingSessionInfoFromUser() )
			{
				//draw background image of GUI
				GUI.DrawTexture(_GUI_.fullscreenRect, backgroundImage);

				//draw area to get Session information
				GUI.Label( _GUI_.Menu_SessionNumberLabelRect, "Select Session #" );
				try{
					//get the session number
					SelectedSession =
						Int32.Parse( GUI.TextArea( _GUI_.Menu_SessionNumberInputRect, SelectedSession.ToString() ) );
					string sessKey = SelectedSession.ToString();
					string sessVal = "";
					//if the session number is valid, write the name of the session in the GUI
					if( sessNumMap.ContainsKey(sessKey) ) {
						sessVal = sessNumMap[sessKey];
						GUI.Label ( _GUI_.Menu_SessionNameLabelRect, sessVal );

						//automatically set the number of PC's based on the Configuration
						if( sessVal.Contains("2PC") )
							NumPCs = 2;
						else if( sessVal.Contains("1PC") )
							NumPCs = 1;
						else
							NumPCs = 1;
					}
				} catch (Exception e){ SelectedSession = 1; }
				
				//draw area to get Number of PCs information
				GUI.Label( _GUI_.Menu_NumPCLabelRect, "Select # of PCs" );
				try{
					GUI.Label( _GUI_.Menu_NumPCInputRect, "<size=20> "+NumPCs.ToString()+"</size>" );
				} catch (Exception e){ NumPCs = 1; }
				
				//draw area to get Input Method information
				GUI.Label( _GUI_.Menu_InputMethodLabelRect, "Select Input Method" );
				InputMethodString = GUI.TextArea( _GUI_.Menu_InputMethodInputRect, InputMethodString );
				
				//draw area to get Server IP information
				GUI.Label( _GUI_.Menu_ServerIPEntryLabelRect, "Enter Server IP" );
				ServerIPString = GUI.TextArea( _GUI_.Menu_ServerIPEntryInputRect, ServerIPString );
				
				//draw area to get Client 1 Port information
				GUI.Label( _GUI_.Menu_ClientPort1LabelRect, "Select Client 1 Port" );
				try{
					ClientPort1 =
						Int32.Parse( GUI.TextArea( _GUI_.Menu_ClientPort1InputRect, ClientPort1.ToString() ) );
				} catch (Exception e){ ClientPort1 = 19011; }

				//display client 2 information entry only if 2 clients are specified
				if( NumPCs == 2 )
				{
					//draw area to get Client 2 Port information
					GUI.Label( _GUI_.Menu_ClientPort2LabelRect, "Select Client 2 Port" );
					try{
						ClientPort2 =
							Int32.Parse( GUI.TextArea( _GUI_.Menu_ClientPort2InputRect, ClientPort2.ToString() ) );
					} catch (Exception e){ ClientPort2 = 19012; }
				}

				//display p1 id entry
				GUI.Label (_GUI_.Menu_P1IDLabelRect, "P1 ID" );
				p1ID = GUI.TextArea( _GUI_.Menu_P1IDInputRect, p1ID );

				//display p2 id entry
				GUI.Label ( _GUI_.Menu_P2IDLabelRect, "P2 ID" );
				p2ID = GUI.TextArea( _GUI_.Menu_P2IDInputRect, p2ID );

				//display p1 hrs gaming per week entry
				GUI.Label ( _GUI_.Menu_P1HrsLabelRect, "P1 Hours Games/Week" );
				try{
					hrsGamePerWeekP1 =
						Single.Parse( GUI.TextArea ( _GUI_.Menu_P1HrsInputRect, hrsGamePerWeekP1.ToString() ) );
				} catch (Exception e){ hrsGamePerWeekP1 = -1F; }

				//display p2 hrs gaming per week entry
				GUI.Label ( _GUI_.Menu_P2HrsLabelRect, "P2 Hours Games/Week" );
				try{
					hrsGamePerWeekP2 =
						Single.Parse( GUI.TextArea ( _GUI_.Menu_P2HrsInputRect, hrsGamePerWeekP2.ToString() ) );
				} catch (Exception e){ hrsGamePerWeekP2 = -1F; }

				//draw button for submitting Session setup information (if entered data is valid)
				if( ValidDataEntered() && GUI.Button(_GUI_.Menu_SubmitButtonRect, "Submit") )
				{
					//announce finished
					EventUtils.AnnounceMenuInfoSubmissionComplete();
				}

//				//get config type for Throughput analysis
//				TAconfigName = GUI.TextArea ( new Rect(Screen.width*0.75F,Screen.height*0.1F,200F,75F), TAconfigName );
//
//				//get iteration number for Throughput analysis
//				TAiteration = GUI.TextArea ( new Rect(Screen.width*0.75F,Screen.height*0.2F,200F,75F), TAiteration );

			}
			//-------------------------------------
			//if client
			//-------------------------------------
//			else if( GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_CLIENT )
//			{
//				//if the client has not yet loaded the IP address of the server
//				if(!clientIPLoaded)
//				{
//					//load the IP
//					ServerIPString = GeneralUtils.ReadContentFromFile(Application.dataPath+"/Config/IPConfig.cfg");
//
//					//set flag
//					clientIPLoaded = true;
//				}
//			}
		}
	}

	/**
	 * This function returns true if the entered data is valid--otherwise
	 * returns false.
	 */
	private bool ValidDataEntered()
	{
		if( sessNumMap.ContainsKey(SelectedSession.ToString()) )
			return true;
		else
			return false;
	}
}