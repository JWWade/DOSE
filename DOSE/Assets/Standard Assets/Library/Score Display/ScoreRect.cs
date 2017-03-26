using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ScoreRect
{
	/* Static Members */
	public static readonly string TYPE_UNASSIGNED = "UNASSIGNED";
	public static readonly string TYPE_LEFT_PADDLE = "leftPaddle";
	public static readonly string TYPE_RIGHT_PADDLE = "rightPaddle";
	public static readonly string TYPE_RALLY = "rally";
	public static readonly int NUM_SUBRECTS = 5;
	public static readonly Rect FRAME_RALLY;
	public static readonly Rect FRAME_LEFT_PADDLE;
	public static readonly Rect FRAME_RIGHT_PADDLE;

	/* Member Data */
	private string m_type; //domain:{"leftPaddle","rightPaddle","rally"}
	private int m_score;
	private Texture2D m_scoreIcon;
	private Texture2D m_scoreIconAbsent;
	private ColorMap m_colorMap;
	private Rect m_frame;
	private Rect[] m_subRects; //sub-Rect divisions
	private ScoreIconAuto m_scoreIconFSM;
	private byte m_imageBitmask;

	/**
	 * Static constructor.
	 */
	static ScoreRect()
	{
		float Sw = Screen.width;
		float Sh = Screen.height;
		float Fw = Sw / 4F;
		float Fh = Sh / 7F;

		//initialize the static Rects
		FRAME_RALLY = new Rect(Sw/2F-Fw/2F,0,Fw,Fh);
		FRAME_LEFT_PADDLE = new Rect(Sw/4F-Fw/2F,0,Fw,Fh);
		FRAME_RIGHT_PADDLE = new Rect(3F*Sw/4F-Fw/2F,0,Fw,Fh);
	}

	/**
	 * Default constructor.
	 */
	public ScoreRect()
	{
		m_type = ScoreRect.TYPE_UNASSIGNED;
		m_score = -1;
		m_scoreIcon = null;
		m_scoreIconAbsent = null;
		m_colorMap = null;
		m_frame = new Rect ();
		m_subRects = null;
		m_scoreIconFSM = new ScoreIconAuto();
		m_imageBitmask = 0x00;
	}

	/**
	 * Instance constructor.
	 */
	public ScoreRect(string _type_,
	                 string _scoreIconPath_,
	                 string _scoreIconAbsentPath_)
	{
		m_type = _type_;
		m_score = 0;
		m_colorMap = new ColorMap (Application.dataPath+"/Config/hot_colormap_size_20.csv");
		m_subRects = new Rect[NUM_SUBRECTS];
		CalculateRects ();
		LoadScoreIcon (_scoreIconPath_,_scoreIconAbsentPath_);
		m_scoreIconFSM = new ScoreIconAuto();
		m_imageBitmask = 0x00;
	}

	/**
	 * This method calculates the set of Rects used by this object.
	 */
	private void CalculateRects()
	{
		//set the base frame
		if( m_type == TYPE_LEFT_PADDLE )
			m_frame = FRAME_LEFT_PADDLE;
		else if( m_type == TYPE_RIGHT_PADDLE )
			m_frame = FRAME_RIGHT_PADDLE;
		else if( m_type == TYPE_RALLY )
			m_frame = FRAME_RALLY;
		else
			m_frame = new Rect();

		float dw = m_frame.width / NUM_SUBRECTS;
		float dh = m_frame.height;
		float dx = m_frame.x;
		float dy = m_frame.y;

		//set the sub-Rect parameters
		for(int i = 0; i < NUM_SUBRECTS; i++)
			m_subRects [i] = new Rect (dx + i*dw, dy, dw, dh);
	}

	/**
	 * This method loads the image used as the score icon.
	 */
	private void LoadScoreIcon(string _scoreIconPath_, string _scoreIconAbsentPath_)
	{
		m_scoreIcon = (Texture2D)Resources.Load (_scoreIconPath_);
		m_scoreIconAbsent = (Texture2D)Resources.Load (_scoreIconAbsentPath_);
	}

	/**
	 * This method returns the sub-Rect at the specified index.
	 */
	public Rect GetSubRect( int _index_ )
	{
		//if the index is out of range
		if( _index_ < 0 || _index_ >= NUM_SUBRECTS)
			return new Rect();
		//otherwise, return the requested sub-Rect
		else
			return m_subRects[_index_];
	}
	
	/**
	 * This method returns the sub-Rect image at the specified index.
	 */
	public Texture2D GetSubRectImage( int _index_ )
	{
		//if the index is out of range
		if( _index_ < 0 || _index_ >= NUM_SUBRECTS)
			return null;
		//otherwise, return the requested sub-Rect image
		else
		{
			switch( m_imageBitmask )
			{
			case 0x00: return m_scoreIconAbsent;
			case 0x01: return _index_ == 2 ? m_scoreIcon : m_scoreIconAbsent;
			case 0x03: return _index_ == 0 ? m_scoreIconAbsent : m_scoreIcon;
			case 0x07: return m_scoreIcon;
			default: return null;
			}
		}
	}

	/**
	 * This method updates the score.
	 */
	public void UpdateScore( int _newScore_ )
	{
		//update the score
		m_score = _newScore_;

		/** ICON SCORE FINITE STATE MACHINE **/
		//CURRENT STATE: NO POINTS ACQUIRED (i.e., no stars)
		if( m_scoreIconFSM.CurrState == ScoreIconAuto.NO_STARS )
		{
			//update image bitmask
			m_imageBitmask = 0x00;

			//enact transition to the next state
			int nextState = m_score % 4;
			m_scoreIconFSM.Transition( nextState );
		}
		//CURRENT STATE: ONE STAR
		else if( m_scoreIconFSM.CurrState == ScoreIconAuto.ONE_STAR )
		{
			//update image bitmask
			m_imageBitmask = 0x01;
			
			//enact transition to the next state
			int nextState = m_score % 4;
			m_scoreIconFSM.Transition( nextState );
		}
		//CURRENT STATE: TWO STAR
		else if( m_scoreIconFSM.CurrState == ScoreIconAuto.TWO_STARS )
		{
			//update image bitmask
			m_imageBitmask = 0x03;
			
			//enact transition to the next state
			int nextState = m_score % 4;
			m_scoreIconFSM.Transition( nextState );
		}
		//CURRENT STATE: THREE STAR
		else if( m_scoreIconFSM.CurrState == ScoreIconAuto.THREE_STARS )
		{
			//update image bitmask
			m_imageBitmask = 0x07;
			
			//enact transition to the next state
			int nextState = m_score % 4;
			m_scoreIconFSM.Transition( nextState );
		}
	}

	/**
	 * This method returns the numeric score.
	 */
	public int GetScoreNumeric()
	{
		return m_score;
	}

	/**
	 * This method returns the score as a formatted string.
	 */
	public string GetScoreFormatted()
	{
		return FormatString ((m_score / 3).ToString ());
	}

	/**
	 * This method returns the specified string wrapped in rich text format tags.
	 */
	public string FormatString( string s )
	{
		string hexColor = m_colorMap.GetColorHexString (m_score);
		return "<size=50><b><color=#" + hexColor +">" + s + "</color></b></size>";
	}
}

public class ScoreIconAuto : Automaton
{
	/* State Constants */
	public const int NO_STARS = 0;
	public const int ONE_STAR = 1;
	public const int TWO_STARS = 2;
	public const int THREE_STARS = 3;

	/**
	 * Default constructor.
	 */
	public ScoreIconAuto() : base()
	{
		//default transition
		this.Transition (NO_STARS);
	}
}