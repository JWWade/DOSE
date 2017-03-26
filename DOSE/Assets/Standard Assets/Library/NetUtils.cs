using UnityEngine;
using System.Collections;

public static class NetUtils
{
	private static Main m_mainScript;
	private static MenuBehavior m_menuScript;

	/**
	 * Static constructor.
	 */
	public static void InitNetUtils()
	{
		m_mainScript = GameObject.Find("ScriptHub").GetComponent<Main>();
		m_menuScript = GameObject.Find("ScriptHub").GetComponent<MenuBehavior>();
	}

	/**
	 * Announce that all the clients have connected to the server.
	 */
	public static void AnnounceAllClientsConnected()
	{
		m_mainScript.AllClientsConnected = true;
	}
	
	/**
	 * Announce that this client process has successfully connected to the server.
	 */
	public static void AnnounceClientConnectedToServer()
	{
		m_mainScript.ClientConnectedToServer = true;
	}
	
	/**
	 * Gets Server IP.
	 */
	public static string GetIP()
	{
		return m_menuScript.ServerIPString;
	}
	
	/**
	 * This function returns the client port based on the client process ID.
	 */
	public static int GetMyClientPort()
	{
		if( GeneralUtils.ASSIGNED_PONG_CLIENT_ID == GeneralUtils.PONG_CLIENT1_ID )
			return m_menuScript.ClientPort1;
		else
			return m_menuScript.ClientPort2;
	}
	
	/**
	 * Gets Client Port 1.
	 */
	public static int GetClientPort1()
	{
		return m_menuScript.ClientPort1;
	}
	
	/**
	 * Gets Client Port 2.
	 */
	public static int GetClientPort2()
	{
		return m_menuScript.ClientPort2;
	}
	
	/**
	 * Gets Number of Clients.
	 */
	public static int GetNumClients()
	{
		return m_menuScript.NumPCs;
	}
}