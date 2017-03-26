using System;
using UnityEngine;
using System.Collections;

public class BallBehavior : MonoBehaviour
{
	private Rigidbody2D myRigidbody2D;
	private BallAutomaton ballAuto;
	private AgentInput agentComm;
	private Vector2 dormantPosition;
	private Vector2 startPosition;
	private Vector2 activatePosition;
	private int t_remainDormant;
	private byte cachedWinner; //in {HUMAN_PLAYER, AGENT_PLAYER}
	private System.Random rand;
	private bool[] signs;
	[HideInInspector]
	public bool ServerBallBehavior;
	public AudioClip pongBallHitSound;
	[HideInInspector]
	public int ballHitCount;
	private Vector3 currTraj, currPos;
	private Vector3 prevTraj, prevPos;
	private float Cs, Cm, Cf; //slow, medium and fast scalars, respectively
	[HideInInspector]
	public bool endedInDraw;

	void Awake ()
	{
		//cache component lookup
		myRigidbody2D = transform.GetComponent<Rigidbody2D> ();
	}

	void Start ()
	{
		agentComm = GameObject.Find("Agent").GetComponent<AgentInput>();
		ballAuto = new BallAutomaton ();
		dormantPosition = new Vector2 (1000F, 1000F);
		startPosition = new Vector2 (-19.04773F, 81.49192F);
		activatePosition = new Vector2 (-19.04773F, 41.20192F);
		MakeDormant ();
		cachedWinner = GeneralUtils.UNASSIGNED;
		rand = new System.Random (DateTime.Now.Millisecond);
		signs = new bool[8]{true, false, true, false, true, false, true, false};
		ServerBallBehavior = false;
		ballHitCount = 0;
		Cs = 1.5F;
		Cm = 2.5F;
		Cf = 3.5F;
		endedInDraw = false;
	}

	void Update ()
	{
		//the ball is only active when the game is in progress
		if( GeneralUtils.MatchInProgress() )
		{
			//ROLE: SERVER
			if( ServerBallBehavior )
			{
				/** BALL AUTOMATON **/
				//CURRENT STATE: DORMANT
				if( ballAuto.CurrState == BallAutomaton.DORMANT )
				{
					//if 1/2 second has elpased since the Match began
					if( GeneralUtils.TimeSinceMatchBegan() >= 500 )
					{
						//reset ball to starting position
						MoveToStart();

						//apply a small downward force to simulate falling
						ApplyDownwardForce();

						//enact transition to the next state
						ballAuto.Transition( BallAutomaton.FALLING );
					}
				}
				//CURRENT STATE: FALLING
				else if( ballAuto.CurrState == BallAutomaton.FALLING )
				{
					//Debug.Log ( rigidbody2D.velocity.magnitude.ToString("F8") );

					//if the ball has fallen within range of the trigger position
					if( Vector2.Distance(transform.position, activatePosition) < 1.5F )
					{
						//apply a random force to the ball
						ApplyRandomForce();

						//announce that the ball has been launched
						EventUtils.LogEvent( _Event.BALL_LAUNCH );
						EventUtils.AnnounceBallLaunch();

						//enact transition to the next state
						ballAuto.Transition( BallAutomaton.IN_PLAY );
					}
				}
				//CURRENT STATE: IN PLAY
				else if( ballAuto.CurrState == BallAutomaton.IN_PLAY )
				{
					//if the event endedInDraw occurred
					if( endedInDraw )
					{
						//reset flag
						endedInDraw = false;

						//make the ball dormant again
						MakeDormant ();

						//enact transition to the next state
						ballAuto.Transition( BallAutomaton.DORMANT );
					}
					//if the ball goes offscreen
					else if( BallOffscreen() )
					{
						//announce the Match ends to the Session script
						if( !GeneralUtils.IsRally( GeneralUtils.GetCurrentConfig() ) )
							EventUtils.AnnouncePoint( cachedWinner );
						//otherwise
						else
							EventUtils.AnnounceMatchEnd();

						//make the ball dormant again
						MakeDormant ();

						//enact transition to the next state
						ballAuto.Transition(BallAutomaton.DORMANT);
					}
				}
			}
			//ROLE: CLIENT
			else
			{

			}
		}
	}

