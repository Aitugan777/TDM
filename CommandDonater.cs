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
	public class CommandDonater : IRocketCommand
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
					"donater"
				};
			}
		}

		public AllowedCaller AllowedCaller
		{
			get
			{
				return AllowedCaller.Both;
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
				return "donater";
			}
		}

		public string Syntax
		{
			get
			{
				return "donater";
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
			if (command.Length == 0)
            {
				UnturnedChat.Say(caller, "/donater add nick/csteamID classPermID teamName");
				return;
            }

			UnturnedPlayer checkplayer = UnturnedPlayer.FromName(command[1]);
			ulong SteamID;
			if (checkplayer == null)
			{
				if (!ulong.TryParse(command[1], out SteamID))
				{
					UnturnedChat.Say(caller, Plugin.Instance.Translate("player_not_found", null), Color.red);
					return; // Игрок не найден
				}
			}
			SteamID = (ulong)checkplayer.CSteamID;

			if (command[0] == "add")
            {
				Plugin.Instance.Configuration.Instance.Donaters.Add(new DVDonater() { CSTeamID = SteamID, PermID = command[2], TeamName = command[3] });
				UnturnedChat.Say(caller, "Игроку был успешно выдан класс");
			}
			else if (command[0] == "remove")
            {
				foreach(DVDonater donater in Plugin.Instance.Configuration.Instance.Donaters)
                {
					if (donater.CSTeamID == SteamID && donater.PermID == command[2] && donater.TeamName == command[3])
                    {
						Plugin.Instance.Configuration.Instance.Donaters.Remove(donater);
						UnturnedChat.Say(caller, "С игрока был успешно снят класс");
						break;
                    }
                }
            }
			if (checkplayer != null)
            {
				Plugin.Instance.UpdateClasses(checkplayer);
            }
			Plugin.Instance.Configuration.Save();
		}
	}
}
