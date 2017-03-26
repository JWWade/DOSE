using UnityEngine;
using System.Collections;

public class HumanInput : MonoBehaviour {

	public Transform myTransform;
	public Renderer myRenderer;
	private Transform LeftPaddle;
	private Transform RightPaddle;
	private float stepSize = 175F;
	private float yMax = 84.69508F;
	private float yMin = 0.3713531F;
	[HideInInspector]
	public float h1Input = 0;
	[HideInInspector]
	public float h2Input = 9999F;
	[HideInInspector]
	public float compositeInput = 9999F;
	[HideInInspector]
	public int h1Dir = 0;
	[HideInInspector]
	public int h2Dir = 0;
	private float LMax=1.5F;
	private float LMin=0.5F;
	private float VMax=20F;

	void Awake ()
	{
		//cache component lookup
		myTransform = transform;
		myRenderer = transform.GetComponent<Renderer> ();
		LeftPaddle = GameObject.Find("P1").GetComponent<Transform>();
		RightPaddle = GameObject.Find("Agent").GetComponent<Transform>();
	}

	void Start ()
	{
		//store local Rally Configuration parameters
		LMax = GeneralUtils.rallyCfg.GetLMax ();
		LMin = GeneralUtils.rallyCfg.GetLMin ();
		VMax = GeneralUtils.rallyCfg.GetVMax ();
	}

	void Update ()
	{
		//if the role of this process is client
		if( GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_CLIENT )
		{
			//cache the current configuration type
			byte config = GeneralUtils.GetCurrentConfig();

			//for configurations where two humans are not sharing a computer
			if( !GeneralUtils.SharingComputer(config) )
			{
				//get input from client 1
				if( GeneralUtils.ASSIGNED_PONG_CLIENT_ID == GeneralUtils.PONG_CLIENT1_ID )
				{
					//get keyboard input { -1:down, 0:null, +1:up }
					if( Input.GetKey (KeyCode.UpArrow) )
						h1Input = 1F;
					else if( Input.GetKey (KeyCode.DownArrow) )
						h1Input = -1F;
					else
						h1Input = 0F;
				}
				//get input from client 2
				else if( GeneralUtils.ASSIGNED_PONG_CLIENT_ID == GeneralUtils.PONG_CLIENT2_ID )
				{
					//get keyboard input { -1:down, 0:null, +1:up }
					if( Input.GetKey (KeyCode.W) )
						h2Input = 1F;
					else if( Input.GetKey (KeyCode.S) )
						h2Input = -1F;
					else
						h2Input = 0F;
				}
			}
			//otherwise, get both inputs from the same process
			else
			{
				//get keyboard input { -1:down, 0:null, +1:up }
				if( Input.GetKey (KeyCode.UpArrow) )
					h1Input = 1F;
				else if( Input.GetKey (KeyCode.DownArrow) )
					h1Input = -1F;
				else
					h1Input = 0F;

				//get keyboard input { -1:down, 0:null, +1:up }
				if( Input.GetKey (KeyCode.W) )
					h2Input = 1F;
				else if( Input.GetKey (KeyCode.S) )
					h2Input = -1F;
				else
					h2Input = 0F;

				//if this configuration is HHvA_1PC, we have to combine the inputs
				if( config == Configuration.HHvA_1PC )
				{
					//combine the inputs
					if( h1Input > 0 && h2Input > 0 )
						h1Input = 1F;
					else if( h1Input < 0 && h2Input < 0 )
						h1Input = -1F;
					else
						h1Input = 0F;
				}
			}
		}
	}

	/**
	 * This function stores the current direction of movement (i.e., input) of the specified human
	 * player. TODO: store composite input which is currently not being logged
	 */
	public void StoreHumanInput( int dir, string pid, string hid )
	{
		byte config = GeneralUtils.GetCurrentConfig ();

		if( config == Configuration.HvH_1PC || config == Configuration.HvH_2PC || GeneralUtils.IsRally(config) )
		{
			if( hid == "Left" )
			{
				h1Input = dir;
				h1Dir = dir;
			}
			else if( hid == "Right" )
			{
				h2Input = dir;
				h2Dir = dir;
			}
		} else {
			if( pid == GeneralUtils.PONG_CLIENT1_ID )
			{
				h1Input = dir;
				h1Dir = dir;
			}
			else if( pid == GeneralUtils.PONG_CLIENT2_ID )
			{
				h2Input = dir;
				h2Dir = dir;
			}
		}
	}

