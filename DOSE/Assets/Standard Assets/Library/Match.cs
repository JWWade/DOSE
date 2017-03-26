using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Newtonsoft.Json;

[Serializable]
public class Match
{
	/* Static Members */
	public static readonly byte EASY = 0;
	public static readonly byte MEDIUM = 1;
	public static readonly byte HARD = 2;
	public static readonly byte INVALID = 20;

	/* Member Data */
	public Configuration configuration;
	public byte difficulty;

	/**
	 * Default constructor.
	 */
	public Match()
	{
		configuration = new Configuration ();
		difficulty = Match.INVALID;
	}

	/**
	 * Instance constructor.
	 */
	public Match( Configuration _configuration_, byte _difficulty_ )
	{
		configuration = _configuration_;
		difficulty = _difficulty_;
	}

	/**
	 * This method returns a nicely formatted string representation of the object.
	 */
	public override string ToString ()
	{
		string s = "[";

		s += "difficulty=" + difficulty.ToString ();
		s += ",config=" + configuration.ToString () + "]";

		return s;
	}

}