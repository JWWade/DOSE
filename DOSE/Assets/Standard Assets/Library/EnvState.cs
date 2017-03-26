using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Newtonsoft;
using Newtonsoft.Json;

[Serializable]
public class Position2D
{
	/* Member Data */
	public float x;
	public float y;
	
	/**
	 * Default constructor.
	 */
	public Position2D()
	{
		x = 0;
		y = 0;
	}
	
	/**
	 * Instance constructor.
	 */
	public Position2D(float _x_, float _y_)
	{
		x = _x_;
		y = _y_;
	}
	
	/**
	 * This method returns the object as a Vector3.
	 */
	public Vector2 ToVector2()
	{
		return new Vector2 (x, y);
	}

	/**
	 * This static function converts a Vector2 to a Position2D.
	 */
	public static Position2D Vector2ToPosition2D(Vector2 v)
	{
		return new Position2D (v.x, v.y);
	}
}

[Serializable]
public class Position3D
{
	/* Member Data */
	public float x;
	public float y;
	public float z;

	/**
	 * Default constructor.
	 */
	public Position3D()
	{
		x = 0;
		y = 0;
		z = 0;
	}

	/**
	 * Instance constructor.
	 */
	public Position3D(float _x_, float _y_, float _z_)
	{
		x = _x_;
		y = _y_;
		z = _z_;
	}

	/**
	 * This method returns the object as a Vector3.
	 */
	public Vector3 ToVector3()
	{
		return new Vector3 (x, y, z);
	}
	
	/**
	 * This static function converts a Vector2 to a Position2D.
	 */
	public static Position3D Vector3ToPosition3D(Vector3 v)
	{
		return new Position3D (v.x, v.y, v.z);
	}
}

[Serializable]
public class EnvState
{
	/* Member Data */
	public int leftScore;
	public int rightScore;
	public Position3D ballOrientation;
	public Position2D ballPos;
	public Position2D agentPos;
	public Position2D humanPos;
	public float leftPaddleLen;
	public float leftPaddleWidth;
	public float rightPaddleLen;
	public float rightPaddleWidth;
	public int sessionState;
	public Match currMatch;
	public string extraInfo;
	
	/**
	 * Default constructor.
	 */
	public EnvState()
	{
		leftScore = 0;
		rightScore = 0;
		ballPos = new Position2D (0, 0);
		agentPos = new Position2D (0, 0);
		humanPos = new Position2D (0, 0);
		sessionState = 0;
		currMatch = new Match ();
		extraInfo = "NULL";
	}
	
	/**
	 * Instance constructor.
	 */
	public EnvState(int _leftScore_,
	                     int _rightScore_,
	                	 Position3D _ballOrientation_,
	                	 Position2D _ballPos_,
	                	 Position2D _rightPos_,
	                	 Position2D _leftPos_,
	                	 float _leftPaddleLen_,
	                	 float _rightPaddleLen_,
	                	 float _leftPaddleWidth_,
	                	 float _rightPaddleWidth_,
	                     int _sessionState_,
	                     Match _currMatch_,
	                	 string _extraInfo_ = "NULL")
	{
		leftScore = _leftScore_;
		rightScore = _rightScore_;
		ballOrientation = _ballOrientation_;
		ballPos = _ballPos_;
		agentPos = _rightPos_;
		humanPos = _leftPos_;
		leftPaddleLen = _leftPaddleLen_;
		rightPaddleLen = _rightPaddleLen_;
		leftPaddleWidth = _leftPaddleWidth_;
		rightPaddleWidth = _rightPaddleWidth_;
		sessionState = _sessionState_;
		currMatch = _currMatch_;
		extraInfo = _extraInfo_;
	}
	
	/**
	 * This method returns a nicely formatted string representation of the object.
	 */
	public override string ToString ()
	{
		return string.Format ("[EnvState]");
	}
	
	/**
	 * This method returns the serialized JSON representation of the object.
	 */
	public string ToJsonString()
	{
		string jsonString = JsonConvert.SerializeObject (this, Formatting.Indented, GeneralUtils.jss);
		//Debug.Log (jsonString);
		return jsonString;
	}
}