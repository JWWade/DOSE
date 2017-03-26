using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

public static class JsonUtilities
{
	/**
	 * This function serializes the object provided to a JSON string
	 * and returns the string.
	 */
	public static string SerializeObjectToJSON( System.Object obj)
	{
		return JsonConvert.SerializeObject (obj);
	}
	
	/**
	 * This function deserializes the provided JSON string and returns
	 * an instance of the object serialized.
	 */
	public static System.Object DeserializeObjectFromJSON( string s)
	{
		return JsonConvert.DeserializeObject<System.Object>(s);
	}
	
}