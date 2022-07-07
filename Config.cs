using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Rocket.API;
using UnityEngine;

namespace DVPlugin
{
	public class Config : IRocketPluginConfiguration, IDefaultable
	{
		public void LoadDefaults()
		{
			this.Team1 = new DVTeam()
			{
				Name = "Axiss",
				Permission1 = "team.russia",
				PermissionID1 = "russia",
				URLImg1 = "russiaURL",
				Permission2 = "team.usa",
				PermissionID2 = "usa",
				URLImg2 = "usaURL",
				SteamGroupID = 1,
				Color = "red",
				URLImg = "AxisURL"
			};
			this.Team2 = new DVTeam()
			{
				Name = "Allies",
				Permission1 = "team.germany",
				PermissionID1 = "germany",
				URLImg1 = "germanyURL",
				Permission2 = "team.japan",
				PermissionID2 = "japan",
				URLImg2 = "japanURL",
				SteamGroupID = 2,
				Color = "green",
				URLImg = "AlliesURL"
			};
			this.PointA = new DVPoint()
			{
				SecOnCapture = 30,
				Radius = 10 
			};
			this.PointB = new DVPoint()
			{
				SecOnCapture = 60,
				Radius = 20
			};
			this.LobbyPos = new Vector3();
			this.IDsBuildings = new List<ushort>()
			{
				1, 2, 3, 4, 5
			};
		}

		public bool LoadWorkshop = true;
		public ushort ChoseUI = 9410;
		public ushort StatusUI = 9411;
		public ushort WinTable = 9412;
		public string CommandOnUpdateClass = "class";

		public byte MinCountPlayers = 3;
		public uint ExpReward = 1000;
		public uint UconomyMonyReward = 5000;
		public uint UconomyMonyRewardKillerSkull = 100;
		public uint UconomyMonyRewardKillerBody = 70;

		public int TimeRound = 1800;
		public int TimeLobby = 60;
		public int ClearItemsSeconds = 1;
		public float TPY = 0.5f;

		public bool ClearVehicles = true;
		[XmlArray("ID's Buildings"), XmlArrayItem("ID")]
		public List<ushort> IDsBuildings;

		public string EngPermissionID = "English";
		public string EngPermission = "eng";
		public string EngURLImg = "EngURLImg";
		public string RusPermissionID = "Russia";
		public string RusPermission = "rus";
		public string RusURLImg = "RusURLImg";

		public DVTeam Team1;
		public DVTeam Team2;

		public DVPoint PointA;
		public DVPoint PointB;

		[XmlElement("LobbyPosition")]
		public Vector3 LobbyPos;

		[XmlArray("Positions"), XmlArrayItem("Position")]
		public List<DVPos> PositionsSpawn = new List<DVPos>();
		[XmlArray("Donaters"), XmlArrayItem("Donate")]
		public List<DVDonater> Donaters = new List<DVDonater>();

	}
}
