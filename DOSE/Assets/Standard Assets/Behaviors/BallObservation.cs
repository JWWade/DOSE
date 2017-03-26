using System;
using UnityEngine;
using System.Collections;

public class BallObservation : MonoBehaviour
{
	private BallMovementAutomaton bmAuto;
	private AgentInput agentComm;
	[HideInInspector]
	public bool ballLaunced;
	[HideInInspector]
	public bool ballCollidedPaddle;
	private float timeThreshold;
	private DateTime timeBeganObserving;
	private DateTime timeOfLastCurrPosCapture;
	private float prevBx, currBx;
	private Vector2 currPos;
	private PositionSamples posSamples;

	void Start ()
	{
		bmAuto = new BallMovementAutomaton ();
		agentComm = GameObject.Find("Agent").GetComponent<AgentInput>();
		ballLaunced = false;
		ballCollidedPaddle = false;
		timeThreshold = 25; //milliseconds
		timeOfLastCurrPosCapture = DateTime.Now;
		prevBx = 0F; currBx = 0F;
		currPos = BallUtils.GetBallPosition ();
		posSamples = new PositionSamples (currPos);
	}

	void Update ()
	{
		//if this process is the server
		if( GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_SERVER )
		{
			//store ball position changes
			if(DateTime.Now.Subtract(timeOfLastCurrPosCapture).TotalMilliseconds > 50)
			{
				//capture the current time
				timeOfLastCurrPosCapture = DateTime.Now;
			}
			currPos = BallUtils.GetBallPosition ();
			posSamples.AddSample(currPos);

			
//			//draw lines TODO: remove this when done testing
//			Vector2 p1 = BallUtils.GetBallPosition();
//			Vector2 ballVel = BallUtils.GetBallVelocity();
//			Vector2 p2 = new Vector2( p1.x+5F*ballVel.x, p1.y+5F*ballVel.y );
//			Vector3 draw1 = new Vector3 ( p1.x, p1.y, 0 );
//			Vector3 draw2 = new Vector3 ( p2.x, p2.y, 0 );
//			Debug.DrawLine ( draw1, draw2, Color.green );


			//CURRENT STATE: MATCH HAS NOT YET BEGUN
			if( bmAuto.CurrState == BallMovementAutomaton.MATCH_NOT_IN_PROGRESS )
			{
				//if the match begins
				if( GeneralUtils.MatchInProgress() )
				{
					//enact transition to the next state
					bmAuto.Transition( BallMovementAutomaton.BALL_NOT_LAUNCHED );
				}
			}
			//CURRENT STATE: BALL HAS NOT YET BEEN LAUNCHED / COLLIDED
			else if( bmAuto.CurrState == BallMovementAutomaton.BALL_NOT_LAUNCHED )
			{
				//if the ball is launched
				if( ballLaunced )
				{
					//reset flags
					ballLaunced = false;

					//capture the current time
					timeBeganObserving = DateTime.Now;

					//capture the ball's position
					prevBx = BallUtils.GetBallPosition().x;

					//enact transition to the next state
					bmAuto.Transition( BallMovementAutomaton.OBSERVING_TRAJECTORY );
				}

				//if the match ends
				if( !GeneralUtils.MatchInProgress() )
				{
					//enact transition to the next state
					bmAuto.Transition( BallMovementAutomaton.MATCH_NOT_IN_PROGRESS );
				}
			}
			//CURRENT STATE: CURRENTLY OBSERVING THE BALL'S TRAJECTORY
			else if( bmAuto.CurrState == BallMovementAutomaton.OBSERVING_TRAJECTORY )
			{
				//update the position sample object with the current position
				posSamples.AddSample(currPos);
				
				//if the ball has a collision
				if( ballCollidedPaddle )
				{
					//reset flag
					ballCollidedPaddle = false;
					
					//capture the current time
					timeBeganObserving = DateTime.Now;
					
					//capture the ball's position
					prevBx = BallUtils.GetBallPosition().x;
					
					//enact transition to the next state
					bmAuto.Transition( BallMovementAutomaton.OBSERVING_TRAJECTORY );
				}

				//if enough time has elapsed
				if( DateTime.Now.Subtract( timeBeganObserving ).TotalMilliseconds > timeThreshold )
				{
					//capture the ball's current position
					currBx = BallUtils.GetBallPosition().x;

					//if the ball has moved closer to the right paddle
					if( currBx > prevBx )
					{
						//annouce that the ball is moving towards the right paddle
						agentComm.ballMovingToRight = true;
						agentComm.ballMovingToLeft = false;
						//print ( "======> RIGHT, v = " + GeneralUtils.GetBallVelocity().ToString() );
					}
					//otherwise
					else
					{
						//annouce that the ball is moving towards the left paddle
						agentComm.ballMovingToRight = false;
						agentComm.ballMovingToLeft = true;
						//print ( "======> LEFT, v = " + GeneralUtils.GetBallVelocity().ToString() );
					}

					//enact transition to the next state
					bmAuto.Transition( BallMovementAutomaton.OBSERVING_TRAJECTORY ); //intended self transition
				}
				
				//if the match ends
				if( !GeneralUtils.MatchInProgress() )
				{
					//enact transition to the next state
					bmAuto.Transition( BallMovementAutomaton.MATCH_NOT_IN_PROGRESS );
				}
			}
		}
	}

	void OnGUI()
	{
//		//if this process is the server
//		if( GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_SERVER )
//		{
//			//debug GUI
//			string debugStr = "[observ.state=" + bmAuto.StateString() + "] [Vx="
//				+ GetBallVelocity().x.ToString("F5") + "]";
//			GUI.Label ( _GUI_.ClientDebugRect, _GUI_.FormatClientDebugText( debugStr ) );
//		}
	}

	public Vector2 GetBallVelocity()
	{
		return posSamples.GetChangeInPosition ();
	}
}


public class BallMovementAutomaton : Automaton
{
	public const int MATCH_NOT_IN_PROGRESS=0;
	public const int BALL_NOT_LAUNCHED=1;
	public const int OBSERVING_TRAJECTORY=2;

	public BallMovementAutomaton() : base()
	{
		this.Transition (MATCH_NOT_IN_PROGRESS);
	}

	/**
	 * This method returns the current state as a string.
	 */
	public string StateString()
	{
		switch(this.CurrState)
		{
		case MATCH_NOT_IN_PROGRESS:
			return "MATCH_NOT_IN_PROGRESS";
		case BALL_NOT_LAUNCHED:
			return "BALL_NOT_LAUNCHED";
		case OBSERVING_TRAJECTORY:
			return "OBSERVING_TRAJECTORY";
		default:
			return "UNKNOWN_STATE";
		}
	}
}

public class PositionSamples
{
	public Vector2[] samples;
	private int lastInsertIndex;
	private int SAMPLE_SIZE=5;

	public PositionSamples(Vector2 v)
	{
		samples = new Vector2[SAMPLE_SIZE];
		for(int i = 0; i < SAMPLE_SIZE; i++)
			samples[i] = v;
		lastInsertIndex = 0;
	}

	public void AddSample( Vector2 s )
	{
		samples [lastInsertIndex] = s;
		lastInsertIndex++;
		if (lastInsertIndex == SAMPLE_SIZE)
			lastInsertIndex = 0;
	}

	public Vector2 GetChangeInPosition()
	{
		int oldest = lastInsertIndex + 1;
		if( oldest == SAMPLE_SIZE )
			oldest = 0;
		Vector2 change = samples [lastInsertIndex] - samples [oldest];
		return change;
	}
}