using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Newtonsoft.Json;

[Serializable]
public class Configuration
{
	/* Static Members */
	public static readonly byte HHvA_1PC = 0; //2 humans vs AI on 1 pc
	public static readonly byte HvH_1PC = 1;  //human vs human on 1 pc
	public static readonly byte HvA_1PC = 2;  //human vs AI on 1 pc
	public static readonly byte HHvA_2PC = 3; //2 humans vs AI on 2 pc
	public static readonly byte HvH_2PC = 4;  //human vs human on 2 pc
	public static readonly byte Rally_1PC = 5;//rally on 1 pc
	public static readonly byte Rally_2PC = 6;//rally on 2 pc
	public static readonly byte INVALID = 20;
	public static readonly int ONE_PC = 1;
	public static readonly int TWO_PC = 2;
	public static readonly bool THERAPIST_PLAYING = true;
	public static readonly bool THERAPIST_NOT_PLAYING = false;
	public static readonly bool COLLABORATIVE = true;
	public static readonly bool NOT_COLLABORATIVE = false;
	public static readonly bool RALLY = true;
	public static readonly bool NOT_RALLY = false;

	/* Member Data */
	public int numPCs;
	public bool therapistPlaying;
	public bool collaborative;
	public bool rally;
	public byte shorthandConfig; //of form "HHvA_1PC", "HvH_1PC", etc.

	/**
	 * Default constructor.
	 */
	public Configuration()
	{
		numPCs = 0;
		therapistPlaying = false;
		collaborative = false;
		rally = false;
		shorthandConfig = Configuration.INVALID;
	}

	/**
	 * Instance constructor.
	 */
	public Configuration( int _numPCs_, bool _therapistPlaying_, bool _collaborative_, bool _rally_ )
	{
		numPCs = _numPCs_;
		therapistPlaying = _therapistPlaying_;
		collaborative = _collaborative_;
		rally = _rally_;
		shorthandConfig = GetShorthandConfig ();
	}

	/**
	 * This method returns a nicley formatted string representation of the object.
	 */
	public override string ToString ()
	{
		string s = "[";

		s += "shorthand=" + shorthandConfig.ToString ();
		s += ",numPCs=" + numPCs.ToString ();
		s += ",therapistPlaying=" + therapistPlaying.ToString ();
		s += ",collaborative=" + collaborative.ToString ();
		s += ",rally=" + rally.ToString () + "]";

		return s;
	}

	/**
	 * This method returns the shorthand notation of the Configuration.
	 */
	private byte GetShorthandConfig()
	{
		if( numPCs == ONE_PC ) {
			if(!rally){
				if(collaborative) {
					if( therapistPlaying )
						return Configuration.HHvA_1PC;
					else
						return Configuration.INVALID;
				} else {
					if( therapistPlaying )
						return Configuration.HvH_1PC;
					else
						return Configuration.HvA_1PC;
				}
			} else {
				return Configuration.Rally_1PC;
			}
		} else if( numPCs == TWO_PC ) {
			if(!rally) {
				if(collaborative) {
					if( therapistPlaying )
						return Configuration.HHvA_2PC;
					else
						return Configuration.INVALID;
				} else {
					if( therapistPlaying )
						return Configuration.HvH_2PC;
					else
						return Configuration.INVALID;
				}
			} else {
				return Configuration.Rally_2PC;
			}
		} else
			return Configuration.INVALID;
	}
}