	/**
	 * This method updates the position of the human player(s) based on the input and
	 * match configuration type.
	 */
	public void UpdatePosition()
	{
		//get the current configuration type
		byte currConfig = GeneralUtils.GetCurrentConfig ();

		//if HvA 1PC: apply changes to left paddle
		if( currConfig == Configuration.HvA_1PC )
		{
			Vector3 p = LeftPaddle.position;
			
			if( h1Dir > 0 )
				LeftPaddle.position = new Vector3( p.x, p.y + stepSize*Time.deltaTime, 0 );
			else if( h1Dir < 0 )
				LeftPaddle.position = new Vector3( p.x, p.y - stepSize*Time.deltaTime, 0 );
			
			p = LeftPaddle.position;
			if( p.y < yMin ) {
				p.y = yMin;
				LeftPaddle.position = p;
			}
			else if( p.y > yMax ) {
				p.y = yMax;
				LeftPaddle.position = p;
			}
		}
		//if HHvA 1PC: apply changes to left paddle from both inputs
		else if( currConfig == Configuration.HHvA_1PC )
		{
			//store the composite input
			compositeInput = h1Dir;
			
			Vector3 p = LeftPaddle.position;
			
			if( h1Dir > 0 )
				LeftPaddle.position = new Vector3( p.x, p.y + stepSize*Time.deltaTime, 0 );
			else if( h1Dir < 0 )
				LeftPaddle.position = new Vector3( p.x, p.y - stepSize*Time.deltaTime, 0 );
			
			p = LeftPaddle.position;
			if( p.y < yMin ) {
				p.y = yMin;
				LeftPaddle.position = p;
			}
			else if( p.y > yMax ) {
				p.y = yMax;
				LeftPaddle.position = p;
			}
		}
		//if HHvA 2PC: apply changes to left paddle from both inputs
		else if( currConfig == Configuration.HHvA_2PC )
		{
			//store the composite input
			if( h1Dir > 0 && h2Dir > 0 ) compositeInput = 1;
			else if( h1Dir < 0 && h2Dir < 0 ) compositeInput = -1;
			else compositeInput = 0;
			
			Vector3 p = LeftPaddle.position;
			
			if( h1Dir > 0 && h2Dir > 0 )
				LeftPaddle.position = new Vector3( p.x, p.y + stepSize*Time.deltaTime, 0 );
			else if( h1Dir < 0 && h2Dir < 0 )
				LeftPaddle.position = new Vector3( p.x, p.y - stepSize*Time.deltaTime, 0 );
			
			p = LeftPaddle.position;
			if( p.y < yMin ) {
				p.y = yMin;
				LeftPaddle.position = p;
			}
			else if( p.y > yMax ) {
				p.y = yMax;
				LeftPaddle.position = p;
			}
		}
		//if HvH 1PC: apply changes based on the inputs from both human participants
		else if( currConfig == Configuration.HvH_1PC )
		{
			//update left paddle
			Vector3 p = LeftPaddle.position;
			
			if( h1Dir > 0 )
				LeftPaddle.position = new Vector3( p.x, p.y + stepSize*Time.deltaTime, 0 );
			else if( h1Dir < 0 )
				LeftPaddle.position = new Vector3( p.x, p.y - stepSize*Time.deltaTime, 0 );
			
			p = LeftPaddle.position;
			if( p.y < yMin ) {
				p.y = yMin;
				LeftPaddle.position = p;
			}
			else if( p.y > yMax ) {
				p.y = yMax;
				LeftPaddle.position = p;
			}

			//update right paddle
			p = RightPaddle.position;
			
			if( h2Dir > 0 )
				RightPaddle.position = new Vector3( p.x, p.y + stepSize*Time.deltaTime, 0 );
			else if( h2Dir < 0 )
				RightPaddle.position = new Vector3( p.x, p.y - stepSize*Time.deltaTime, 0 );
			
			p = RightPaddle.position;
			if( p.y < yMin ) {
				p.y = yMin;
				RightPaddle.position = p;
			}
			else if( p.y > yMax ) {
				p.y = yMax;
				RightPaddle.position = p;
			}

		}
		//if HvH 2PC: apply changes based on which client this is
		else if( currConfig == Configuration.HvH_2PC )
		{
			//update left paddle
			Vector3 p = LeftPaddle.position;
			
			if( h1Dir > 0 )
				LeftPaddle.position = new Vector3( p.x, p.y + stepSize*Time.deltaTime, 0 );
			else if( h1Dir < 0 )
				LeftPaddle.position = new Vector3( p.x, p.y - stepSize*Time.deltaTime, 0 );
			
			p = LeftPaddle.position;
			if( p.y < yMin ) {
				p.y = yMin;
				LeftPaddle.position = p;
			}
			else if( p.y > yMax ) {
				p.y = yMax;
				LeftPaddle.position = p;
			}
			
			//update right paddle
			p = RightPaddle.position;
			
			if( h2Dir > 0 )
				RightPaddle.position = new Vector3( p.x, p.y + stepSize*Time.deltaTime, 0 );
			else if( h2Dir < 0 )
				RightPaddle.position = new Vector3( p.x, p.y - stepSize*Time.deltaTime, 0 );
			
			p = RightPaddle.position;
			if( p.y < yMin ) {
				p.y = yMin;
				RightPaddle.position = p;
			}
			else if( p.y > yMax ) {
				p.y = yMax;
				RightPaddle.position = p;
			}
		}
		//if RALLY_1PC or RALLY_2PC
		else if( GeneralUtils.IsRally(currConfig) )
		{
			//update left paddle position
			Vector3 p = LeftPaddle.position;
			
			if( h1Dir > 0 )
				LeftPaddle.position = new Vector3( p.x, p.y + stepSize*Time.deltaTime, 0 );
			else if( h1Dir < 0 )
				LeftPaddle.position = new Vector3( p.x, p.y - stepSize*Time.deltaTime, 0 );
			
			p = LeftPaddle.position;
			if( p.y < yMin ) {
				p.y = yMin;
				LeftPaddle.position = p;
			}
			else if( p.y > yMax ) {
				p.y = yMax;
				LeftPaddle.position = p;
			}

			//update left paddle scale
			float v2 = VelocityUtils.GetVelocity( "rightPaddle" );
			float len1 = this.GetLength( v2 );
			Vector2 leftPaddleScale = GeneralUtils.GetHumanScale();
			float wid1 = leftPaddleScale.x*(len1/leftPaddleScale.y);
			GeneralUtils.SetPaddleSize( "leftPaddle", len1, wid1 );
			
			//update right paddle position
			p = RightPaddle.position;
			
			if( h2Dir > 0 )
				RightPaddle.position = new Vector3( p.x, p.y + stepSize*Time.deltaTime, 0 );
			else if( h2Dir < 0 )
				RightPaddle.position = new Vector3( p.x, p.y - stepSize*Time.deltaTime, 0 );
			
			p = RightPaddle.position;
			if( p.y < yMin ) {
				p.y = yMin;
				RightPaddle.position = p;
			}
			else if( p.y > yMax ) {
				p.y = yMax;
				RightPaddle.position = p;
			}
			
			//update right paddle scale
			float v1 = VelocityUtils.GetVelocity( "leftPaddle" );
			float len2 = this.GetLength( v1 );
			Vector2 rightPaddleScale = GeneralUtils.GetAgentScale();
			float wid2 = rightPaddleScale.x*(len2/rightPaddleScale.y);
			GeneralUtils.SetPaddleSize( "rightPaddle", len2, wid2 );
		}
	}

	/**
	 * This function takes the velocity of player j and returns the desired length
	 * for player i's paddle (Rally mode only).
	 */
	private float GetLength( float vj )
	{
		if(float.IsNaN(vj))
			return LMin;
		else
		{
			float leni = ((LMax-LMin)/VMax)*vj + LMin;
			if(leni > LMax)
				leni = LMax;
			if(leni < LMin)
				leni = LMin;
			return leni;
		}
	}
}
