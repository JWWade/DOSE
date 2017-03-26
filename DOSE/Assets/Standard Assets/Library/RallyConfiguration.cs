using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Newtonsoft.Json;

[Serializable]
public class RallyConfiguration
{
	/* Member Data */
	public float m_LMax;
	public float m_LMin;
	public float m_VMax;

	/**
	 * Default constructor.
	 */
	public RallyConfiguration()
	{
		m_LMax = 1.5F;
		m_LMin = 0.5F;
		m_VMax = 20F;
	}

	/**
	 * Instance constructor.
	 */
	public RallyConfiguration( float _LMax_, float _LMin_, float _VMax_ )
	{
		m_LMax = _LMax_;
		m_LMin = _LMin_;
		m_VMax = _VMax_;
	}

	/**
	 * Instance constructor.
	 * Creates a new instance from the specified JSON file.
	 */
	public RallyConfiguration( string _jsonFile_ )
	{
		string jsonString = GeneralUtils.ReadContentFromFile (_jsonFile_);
		RallyConfiguration rcfg = JsonConvert.DeserializeObject<RallyConfiguration> (jsonString);
		this.m_LMax = rcfg.m_LMax;
		this.m_LMin = rcfg.m_LMin;
		this.m_VMax = rcfg.m_VMax;
	}

	/**
	 * This method returns a nicely formatted string representation of the object.
	 */
	public override string ToString ()
	{
		return string.Format ("[RallyConfiguration: m_LMax={0}, m_LMin={1}, m_VMax={2}]", m_LMax, m_LMin, m_VMax);
	}

	/**
	 * This method returns the JSON serialized string representation of the object.
	 */
	public string ToJsonString()
	{
		return JsonConvert.SerializeObject (this);
	}

	/**
	 * Accessors.
	 */
	public float GetLMax()
	{
		return m_LMax;
	}
	public float GetLMin()
	{
		return m_LMin;
	}
	public float GetVMax()
	{
		return m_VMax;
	}
}