	public void OnCollisionExit2D( Collision2D c )
	{
		if( GeneralUtils.MatchInProgress() && GeneralUtils.GetProcessRole() == GeneralUtils.ROLE_SERVER)
		{
			//increment the ball hit count
			ballHitCount += 1;

			//if the ball hit a paddle
			foreach( ContactPoint2D contactPt2d in c.contacts )
			{
				if( contactPt2d.collider.tag == "paddle" )
				{
					//announce that the ball has hit a paddle
					EventUtils.LogEvent( _Event.BALL_HIT );

					//if this is Rally mode, announce a point
					if( GeneralUtils.IsRally( GeneralUtils.GetCurrentConfig() ) )
						EventUtils.AnnouncePoint( GeneralUtils.RALLY_ID );

					//get ball and paddle positions as well as paddle length
					Vector3 paddlePos;
					float Pl = 0F;
					if( contactPt2d.collider.transform.name == "P1" )
					{
						//announce hit event
						EventUtils.AnnounceBallHitEvent("Left");
						Pl = GeneralUtils.GetPaddleSize("Left");
						paddlePos = GeneralUtils.GetHumanPosition();
					}
					else
					{
						//announce hit event
						EventUtils.AnnounceBallHitEvent("Right");
						Pl = GeneralUtils.GetPaddleSize("Right");
						paddlePos = GeneralUtils.GetAgentPosition();
					}
					Vector3 ballPos = BallUtils.GetBallPosition();

					//store y component of paddle pos
					float Py = paddlePos.y;

					//get distance between paddle hit and paddle center
					float d = Math.Abs(Py - ballPos.y);

					//piecewise function for adding force perturbation based on paddle hit location
					float scalar = 1F;
					if( d < (Pl/10F) ) //use slow scalar
					{
						scalar = Cs;
					}
					else if( d > (3F*Pl/10F) ) //use fast scalar
					{
						scalar = Cf;
					}
					else //use medium speed scalar
					{
						scalar = Cm;
					}

					//apply force perturbation
					myRigidbody2D.velocity = Vector2.zero;
					myRigidbody2D.angularVelocity = 0;
					if( contactPt2d.collider.name == "P1" )
					{
						myRigidbody2D.AddForce ( new Vector2( scalar* 10F, scalar*2.5F*(signs[rand.Next (0,8)] ? 1F : -1F) ) );
						myRigidbody2D.AddTorque ( 20F );
					}
					else
					{
						myRigidbody2D.AddForce ( new Vector2( scalar*-10F, scalar*2.5F*(signs[rand.Next (0,8)] ? 1F : -1F) ) );
						myRigidbody2D.AddTorque ( -20F );
					}
				}
				else
				{
					//tell the agent that the ball hit something
					agentComm.ballHitSomething = true;
				}
			}
		}
	}

	public void PlayBallHitSound()
	{
		//play sound
		GetComponent<AudioSource>().PlayOneShot(pongBallHitSound);
	}

	/**
	 * This function applies a random force to the ball.
	 */
	private void ApplyRandomForce()
	{
		float fx = 10, fy = 2.5F;
		float c = 1;
		myRigidbody2D.velocity = Vector2.zero;
		myRigidbody2D.angularVelocity = 0F;
		transform.position = activatePosition;

		if( signs[rand.Next (0,8)] )
		{
			myRigidbody2D.AddTorque ( 20F );
			fx *= -1F;
		}
		else
			myRigidbody2D.AddTorque ( -20F );

		if( signs[rand.Next (0,8)] )
			fy *= -1F;

		myRigidbody2D.AddForce ( new Vector2( c*fx, c*fy ) );
	}

	/**
	 * This function applies a small downward force on the ball.
	 */
	private void ApplyDownwardForce()
	{
		float fx = 0F, fy = 15; //TODO play with this parameter
		myRigidbody2D.AddForce ( new Vector2( fx, fy ) );
	}

	/**
	 * This function sets the state of the ball to dormant.
	 * This means that the ball is in a position offscreen
	 * and its velocity is zero.
	 */
	private void MakeDormant()
	{
		myRigidbody2D.velocity = Vector2.zero;
		myRigidbody2D.angularVelocity = 0F;
		transform.position = dormantPosition;
	}

	/**
	 * This function sets the state of the ball to the initial
	 * position for falling.
	 */
	private void MoveToStart()
	{
		myRigidbody2D.velocity = Vector2.zero;
		myRigidbody2D.angularVelocity = 0F;
		transform.position = startPosition;
	}

	/**
	 * This function returns true if the ball has gone offscreen.
	 * Otherwise, returns false.
	 * 
	 * If the distance between a player and the ball is larger than the
	 * distance between both players, then the ball has gone offscreen.
	 */
	public bool BallOffscreen()
	{
		Vector2 h = GeneralUtils.GetHumanPosition ();
		Vector2 a = GeneralUtils.GetAgentPosition ();
		Vector2 b = transform.position; //ball position

		//if human player lost
		if( b.x < h.x ) {
			cachedWinner = GeneralUtils.AGENT_ID;
			return true;
		}
		//if agent lost
		else if( b.x > a.x ) {
			cachedWinner = GeneralUtils.HUMAN_ID;
			return true;
		}
		//otherwise
		else
			return false;
	}
}

public class BallAutomaton : Automaton
{
	public const int DORMANT = 0;
	public const int FALLING = 1;
	public const int IN_PLAY = 2;

	public BallAutomaton() : base()
	{
		Transition (DORMANT);
	}
}