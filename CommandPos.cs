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
	public class CommandPos : IRocketCommand
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
					"pos"
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
				return "pos";
			}
		}

		public string Syntax
		{
			get
			{
				return "pos";
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
			if (command.Length == 0)
			{
				UnturnedChat.Say(uplayer, "/pos a - точка А");
				UnturnedChat.Say(uplayer, "/pos b - точка В");
				UnturnedChat.Say(uplayer, "/pos l - лобби позиция");
				UnturnedChat.Say(uplayer, "/pos 1 - позиция для игроков 1-ой команды");
				UnturnedChat.Say(uplayer, "/pos 2 - позиция для игроков 2-ой команды");
				return;
			}
			if (command[0] == "a")
			{
				Plugin.Instance.Configuration.Instance.PointA.Position = uplayer.Position;
				UnturnedChat.Say(uplayer, "Позиция для точки А установлена!");
			}
			if (command[0] == "b")
            {
				Plugin.Instance.Configuration.Instance.PointB.Position = uplayer.Position;
				UnturnedChat.Say(uplayer, "Позиция для точки B установлена!");
			}
			if (command[0] == "l")
            {
				Plugin.Instance.Configuration.Instance.LobbyPos = uplayer.Position;
				UnturnedChat.Say(uplayer, "Позиция для лобби установлена!");
			}
			if (command[0] == "1")
            {
				DVPos pos = new DVPos()
				{
					TeamName = Plugin.Instance.Configuration.Instance.Team1.Name,
					Position = uplayer.Position
				};
				Plugin.Instance.Configuration.Instance.PositionsSpawn.Add(pos);
				UnturnedChat.Say(uplayer, $"Позиция для спавна игроков команды {Plugin.Instance.Configuration.Instance.Team1.Name} установлена!");
			}
			if (command[0] == "2")
            {
				DVPos pos = new DVPos()
				{
					TeamName = Plugin.Instance.Configuration.Instance.Team2.Name,
					Position = uplayer.Position
				};
				Plugin.Instance.Configuration.Instance.PositionsSpawn.Add(pos);
				UnturnedChat.Say(uplayer, $"Позиция для спавна игроков команды {Plugin.Instance.Configuration.Instance.Team2.Name} установлена!");
			}
			Plugin.Instance.Configuration.Save();
		}
	}
}
