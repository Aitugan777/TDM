using System;
using System.Collections.Generic;
using System.Reflection;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace DVPlugin
{
	public class CommandLang : IRocketCommand
	{
		public bool AllowFromConsole
		{
			get
			{
				return false;
			}
		}

		public List<string> Permissions
		{
			get
			{
				return new List<string>
				{
					"lang"
				};
			}
		}

		public AllowedCaller AllowedCaller
		{
			get
			{
				return AllowedCaller.Player;
			}
		}

		public bool RunFromConsole
		{
			get
			{
				return false;
			}
		}

		public string Name
		{
			get
			{
				return "lang";
			}
		}

		public string Syntax
		{
			get
			{
				return "lang";
			}
		}

		public string Help
		{
			get
			{
				return "";
			}
		}

		public List<string> Aliases
		{
			get
			{
				return new List<string>();
			}
		}

		public void Execute(IRocketPlayer caller, string[] command)
		{
			UnturnedPlayer uplayer = (UnturnedPlayer)caller;

			if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.Team1.Permission1))
				R.Permissions.RemovePlayerFromGroup(Plugin.Instance.Configuration.Instance.Team1.PermissionID1, (IRocketPlayer)uplayer);
			if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.Team1.Permission2))
				R.Permissions.RemovePlayerFromGroup(Plugin.Instance.Configuration.Instance.Team1.PermissionID2, (IRocketPlayer)uplayer);
			if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.Team2.Permission1))
				R.Permissions.RemovePlayerFromGroup(Plugin.Instance.Configuration.Instance.Team2.PermissionID1, (IRocketPlayer)uplayer);
			if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.Team2.Permission2))
				R.Permissions.RemovePlayerFromGroup(Plugin.Instance.Configuration.Instance.Team2.PermissionID2, (IRocketPlayer)uplayer);
			if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.EngPermission))
				R.Permissions.RemovePlayerFromGroup(Plugin.Instance.Configuration.Instance.EngPermissionID, (IRocketPlayer)uplayer);
			if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.RusPermission))
				R.Permissions.RemovePlayerFromGroup(Plugin.Instance.Configuration.Instance.RusPermissionID, (IRocketPlayer)uplayer);
			if (Plugin.Instance.TeamChoosed.ContainsKey(uplayer.CSteamID))
				Plugin.Instance.TeamChoosed.Remove(uplayer.CSteamID);
			Plugin.Instance.OpenChoseUI(uplayer);
			uplayer.Teleport(Plugin.Instance.Configuration.Instance.LobbyPos, uplayer.Rotation);
		}
	}
}
