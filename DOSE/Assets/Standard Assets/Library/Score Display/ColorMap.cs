using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ColorMap
{
	/* Member Data */
	private List<Color> m_map;
	private int m_size;

	/**
	 * Default constructor.
	 */
	public ColorMap()
	{
		m_map = new List<Color> ();
		m_size = 0;
	}

	/**
	 * Instance constructor.
	 */
	public ColorMap( string _filePath_ )
	{
		//extract the data from the specified file
		string[] rows = GeneralUtils.ReadContentFromFile (_filePath_).Replace("\r","").Split('\n');
		m_map = new List<Color> ();
		m_size = 0;

		//iterate over each row and add a new Color to the ColorMap
		foreach( string row in rows )
		{
			//extract the R,G,B components from the row
			string[] rowComponents = row.Split(',');
			if(rowComponents.Length < 3)
				break;
			float r = Single.Parse (rowComponents[0]);
			float g = Single.Parse (rowComponents[1]);
			float b = Single.Parse (rowComponents[2]);

			//create the new Color and add it to the map
			Color c = new Color(r,g,b);
			m_map.Add( c );
		}
	
		//set the size
		m_size = m_map.Count;
	}

	/**
	 * This method returns the Color object at the specified instance.
	 */
	public Color GetColor( int _index_ )
	{
		//if the index is less than minimum index, return first color in map
		if( _index_ < 5 )
			return m_map[5];
		//if the index is greater than the max index, return last color in map
		if( _index_ >= m_size )
			return m_map[m_size-1];
		//otherwise, return the requested Color object
		else
			return m_map[_index_];
	}

	/**
	 * This method returns the Color object as a hex string at the specified index.
	 */
	public string GetColorHexString( int _index_ )
	{
		//get the Color object
		Color32 c = (Color32)this.GetColor (_index_);

		//convert the color to hex string
		string hexString = c.r.ToString ("X2") + c.g.ToString ("X2") + c.b.ToString ("X2");
		return hexString;
	}

	/**
	 * This method returns the size of the colormap.
	 */
	public int GetSize()
	{
		return m_size;
	}
}