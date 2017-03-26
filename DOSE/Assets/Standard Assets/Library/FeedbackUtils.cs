using System; //for random number generator
using UnityEngine;
using System.Collections;
using System.Collections.Generic; //for hashtable/dictionary

public static class FeedbackUtils
{
	public static readonly Dictionary<string, string> sMap;
	public static readonly Dictionary<string, AudioClip> aMap;
	private static System.Random rndm;
	public static readonly Texture negFeedbackImg;
	public static readonly Texture posFeedbackImg;

	/**
	 * Static constructor.
	 */
	static FeedbackUtils()
	{
		//initialize the dictionaries
		sMap = new Dictionary<string, string> ();
		aMap = new Dictionary<string, AudioClip> ();

		//initialize the Random object
		rndm = new System.Random ();

		// format of key is "Outcome_MessageNumber" => "O_M"
		sMap.Add ("S_1", "Great Job!");
		sMap.Add ("S_2", "Awesome Job!");
		sMap.Add ("S_3", "Really Nice Job!");

		sMap.Add ("F_1", "Good luck on the next one.");
		sMap.Add ("F_2", "Better luck next time.");
		sMap.Add ("F_3", "Almost. Try again.");

		sMap.Add ("D", "DRAW!");

		// store audio
		aMap.Add ("S_1", (AudioClip)Resources.Load ("Great Job - Laura", typeof(AudioClip)));
		aMap.Add ("S_2", (AudioClip)Resources.Load ("Awesome Job - Laura", typeof(AudioClip)));
		aMap.Add ("S_3", (AudioClip)Resources.Load ("Really Nice Job - Laura", typeof(AudioClip)));
		
		aMap.Add ("F_1", (AudioClip)Resources.Load ("Good luck on the next one - Laura", typeof(AudioClip)));
		aMap.Add ("F_2", (AudioClip)Resources.Load ("Better luck next time - Laura", typeof(AudioClip)));
		aMap.Add ("F_3", (AudioClip)Resources.Load ("Almost try again - Laura", typeof(AudioClip)));
		
		negFeedbackImg = Resources.Load <Texture> ("negFeedbackImg");
		posFeedbackImg = Resources.Load <Texture> ("posFeedbackImg");
	}

	/**
	 * This method returns a random number for selecting an arbitrary feedback.
	 */
	public static int GetRandFeedbackIndex()
	{
		return rndm.Next ( 1,4 );
	}

	/**
	 * This method returns the feedback string corresponding to the supplied
	 * outcome parameter.
	 */
	public static string GetFeedbackString( string outcome, int index )
	{
		//create key
		string key = "";
		if(outcome == "D") //if draw
			key = "D";
		else
			key = outcome + "_" + index.ToString ();

		if( sMap.ContainsKey(key) )
			return sMap[key];
		else
			return "No feedback for outcome \"" + outcome + "\"";
	}

	/**
	 * This method returns the feedback audio corresponding to the supplied
	 * outcome parameter.
	 */
	public static AudioClip GetFeedbackAudio( string outcome, int index )
	{
		if(outcome == "D") //if draw
			return null;

		//create key
		string key = outcome + "_" + index.ToString ();
		
		if( sMap.ContainsKey(key) )
			return aMap[key];
		else
			return null;
	}

	/**
	 * This method returns the feedback image corresponding to the supplied
	 * outcome parameter.
	 */
	public static Texture GetFeedbackImage( string outcome )
	{
		//return positive image if draw or success
		if(outcome == "D" || outcome == "S")
			return posFeedbackImg;
		//otherwise, return negative image for failure
		else
			return negFeedbackImg;
	}
}
















