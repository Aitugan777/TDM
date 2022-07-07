using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace DVPlugin
{
	public class Plugin : RocketPlugin<Config>
	{
		public static Plugin Instance;
		protected override void Load()
		{
			Plugin.Instance = this;
            U.Events.OnPlayerConnected += onPlayerConnected;
            U.Events.OnPlayerDisconnected += onPlayerDisconnected;
			EffectManager.onEffectButtonClicked += onEffectButtonClicked;
            UnturnedPlayerEvents.OnPlayerRevive += onPlayeRevive;
            UnturnedPlayerEvents.OnPlayerDeath += onPlayerDeath; 
            UnturnedPlayerEvents.OnPlayerInventoryRemoved += onInventoryRemoved;

			SecondsClearItems = base.Configuration.Instance.ClearItemsSeconds;

			CapturePoint.TimeLobby = Plugin.Instance.Configuration.Instance.TimeLobby;

			CapturePoint.PointA = new DVPoint()
			{
				Position = base.Configuration.Instance.PointA.Position,
				Radius = base.Configuration.Instance.PointA.Radius
			};
			CapturePoint.PointB = new DVPoint()
			{
				Position = base.Configuration.Instance.PointB.Position,
				Radius = base.Configuration.Instance.PointB.Radius
			};

			CapturePoint.StatusUI = Plugin.Instance.Configuration.Instance.StatusUI;

			Provider.modeConfigData.Gameplay.Timer_Leave_Group = 0U;

			if (base.Configuration.Instance.LoadWorkshop)
				WorkshopDownloadConfig.getOrLoad().File_IDs.Add(2678264907);
			isTeamCreated = false;

			Console.WriteLine("           _______________________________           ", Color.cyan);
			Console.WriteLine("----------|                               |----------", Color.cyan);
			Console.WriteLine("----------|          TDM Loaded           |----------", Color.cyan);
			Console.WriteLine("----------|        Plugin by Aituk        |----------", Color.cyan);
			Console.WriteLine("----------|  https://vk.com/aitukirving   |----------", Color.cyan);
			Console.WriteLine("----------|_______________________________|----------", Color.cyan);
		}

        private void onInventoryRemoved(UnturnedPlayer uplayer, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
			ItemManager.askClearAllItems();
		}

        private void onPlayerDeath(UnturnedPlayer died, EDeathCause cause, ELimb limb, CSteamID murderer)
		{
			ClearInventory(died);
			UnturnedPlayer killer = UnturnedPlayer.FromCSteamID(murderer);
			if (killer == null && died == null)
				return;
			if (killer.CSteamID == died.CSteamID)
				return;
			if (cause == EDeathCause.SUICIDE)
				return;
            try
            {
				if (limb == ELimb.SKULL)
					R.Commands.Execute(new ConsolePlayer(), "pay " + killer.CSteamID + " " + Plugin.Instance.Configuration.Instance.UconomyMonyRewardKillerSkull);
                else
					R.Commands.Execute(new ConsolePlayer(), "pay " + killer.CSteamID + " " + Plugin.Instance.Configuration.Instance.UconomyMonyRewardKillerBody);
			}
            catch { }
        }

        private void onPlayeRevive(UnturnedPlayer uplayer, Vector3 position, byte angle)
		{
			TeleportPlayers.Add(uplayer.CSteamID, 2);
		}

        public bool isTeamCreated;

		public Dictionary<CSteamID, int> GodPlayers = new Dictionary<CSteamID, int>();

		public Dictionary<CSteamID, int> TeleportPlayers = new Dictionary<CSteamID, int>();

		public Dictionary<CSteamID, int> TeamChoosed = new Dictionary<CSteamID, int>();

		public void FixedUpdate()
        {
			Timer();
        }

		private DateTime lastCalled = DateTime.Now;
		private int SecondsClearItems = 0;

		public void Timer()
		{
			if ((DateTime.Now - this.lastCalled).TotalSeconds > 1.0)
			{
				this.lastCalled = DateTime.Now;
				CapturePoint.Update();
				try
				{
					foreach (SteamPlayer steamPlayer in Provider.clients)
					{
						UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);
						if (TeleportPlayers.ContainsKey(uplayer.CSteamID))
						{
							if (TeleportPlayers[uplayer.CSteamID] == 0)
							{
								RandPosTeleport(uplayer);
								TeleportPlayers.Remove(uplayer.CSteamID);
							}
							else
							{
								TeleportPlayers[uplayer.CSteamID]--;
							}
						}
					}
				}
				catch { }
				if (SecondsClearItems <= 0)
				{
					ItemManager.askClearAllItems();
					SecondsClearItems = base.Configuration.Instance.ClearItemsSeconds;
				}
				else
					SecondsClearItems--;
			}
		}

		private void onPlayerDisconnected(UnturnedPlayer uplayer)
        {
			R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team1.PermissionID1, (IRocketPlayer)uplayer);
			R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team1.PermissionID2, (IRocketPlayer)uplayer);
			R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team2.PermissionID1, (IRocketPlayer)uplayer);
			R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team2.PermissionID2, (IRocketPlayer)uplayer);

			GroupManager.requestGroupExit(uplayer.Player);
		}

		private void onEffectButtonClicked(Player player, string buttonName)
		{
			UnturnedPlayer uplayer = UnturnedPlayer.FromPlayer(player);


			if (!uplayer.HasPermission(base.Configuration.Instance.EngPermission) && !uplayer.HasPermission(base.Configuration.Instance.RusPermission))
			{
				if (buttonName == "choose1")
				{
					R.Permissions.AddPlayerToGroup(base.Configuration.Instance.EngPermissionID, (IRocketPlayer)uplayer);
				}
				else if (buttonName == "choose2")
				{
					R.Permissions.AddPlayerToGroup(base.Configuration.Instance.RusPermissionID, (IRocketPlayer)uplayer);
				}
				OpenChoseUI(uplayer);
				return;
			}
			else if (!TeamChoosed.ContainsKey(uplayer.CSteamID))
			{

				if (buttonName == "choose1")
				{
					if (MembersCount(base.Configuration.Instance.Team1) > MembersCount(base.Configuration.Instance.Team2))
					{
						if (uplayer.HasPermission(base.Configuration.Instance.RusPermission))
							UnturnedChat.Say(uplayer, Translate("ru_no_balance", null), Color.red);
						if (uplayer.HasPermission(base.Configuration.Instance.EngPermission))
							UnturnedChat.Say(uplayer, Translate("eng_no_balance", null), Color.red);
						return;
					}
					TeamChoosed.Add(uplayer.CSteamID, 1);
				}
				else if (buttonName == "choose2")
				{

					if (MembersCount(base.Configuration.Instance.Team2) > MembersCount(base.Configuration.Instance.Team1))
					{
						if (uplayer.HasPermission(base.Configuration.Instance.RusPermission))
							UnturnedChat.Say(uplayer, Translate("ru_no_balance", null), Color.red);
						if (uplayer.HasPermission(base.Configuration.Instance.EngPermission))
							UnturnedChat.Say(uplayer, Translate("eng_no_balance", null), Color.red);
						return;
					}
					TeamChoosed.Add(uplayer.CSteamID, 2);
				}
				OpenChoseUI(uplayer);
				return;
			}
			else if (TeamChoosed[uplayer.CSteamID] == 1)
            {

				if (buttonName == "choose1")
				{

					if (uplayer.HasPermission(base.Configuration.Instance.Team2.Permission1))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team2.PermissionID1, (IRocketPlayer)uplayer);
					if (uplayer.HasPermission(base.Configuration.Instance.Team2.Permission2))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team2.PermissionID2, (IRocketPlayer)uplayer);
					if (uplayer.HasPermission(base.Configuration.Instance.Team1.Permission2))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team1.PermissionID2, (IRocketPlayer)uplayer);

					R.Permissions.AddPlayerToGroup(base.Configuration.Instance.Team1.PermissionID1, (IRocketPlayer)uplayer);
					player.quests.ServerAssignToGroup(new CSteamID(base.Configuration.Instance.Team1.SteamGroupID), EPlayerGroupRank.MEMBER, true);
					if (uplayer.HasPermission(base.Configuration.Instance.RusPermission))
						UnturnedChat.Say(uplayer, Translate("ru_succes_join", null));
					if (uplayer.HasPermission(base.Configuration.Instance.EngPermission))
						UnturnedChat.Say(uplayer, Translate("eng_succes_join", null));
					RandPosTeleport(uplayer);
					ClearInventory(uplayer);

					EffectManager.askEffectClearByID(base.Configuration.Instance.ChoseUI, uplayer.CSteamID);
					if (base.Configuration.Instance.CommandOnUpdateClass != "")
						uplayer.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
					UpdateClasses(uplayer);
				}

				if (buttonName == "choose2")
				{
					if (uplayer.HasPermission(base.Configuration.Instance.Team2.Permission1))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team2.PermissionID1, (IRocketPlayer)uplayer);
					if (uplayer.HasPermission(base.Configuration.Instance.Team2.Permission2))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team2.PermissionID2, (IRocketPlayer)uplayer);
					if (uplayer.HasPermission(base.Configuration.Instance.Team1.Permission2))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team1.PermissionID1, (IRocketPlayer)uplayer);

					R.Permissions.AddPlayerToGroup(base.Configuration.Instance.Team1.PermissionID2, (IRocketPlayer)uplayer);
					player.quests.ServerAssignToGroup(new CSteamID(base.Configuration.Instance.Team1.SteamGroupID), EPlayerGroupRank.MEMBER, true);
					if (uplayer.HasPermission(base.Configuration.Instance.RusPermission))
						UnturnedChat.Say(uplayer, Translate("ru_succes_join", null));
					if (uplayer.HasPermission(base.Configuration.Instance.EngPermission))
						UnturnedChat.Say(uplayer, Translate("eng_succes_join", null));
					RandPosTeleport(uplayer);
					ClearInventory(uplayer);

					EffectManager.askEffectClearByID(base.Configuration.Instance.ChoseUI, uplayer.CSteamID);
					if (base.Configuration.Instance.CommandOnUpdateClass != "")
						uplayer.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
					UpdateClasses(uplayer);
				}
			}
			else if (TeamChoosed[uplayer.CSteamID] == 2)
            {

				if (buttonName == "choose1")
				{

					if (uplayer.HasPermission(base.Configuration.Instance.Team1.Permission1))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team1.PermissionID1, (IRocketPlayer)uplayer);
					if (uplayer.HasPermission(base.Configuration.Instance.Team1.Permission2))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team1.PermissionID2, (IRocketPlayer)uplayer);
					if (uplayer.HasPermission(base.Configuration.Instance.Team2.Permission1))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team2.PermissionID1, (IRocketPlayer)uplayer);

					R.Permissions.AddPlayerToGroup(base.Configuration.Instance.Team2.PermissionID1, (IRocketPlayer)uplayer);
					player.quests.ServerAssignToGroup(new CSteamID(base.Configuration.Instance.Team2.SteamGroupID), EPlayerGroupRank.MEMBER, true);
					if (uplayer.HasPermission(base.Configuration.Instance.RusPermission))
						UnturnedChat.Say(uplayer, Translate("ru_succes_join", null));
					if (uplayer.HasPermission(base.Configuration.Instance.EngPermission))
						UnturnedChat.Say(uplayer, Translate("eng_succes_join", null));
					RandPosTeleport(uplayer);
					ClearInventory(uplayer);

					EffectManager.askEffectClearByID(base.Configuration.Instance.ChoseUI, uplayer.CSteamID);
					if (base.Configuration.Instance.CommandOnUpdateClass != "")
						uplayer.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
					UpdateClasses(uplayer);
				}

				if (buttonName == "choose2")
				{
					if (uplayer.HasPermission(base.Configuration.Instance.Team1.Permission1))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team1.PermissionID1, (IRocketPlayer)uplayer);
					if (uplayer.HasPermission(base.Configuration.Instance.Team1.Permission2))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team1.PermissionID2, (IRocketPlayer)uplayer);
					if (uplayer.HasPermission(base.Configuration.Instance.Team2.Permission1))
						R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team2.PermissionID2, (IRocketPlayer)uplayer);

					R.Permissions.AddPlayerToGroup(base.Configuration.Instance.Team2.PermissionID2, (IRocketPlayer)uplayer);
					player.quests.ServerAssignToGroup(new CSteamID(base.Configuration.Instance.Team2.SteamGroupID), EPlayerGroupRank.MEMBER, true);
					if (uplayer.HasPermission(base.Configuration.Instance.RusPermission))
						UnturnedChat.Say(uplayer, Translate("ru_succes_join", null));
					if (uplayer.HasPermission(base.Configuration.Instance.EngPermission))
						UnturnedChat.Say(uplayer, Translate("eng_succes_join", null));
					RandPosTeleport(uplayer);
					ClearInventory(uplayer);
					EffectManager.askEffectClearByID(base.Configuration.Instance.ChoseUI, uplayer.CSteamID);
					if (base.Configuration.Instance.CommandOnUpdateClass != "")
						uplayer.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
					UpdateClasses(uplayer);
				}
			}
		}

		public void UpdateClasses(UnturnedPlayer uplayer)
        {
			foreach(DVDonater donater in base.Configuration.Instance.Donaters)
            {
				if (donater.CSTeamID == (ulong)uplayer.CSteamID)
				{
					R.Permissions.RemovePlayerFromGroup(donater.PermID, (IRocketPlayer)uplayer);
				}
			}
			foreach (DVDonater donater in base.Configuration.Instance.Donaters)
			{
				if (donater.CSTeamID == (ulong)uplayer.CSteamID && GetPlayerTeam(uplayer).Name == donater.TeamName)
				{
					R.Permissions.AddPlayerToGroup(donater.PermID, (IRocketPlayer)uplayer);
				}
			}

			if (base.Configuration.Instance.CommandOnUpdateClass != "")
            {
				R.Commands.Execute(uplayer, base.Configuration.Instance.CommandOnUpdateClass);
			}
		}

		public void ClearMap()
        {
			if (base.Configuration.Instance.ClearVehicles)
			{
				while(VehicleManager.vehicles.Count > 0)
                {
					VehicleManager.askVehicleDestroy(VehicleManager.vehicles[0]);
				}
            }

			int upperBound;
			int upperBound2;
			bool clearedStr = false;
			bool clearedBar = false;
			if (StructureManager.regions != null)
			{
				StructureRegion[,] regions = StructureManager.regions;
				upperBound = regions.GetUpperBound(0);
				upperBound2 = regions.GetUpperBound(1);
				while (!clearedStr)
				{
					try
					{
						for (int i = regions.GetLowerBound(0); i <= upperBound; i++)
						{
							for (int j = regions.GetLowerBound(1); j <= upperBound2; j++)
							{
								StructureRegion structureRegion = regions[i, j];
								if (structureRegion != null)
								{
									int indx = 0;
									foreach (StructureData structureData in structureRegion.structures)
									{
										if (((structureData != null) ? structureData.structure : null) != null)
										{
											ItemStructureAsset itemStructureAsset = (ItemStructureAsset)Assets.find(EAssetType.ITEM, structureData.structure.id);
											if (itemStructureAsset != null)
											{
												if (!isCfgBuilding(structureData.structure.id))
												{
													StructureManager.destroyStructure(structureRegion, (byte)i, (byte)j, (ushort)indx, structureData.point);
												}
											}
										}
										indx++;
									}
								}
							}
						}
						clearedStr = true;
					}
					catch { }
				}
			}
			if (BarricadeManager.regions == null)
			{
				return;
			}
			BarricadeRegion[,] regions2 = BarricadeManager.regions;
			upperBound2 = regions2.GetUpperBound(0);
			upperBound = regions2.GetUpperBound(1);
			while (!clearedBar)
			{
				try
				{
					for (int i = regions2.GetLowerBound(0); i <= upperBound2; i++)
					{
						for (int j = regions2.GetLowerBound(1); j <= upperBound; j++)
						{
							BarricadeRegion barricadeRegion = regions2[i, j];
							if (barricadeRegion != null)
							{
								int indx = 0;
								foreach (BarricadeData barricadeData in barricadeRegion.barricades)
								{
									if (((barricadeData != null) ? barricadeData.barricade : null) != null)
									{
										ItemBarricadeAsset itemBarricadeAsset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, barricadeData.barricade.id);
										if (itemBarricadeAsset != null)
										{
											if (!isCfgBuilding(barricadeData.barricade.id))
											{
												BarricadeManager.destroyBarricade(barricadeRegion, (byte)i, (byte)j, 65535, (ushort)indx);
											}
										}
									}
								}
							}
						}
					}
					clearedBar = true;
				}
				catch { }
			}


			Console.WriteLine("Карта очищена!");
		}

		public bool isCfgBuilding(ushort id)
        {
			foreach (ushort allid in base.Configuration.Instance.IDsBuildings)
            {
				if (id == allid)
					return true;
            }
			return false;
        }

		public void RandPosTeleport(UnturnedPlayer uplayer)
		{
			if (base.Configuration.Instance.PositionsSpawn.Count == 0)
				return;

			if (CapturePoint.TimeLobby > 0 && CapturePoint.TimeRound == 0)
            {
				uplayer.Teleport(base.Configuration.Instance.LobbyPos, uplayer.Rotation);
				return;
            }
			if (GetPlayerTeam(uplayer) == null)
				return;

			DVTeam team = GetPlayerTeam(uplayer);
			List<DVPos> positions = new List<DVPos>();

			foreach (DVPos pos in base.Configuration.Instance.PositionsSpawn)
            {
				if (pos.TeamName.ToLower() == team.Name.ToLower())
					positions.Add(pos);
            }

			System.Random random = new System.Random();
			int randvalue = random.Next(0, (positions.Count - 1));
			Vector3 postp = positions[randvalue].Position;
			postp.y += base.Configuration.Instance.TPY;
			uplayer.Teleport(postp, uplayer.Rotation);
        }

		public DVTeam GetPlayerTeam(UnturnedPlayer uplayer)
        {
			if (uplayer.HasPermission(base.Configuration.Instance.Team1.Permission1) || uplayer.HasPermission(base.Configuration.Instance.Team1.Permission2))
				return base.Configuration.Instance.Team1;
			else if (uplayer.HasPermission(base.Configuration.Instance.Team2.Permission1) || uplayer.HasPermission(base.Configuration.Instance.Team2.Permission2))
				return base.Configuration.Instance.Team2;
			return null;
		}

		public byte MembersCount(DVTeam team)
        {
			byte count = 0;
			foreach(SteamPlayer steam in Provider.clients)
            {
				UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steam);
				if (team.Name == base.Configuration.Instance.Team1.Name)
				{
					if (TeamChoosed.ContainsKey(uplayer.CSteamID))
                    {
						if (TeamChoosed[uplayer.CSteamID] == 1)
							count++;
					}
				}
				else if (team.Name == base.Configuration.Instance.Team2.Name)
				{
					if (TeamChoosed.ContainsKey(uplayer.CSteamID))
					{
						if (TeamChoosed[uplayer.CSteamID] == 2)
							count++;
					}
				}
			}
			return count;
		}
		 
        private void onPlayerConnected(UnturnedPlayer uplayer)
		{
			try
			{
				try
				{
					if (!isTeamCreated)
					{
						GroupManager.addGroup(new CSteamID(base.Configuration.Instance.Team1.SteamGroupID), base.Configuration.Instance.Team1.Name);
						GroupManager.addGroup(new CSteamID(base.Configuration.Instance.Team2.SteamGroupID), base.Configuration.Instance.Team2.Name);
						Console.WriteLine("Группы успешно созданы!");
						isTeamCreated = true;
					}
				}
				catch { }

				if (uplayer.HasPermission(base.Configuration.Instance.Team1.Permission1))
					R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team1.PermissionID1, (IRocketPlayer)uplayer);
				if (uplayer.HasPermission(base.Configuration.Instance.Team1.Permission2))
					R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team1.PermissionID2, (IRocketPlayer)uplayer);
				if (uplayer.HasPermission(base.Configuration.Instance.Team2.Permission1))
					R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team2.PermissionID1, (IRocketPlayer)uplayer);
				if (uplayer.HasPermission(base.Configuration.Instance.Team2.Permission2))
					R.Permissions.RemovePlayerFromGroup(base.Configuration.Instance.Team2.PermissionID2, (IRocketPlayer)uplayer);

				if (TeamChoosed.ContainsKey(uplayer.CSteamID))
					TeamChoosed.Remove(uplayer.CSteamID);

				uplayer.Teleport(base.Configuration.Instance.LobbyPos, uplayer.Rotation);

				OpenChoseUI(uplayer);

				ClearInventory(uplayer);
			}
			catch { }
		}

        public override TranslationList DefaultTranslations => new TranslationList()
		{
			{ "ru_choose_team", "<color=blue>Выберите команду</color>" },
			{ "eng_choose_team", "<color=blue>Choose a team</color>" },
			{ "choose_lang", "<color=blue>Choose a lang | Выберите язык</color>" },
			{ "ru_no_balance", "Эта команда переполнена!" },
			{ "eng_no_balance", "This command is full!" },
			{ "ru_succes_join", "Вы успешно зашли в команду!" },
			{ "eng_succes_join", "Successfull joined!" },
			{ "eng_win", "Win command {0}" },
			{ "ru_win", "Победила команда {0}" },
			{ "eng_draw", "Draw!" },
			{ "ru_draw", "Ничья!" },
			{ "eng_start_time", "The game will continue in {0}" },
			{ "ru_start_time", "Игра начнется через {0}" },
			{ "eng_start_round", "Game Started!" },
			{ "ru_start_round", "Игра началась!" },
			{ "ru_reward_min_players", "Для получения награды, на сервере должно быть минимум {0} игроков!"},
			{ "eng_reward_min_players", "To receive a reward, there must be at least {0} players on the server!"},
		};


		public void OpenChoseUI(UnturnedPlayer uplayer)
		{
			EffectManager.sendUIEffect(base.Configuration.Instance.ChoseUI, (short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true);
			uplayer.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, true);
			if (!uplayer.HasPermission(base.Configuration.Instance.EngPermission) && !uplayer.HasPermission(base.Configuration.Instance.RusPermission))
			{
				EffectManager.sendUIEffectText((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "infotxt", Translate("choose_lang", null));
				EffectManager.sendUIEffectImageURL((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "choose1", base.Configuration.Instance.EngURLImg);
				EffectManager.sendUIEffectImageURL((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "choose2", base.Configuration.Instance.RusURLImg);
			}
            else
			{
				if (uplayer.HasPermission(base.Configuration.Instance.EngPermission))
					EffectManager.sendUIEffectText((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "infotxt", Translate("eng_choose_team", null));
				if (uplayer.HasPermission(base.Configuration.Instance.RusPermission))
					EffectManager.sendUIEffectText((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "infotxt", Translate("ru_choose_team", null));

				if (TeamChoosed.ContainsKey(uplayer.CSteamID))
				{
					if (TeamChoosed[uplayer.CSteamID] == 1)
					{
						EffectManager.sendUIEffectImageURL((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "choose1", base.Configuration.Instance.Team1.URLImg1);
						EffectManager.sendUIEffectImageURL((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "choose2", base.Configuration.Instance.Team1.URLImg2);
					}
					if (TeamChoosed[uplayer.CSteamID] == 2)
					{
						EffectManager.sendUIEffectImageURL((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "choose1", base.Configuration.Instance.Team2.URLImg1);
						EffectManager.sendUIEffectImageURL((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "choose2", base.Configuration.Instance.Team2.URLImg2);
					}
				}
                else
				{
					EffectManager.sendUIEffectImageURL((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "choose1", base.Configuration.Instance.Team1.URLImg);
					EffectManager.sendUIEffectImageURL((short)base.Configuration.Instance.ChoseUI, uplayer.CSteamID, true, "choose2", base.Configuration.Instance.Team2.URLImg);
				}
			}
		}


		public readonly byte[] EMPTY_BYTE_ARRAY = new byte[0];

		public void ClearInventory(UnturnedPlayer uplayer)
		{
			var playerInv = uplayer.Inventory;

			// "Remove "models" of items from player "body""
			uplayer.Player.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER,
				(byte)0, (byte)0, EMPTY_BYTE_ARRAY);
			uplayer.Player.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER,
				(byte)1, (byte)0, EMPTY_BYTE_ARRAY);

			// Remove items
			for (byte page = 0; page < PlayerInventory.PAGES; page++)
			{
				if (page == PlayerInventory.AREA)
					continue;

				var count = playerInv.getItemCount(page);

				for (byte index = 0; index < count; index++)
				{
					playerInv.removeItem(page, 0);
				}
			}

			// Remove clothes

			// Remove unequipped cloths
			System.Action removeUnequipped = () => {
				for (byte i = 0; i < playerInv.getItemCount(2); i++)
				{
					playerInv.removeItem(2, 0);
				}
			};

			// Unequip & remove from inventory
			uplayer.Player.clothing.askWearBackpack(0, 0, EMPTY_BYTE_ARRAY, true);
			removeUnequipped();

			uplayer.Player.clothing.askWearGlasses(0, 0, EMPTY_BYTE_ARRAY, true);
			removeUnequipped();

			uplayer.Player.clothing.askWearHat(0, 0, EMPTY_BYTE_ARRAY, true);
			removeUnequipped();

			uplayer.Player.clothing.askWearPants(0, 0, EMPTY_BYTE_ARRAY, true);
			removeUnequipped();

			uplayer.Player.clothing.askWearMask(0, 0, EMPTY_BYTE_ARRAY, true);
			removeUnequipped();

			uplayer.Player.clothing.askWearShirt(0, 0, EMPTY_BYTE_ARRAY, true);
			removeUnequipped();

			uplayer.Player.clothing.askWearVest(0, 0, EMPTY_BYTE_ARRAY, true);
			removeUnequipped();
		}
		protected override void Unload()
        {
        }
    }
}
