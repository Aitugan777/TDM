using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace DVPlugin
{
	public class DVTeam
	{
		[XmlAttribute("Name")]
		public string Name;

		[XmlAttribute("URLImg")]
		public string URLImg;

		[XmlAttribute("Color")]
		public string Color;

		[XmlAttribute("SteamGroupID")]
		public ulong SteamGroupID;





		[XmlAttribute("PermissionID1")]
		public string PermissionID1;

		[XmlAttribute("Permission1")]
		public string Permission1;

		[XmlAttribute("URLImg1")]
		public string URLImg1;

		[XmlAttribute("PermissionID2")]
		public string PermissionID2;

		[XmlAttribute("Permission2")]
		public string Permission2;

		[XmlAttribute("URLImg2")]
		public string URLImg2;

	}

	public class DVPoint
	{
		[XmlAttribute("Name")]
		public string Name;

		[XmlAttribute("Captured")]
		public string Captured;

		[XmlAttribute("CapturProgress")]
		public string CapturProgress;

		[XmlAttribute("SecOnCapture")]
		public int SecOnCapture;

		[XmlAttribute("Radius")]
		public float Radius;

		[XmlElement("Position")]
		public Vector3 Position;
	}

	public class DVPos
    {
		[XmlAttribute("TeamName")]
		public string TeamName;

		[XmlElement("Position")]
		public Vector3 Position;
	}

	public class DVDonater
    {
		[XmlAttribute("CSteamID")]
		public ulong CSTeamID;
		[XmlAttribute("PermID")]
		public string PermID;
		[XmlAttribute("TeamName")]
		public string TeamName;
	}
}
