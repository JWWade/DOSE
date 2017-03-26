using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Newtonsoft.Json;

[Serializable]
public class InputData
{
	/* Member Data */
	public byte humanID1;
	public byte humanID2;
	public byte inputType;
	public float inputVal1;
	public float inputVal2;

	/**
	 * Default constructor.
	 */
	public InputData()
	{
		humanID1 = GeneralUtils.UNASSIGNED;
		humanID2 = GeneralUtils.UNASSIGNED;
		inputType = GeneralUtils.UNASSIGNED;
		inputVal1 = 0;
		inputVal2 = 0;
	}

	/**
	 * Instance constructor.
	 */
	public InputData( byte _humanID1_, byte _inputType_, float _inputVal1_ )
	{
		humanID1 = _humanID1_;
		humanID2 = 0;
		inputType = _inputType_;
		inputVal1 = _inputVal1_;
		inputVal2 = 0;
	}

	/**
	 * Instance constructor.
	 */
	public InputData( byte _humanID1_, byte _humanID2_, byte _inputType_, float _inputVal1_, float _inputVal2_ )
	{
		humanID1 = _humanID1_;
		humanID2 = _humanID2_;
		inputType = _inputType_;
		inputVal1 = _inputVal1_;
		inputVal2 = _inputVal2_;
	}

	/**
	 * This method returns a nicely formatted string representation of the object.
	 */
	public override string ToString ()
	{
		string s = "[";

		s += "humanID=" + humanID1.ToString () + ",";
		s += "inputType=" + inputType.ToString () + ",";
		s += "inputVal=" + inputVal1.ToString () + "]";

		return s;
	}

	/**
	 * This method returns the serialized JSON representation of the object.
	 */
	public string ToJsonString()
	{
		return JsonConvert.SerializeObject (this, Formatting.Indented, GeneralUtils.jss);
	}
}