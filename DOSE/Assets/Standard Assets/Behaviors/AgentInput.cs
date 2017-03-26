using System;
using UnityEngine;
using System.Collections;

public class AgentInput : MonoBehaviour
{
	public Transform myTransform;
	public Renderer myRenderer;
	[HideInInspector]
	public AgentAutomaton agentAuto;
	private MotionPlanningAutomaton motionAuto;
	private Vector3 initPos;
	public float b = 14.241F; //y-axis beginning point
	public float e = 74.206F; //y-axis end point
	private int t = (int)(1F * 1000); //n * 1000 = milliseconds period
	public int ii = 0;
	public bool dir = true; //false=down,true=up
	private float yMax = 84.69508F;
	private float yMin = 0.3713531F;
	public DateTime timeOfLastIncrement;
	private DateTime timeBallMovesTowardsAgent;
	public int stepCount = 20;
	[HideInInspector]
	public byte mySkillLevel;
	[HideInInspector]
	public bool ServerAgentBehavior;
	[HideInInspector]
	public bool ballLaunched;
	private Vector2 DestPos; //destination position after calc trajectory
	[HideInInspector]
	public bool ballMovingToRight;
	[HideInInspector]
	public bool ballMovingToLeft;
	[HideInInspector]
	public bool ballHitSomething;
//	public GameObject destIndic; //destination indicator
	private float FAST = 300F;
	private float NORMAL = 150F;

	void Awake ()
	{
		//cache component lookup
		myTransform = transform;
		myRenderer = transform.GetComponent<Renderer> ();
	}

	void Start ()
	{
		agentAuto = new AgentAutomaton ();
		motionAuto = new MotionPlanningAutomaton ();
		initPos = transform.position;
		timeOfLastIncrement = DateTime.Now;
		timeBallMovesTowardsAgent = DateTime.Now;
		mySkillLevel = GeneralUtils.AGENT_SKILL_RANDOM;
		ServerAgentBehavior = false;
		ballLaunched = false;
		ballMovingToLeft = false;
		ballMovingToRight = false;
		ballHitSomething = false;
	}

	void Update ()
	{
		//if the agent should perform autonomous behavior
		if( ServerAgentBehavior )
		{
			/** AGENT AUTOMATON **/
			//--------------------------------------------------------------------
			//CURRENT STATE: GAME NOT YET IN PROGRESS
			//--------------------------------------------------------------------
			if( agentAuto.CurrState == AgentAutomaton.WAIT_GAME_BEGIN )
			{
				//if the Match begins
				if( GeneralUtils.MatchInProgress() && ballLaunched )
				{
					//get match information
					byte config = GeneralUtils.GetCurrentConfig();

					//reset flag
					ballLaunched = false;

					//enact transition to the next state
					if( !GeneralUtils.AgentPlaying(config) )
						agentAuto.Transition( AgentAutomaton.NOT_USING_AGENT );
					else
						agentAuto.Transition( AgentAutomaton.GAME_IN_PROGRESS );
				}
			}
			//--------------------------------------------------------------------
			//CURRENT STATE: GAME IN PROGRESS
			//--------------------------------------------------------------------
			else if( agentAuto.CurrState == AgentAutomaton.GAME_IN_PROGRESS )
			{
				//if the game ends
				if( !GeneralUtils.MatchInProgress() )
				{
					//reset position
					transform.position = initPos;

					//enact transition to the next state
					agentAuto.Transition( AgentAutomaton.WAIT_GAME_BEGIN );
				}
				else
				{
					//---------------------------------------------
					//if EASY difficulty
					//---------------------------------------------
					if( mySkillLevel == GeneralUtils.AGENT_SKILL_RANDOM ) {
						if(DateTime.Now.Subtract( timeOfLastIncrement ).TotalMilliseconds >= stepCount) {
							//capture the current time
							timeOfLastIncrement = DateTime.Now;

							//calculate i (time elapsed)
							if(dir == true) { //moving up
								ii += stepCount;
								if( ii >= t )
									dir = !dir;
							} else { //moving down
								ii -= stepCount;
								if( ii <= 0 )
									dir = !dir;
							}

							//set my position
							transform.position = new Vector3(transform.position.x, ((e - b) * ii) / t + b, 0);
						}
					}
					//---------------------------------------------
					//if MEDIUM or HARD difficulty
					//---------------------------------------------
					else if( mySkillLevel == GeneralUtils.AGENT_SKILL_NORMAL || mySkillLevel == GeneralUtils.AGENT_SKILL_FAST )
					{
						//CURRENT STATE: AGENT PADDLE IS DORMANT
						if( motionAuto.CurrState == MotionPlanningAutomaton.DORMANT )
						{
							//if the ball has moved closer to the agent paddle
							if( ballMovingToRight ) {
								//reset flag
								ballMovingToRight=false;

								//capture the current time
								timeBallMovesTowardsAgent = DateTime.Now;

								//enact transition to the next state
								motionAuto.Transition( MotionPlanningAutomaton.BRIEF_PAUSE );
							}
						}
						//CURRENT STATE: BRIEF PAUSE
						else if( motionAuto.CurrState == MotionPlanningAutomaton.BRIEF_PAUSE )
						{
							//if some small time has passed
							if( DateTime.Now.Subtract( timeBallMovesTowardsAgent ).TotalMilliseconds > 400 ) {
								//enact transition to the next state
								motionAuto.Transition( MotionPlanningAutomaton.CALCULATING_TRAJECTORY );
							}
						}
						//CURRENT STATE: CALCULATING TRAJECTORY
						else if( motionAuto.CurrState == MotionPlanningAutomaton.CALCULATING_TRAJECTORY )
						{
							//calculate trajectory
							DestPos = GeneralUtils.GetAgentDestination();
							if(DestPos.y > yMax) DestPos.y = yMax;
							if(DestPos.y < yMin) DestPos.y = yMin;

//							//place the indicator at the destination position for debugging purposes
//							destIndic.transform.position = new Vector3( DestPos.x, DestPos.y, 0 );

							//enact transition to the next state
							motionAuto.Transition( MotionPlanningAutomaton.MOVING_TOWARDS_GOAL );
						}
						//CURRENT STATE: AGENT PADDLE MOVING TOWARDS GOAL
						else if( motionAuto.CurrState == MotionPlanningAutomaton.MOVING_TOWARDS_GOAL )
						{
							//if the ball hits something else on the way towards the paddle
							if( ballHitSomething ) {
								//reset event flag
								ballHitSomething = false;

								//capture the current time
								timeBallMovesTowardsAgent = DateTime.Now;

								//enact transition to the next state
								motionAuto.Transition( MotionPlanningAutomaton.BRIEF_PAUSE );
							}
							//move towards goal if not yet at goal
							else if( Vector2.Distance( GeneralUtils.GetAgentPosition(), DestPos ) > 2F ) {
								Vector3 pos = transform.position;
								if(pos.y > yMax) pos.y = yMax;
								if(pos.y < yMin) pos.y = yMin;
								
								float k = mySkillLevel == GeneralUtils.AGENT_SKILL_NORMAL ? NORMAL : FAST;
								float speed = Time.deltaTime * k;
								Vector2 updatePos = Vector2.MoveTowards(GeneralUtils.GetAgentPosition(), DestPos, speed);
								pos.y = updatePos.y;
								transform.position = pos;
							}
							//otherwise, arrived at goal
							else {
								//enact transition to the next state
								motionAuto.Transition( MotionPlanningAutomaton.AT_GOAL );
							}
						}
						//CURRENT STATE: AGENT PADDLE IS AT GOAL
						else if( motionAuto.CurrState == MotionPlanningAutomaton.AT_GOAL )
						{
							//if the ball has moved further away from the agent paddle
							if( ballMovingToLeft ) {
								//reset flag
								ballMovingToLeft = false;
								//enact transition to the next state
								motionAuto.Transition( MotionPlanningAutomaton.DORMANT );
							}
							//if the ball hits something else on the way towards the paddle
							else if( ballHitSomething ) {
								//reset flag
								ballHitSomething = false;
								
								//capture the current time
								timeBallMovesTowardsAgent = DateTime.Now;
								//print ("re-calculating after ball hit divider<------------------");
								
								//enact transition to the next state
								motionAuto.Transition( MotionPlanningAutomaton.BRIEF_PAUSE );
							}
						}
					}


				}
			}
			//--------------------------------------------------------------------
			//CURRENT STATE: NOT USING AGENT
			//--------------------------------------------------------------------
			else if( agentAuto.CurrState == AgentAutomaton.NOT_USING_AGENT )
			{
			}
		}
	}
}

public class AgentAutomaton : Automaton
{
	public const int WAIT_GAME_BEGIN = 0;
	public const int GAME_IN_PROGRESS = 1;
	public const int NOT_USING_AGENT = 2;

	public AgentAutomaton() : base()
	{
		Transition (WAIT_GAME_BEGIN);
	}

}

public class MotionPlanningAutomaton : Automaton
{
	public const int DORMANT = 0;
	public const int BRIEF_PAUSE = 4;
	public const int CALCULATING_TRAJECTORY = 1;
	public const int MOVING_TOWARDS_GOAL = 2;
	public const int AT_GOAL=3;

	public MotionPlanningAutomaton() : base()
	{
		this.Transition (DORMANT);
	}
}
