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
    public static class CapturePoint
    {
        public static ushort StatusUI;
        public static int TimeRound;
        public static int TimeLobby;

        public static DVPoint PointA;
        public static DVPoint PointB;

        public static void Update()
        {
            if (TimeRound == 0)
            {
                if (TimeLobby > 1)
                {
                    TimeLobby--;
                }
                else if (TimeLobby == 1)
                {
                    TimeLobby--;
                    StartRound();
                }
                else
                {
                    if ((PointA.Captured == "1" && PointB.Captured != "2") || (PointA.Captured != "2" && PointB.Captured == "1"))
                        Win("1");
                    else if ((PointA.Captured != "1" && PointB.Captured == "2") || PointA.Captured == "2" && PointB.Captured != "1")
                        Win("2");
                    else
                        Win("0");
                }
            }
            else if (TimeRound > 0)
            {
                TimeRound--;
                if (CPP("a", "1") != CPP("a", "2"))
                {
                    if (CPP("a", "1") > CPP("a", "2"))
                    {
                        if (PointA.Captured != "1")
                        {
                            if (PointA.CapturProgress == "2")
                                PointA.SecOnCapture = 0;
                            PointA.CapturProgress = "1";
                            PointA.SecOnCapture++;
                            if (PointA.SecOnCapture == Plugin.Instance.Configuration.Instance.PointA.SecOnCapture)
                            {
                                PointA.Captured = "1";
                                PointA.SecOnCapture = 0;
                                if (PointB.Captured == "1")
                                    Win("1");
                            }
                        }
                    }
                    else if (CPP("a", "1") < CPP("a", "2"))
                    {
                        if (PointA.Captured != "2")
                        {
                            if (PointA.CapturProgress == "1")
                                PointA.SecOnCapture = 0;
                            PointA.CapturProgress = "2";
                            PointA.SecOnCapture++;
                            if (PointA.SecOnCapture == Plugin.Instance.Configuration.Instance.PointA.SecOnCapture)
                            {
                                PointA.Captured = "2";
                                PointA.SecOnCapture = 0;
                                if (PointB.Captured == "2")
                                    Win("2");
                            }
                        }
                    }
                }
                else
                {
                    if (PointA.SecOnCapture > 0)
                        PointA.SecOnCapture--;
                }
                if (CPP("b", "1") != CPP("b", "2"))
                {
                    if (CPP("b", "1") > CPP("b", "2"))
                    {
                        if (PointB.Captured != "1")
                        {
                            if (PointB.CapturProgress == "2")
                                PointB.SecOnCapture = 0;
                            PointB.CapturProgress = "1";
                            PointB.SecOnCapture++;
                            if (PointB.SecOnCapture == Plugin.Instance.Configuration.Instance.PointB.SecOnCapture)
                            {
                                PointB.Captured = "1";
                                PointB.SecOnCapture = 0;
                                if (PointA.Captured == "1")
                                    Win("1");
                            }
                        }
                    }
                    else if (CPP("b", "1") < CPP("b", "2"))
                    {
                        if (PointB.Captured != "2")
                        {
                            if (PointB.CapturProgress == "1")
                                PointB.SecOnCapture = 0;
                            PointB.CapturProgress = "2";
                            PointB.SecOnCapture++;
                            if (PointB.SecOnCapture == Plugin.Instance.Configuration.Instance.PointB.SecOnCapture)
                            {
                                PointB.Captured = "2";
                                PointB.SecOnCapture = 0;
                                if (PointA.Captured == "2")
                                    Win("2");
                            }
                        }
                    }
                }
                else
                {
                    if (PointB.SecOnCapture > 0)
                        PointB.SecOnCapture--;
                }
                if (TimeRound > 0)
                    UpdateUI();
            }
        }

        public static void StartRound()
        {
            TimeRound = Plugin.Instance.Configuration.Instance.TimeRound;
            PointA.Captured = "";
            PointA.SecOnCapture = 0;
            PointA.CapturProgress = "";
            PointB.Captured = "";
            PointB.SecOnCapture = 0;
            PointB.CapturProgress = "";
            

            foreach (SteamPlayer steamPlayer in Provider.clients)
            {
                UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                Plugin.Instance.RandPosTeleport(uplayer);
                UnturnedChat.Say(uplayer, Plugin.Instance.Translate("eng_start_round", null));
            }
        }

        public static void Win(string numberCommand)
        {
            TimeLobby = Plugin.Instance.Configuration.Instance.TimeLobby;
            TimeRound = 0;
            foreach (SteamPlayer steamPlayer in Provider.clients)
            { 
                UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                uplayer.Teleport(Plugin.Instance.Configuration.Instance.LobbyPos, uplayer.Rotation);

                if (numberCommand == "1")
                {
                    if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.EngPermission))
                        EffectManager.sendUIEffect(Plugin.Instance.Configuration.Instance.WinTable, (short)StatusUI, uplayer.CSteamID, true, Plugin.Instance.Translate("eng_win", Plugin.Instance.Configuration.Instance.Team1.Name));
                    if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.RusPermission))
                        EffectManager.sendUIEffect(Plugin.Instance.Configuration.Instance.WinTable, (short)StatusUI, uplayer.CSteamID, true, Plugin.Instance.Translate("ru_win", Plugin.Instance.Configuration.Instance.Team1.Name));
                }
                else if (numberCommand == "2")
                {
                    if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.EngPermission))
                        EffectManager.sendUIEffect(Plugin.Instance.Configuration.Instance.WinTable, (short)StatusUI, uplayer.CSteamID, true, Plugin.Instance.Translate("eng_win", Plugin.Instance.Configuration.Instance.Team2.Name));
                    if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.RusPermission))
                        EffectManager.sendUIEffect(Plugin.Instance.Configuration.Instance.WinTable, (short)StatusUI, uplayer.CSteamID, true, Plugin.Instance.Translate("ru_win", Plugin.Instance.Configuration.Instance.Team2.Name));
                }
                else
                {
                    if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.EngPermission))
                        EffectManager.sendUIEffect(Plugin.Instance.Configuration.Instance.WinTable, (short)StatusUI, uplayer.CSteamID, true, Plugin.Instance.Translate("eng_draw", null));
                    if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.RusPermission))
                        EffectManager.sendUIEffect(Plugin.Instance.Configuration.Instance.WinTable, (short)StatusUI, uplayer.CSteamID, true, Plugin.Instance.Translate("ru_draw", null));
                }
                if (numberCommand == "1")
                {
                    if (Plugin.Instance.GetPlayerTeam(uplayer).Name == Plugin.Instance.Configuration.Instance.Team1.Name)
                        Reward(uplayer);
                }
                else if (numberCommand == "2")
                {
                    if (Plugin.Instance.GetPlayerTeam(uplayer).Name == Plugin.Instance.Configuration.Instance.Team2.Name)
                        Reward(uplayer);
                }

                if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.EngPermission))
                    UnturnedChat.Say(uplayer, Plugin.Instance.Translate("eng_start_time", DVTimeSpan(Plugin.Instance.Configuration.Instance.TimeLobby)));
                if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.RusPermission))
                    UnturnedChat.Say(uplayer, Plugin.Instance.Translate("ru_start_time", DVTimeSpan(Plugin.Instance.Configuration.Instance.TimeLobby)));


                if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.Team1.Permission1))
                    R.Permissions.RemovePlayerFromGroup(Plugin.Instance.Configuration.Instance.Team1.PermissionID1, (IRocketPlayer)uplayer);
                if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.Team1.Permission2))
                    R.Permissions.RemovePlayerFromGroup(Plugin.Instance.Configuration.Instance.Team1.PermissionID2, (IRocketPlayer)uplayer);
                if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.Team2.Permission1))
                    R.Permissions.RemovePlayerFromGroup(Plugin.Instance.Configuration.Instance.Team2.PermissionID1, (IRocketPlayer)uplayer);
                if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.Team2.Permission2))
                    R.Permissions.RemovePlayerFromGroup(Plugin.Instance.Configuration.Instance.Team2.PermissionID2, (IRocketPlayer)uplayer);
                if (Plugin.Instance.TeamChoosed.ContainsKey(uplayer.CSteamID))
                    Plugin.Instance.TeamChoosed.Remove(uplayer.CSteamID);
                Plugin.Instance.OpenChoseUI(uplayer);
            }
            Plugin.Instance.ClearMap();
        }

        public static void Reward(UnturnedPlayer uplayer)
        {
            if (Provider.clients.Count < Plugin.Instance.Configuration.Instance.MinCountPlayers)
            {
                if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.EngPermission))
                    UnturnedChat.Say(uplayer, Plugin.Instance.Translate("eng_reward_min_players", Plugin.Instance.Configuration.Instance.MinCountPlayers));
                if (uplayer.HasPermission(Plugin.Instance.Configuration.Instance.RusPermission))
                    UnturnedChat.Say(uplayer, Plugin.Instance.Translate("ru_reward_min_players", Plugin.Instance.Configuration.Instance.MinCountPlayers));
                return;
            }
            uplayer.Experience += Plugin.Instance.Configuration.Instance.ExpReward; 
            R.Commands.Execute(new ConsolePlayer(), "pay " + uplayer.CSteamID + " " + Plugin.Instance.Configuration.Instance.UconomyMonyReward);
        }

        public static int CPP(string namePoint, string numberTeam)
        {
            int count = 0;
            foreach (SteamPlayer steamPlayer in Provider.clients)
            {
                UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);

                if (Plugin.Instance.GetPlayerTeam(uplayer) != null)
                {
                    if (namePoint.ToLower() == "a")
                    {
                        if (numberTeam == "1")
                        {
                            if (Plugin.Instance.GetPlayerTeam(uplayer).Name == Plugin.Instance.Configuration.Instance.Team1.Name)
                            {
                                if (Vector3.Distance(uplayer.Position, PointA.Position) <= PointA.Radius)
                                    count++;
                            }
                        }
                        else if (numberTeam == "2")
                        {
                            if (Plugin.Instance.GetPlayerTeam(uplayer).Name == Plugin.Instance.Configuration.Instance.Team2.Name)
                            {
                                if (Vector3.Distance(uplayer.Position, PointA.Position) <= PointA.Radius)
                                    count++;
                            }
                        }
                    }
                    if (namePoint.ToLower() == "b")
                    {
                        if (numberTeam == "1")
                        {
                            if (Plugin.Instance.GetPlayerTeam(uplayer).Name == Plugin.Instance.Configuration.Instance.Team1.Name)
                            {
                                if (Vector3.Distance(uplayer.Position, PointB.Position) <= PointB.Radius)
                                    count++;
                            }
                        }
                        else if (numberTeam == "2")
                        {
                            if (Plugin.Instance.GetPlayerTeam(uplayer).Name == Plugin.Instance.Configuration.Instance.Team2.Name)
                            {
                                if (Vector3.Distance(uplayer.Position, PointB.Position) <= PointB.Radius)
                                    count++;
                            }
                        }
                    }
                }
            }
            return count;
        }

        public static void UpdateUI()
        {
            string colorTeam1 = Plugin.Instance.Configuration.Instance.Team1.Color;
            string colorTeam2 = Plugin.Instance.Configuration.Instance.Team2.Color;

            foreach (SteamPlayer steamPlayer in Provider.clients)
            {
                UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);

                EffectManager.sendUIEffect(StatusUI, (short)StatusUI, uplayer.CSteamID, true);
                EffectManager.sendUIEffectText((short)StatusUI, uplayer.CSteamID, true, "timetxt", DVTimeSpan(TimeRound));

                if (PointA.Captured == "1")
                    EffectManager.sendUIEffectText((short)StatusUI, uplayer.CSteamID, true, "txtpoint1", $"<color={colorTeam1}>A</color>");
                else if (PointA.Captured == "2")
                    EffectManager.sendUIEffectText((short)StatusUI, uplayer.CSteamID, true, "txtpoint1", $"<color={colorTeam2}>A</color>");
                if (PointB.Captured == "1")
                    EffectManager.sendUIEffectText((short)StatusUI, uplayer.CSteamID, true, "txtpoint2", $"<color={colorTeam1}>B</color>");
                else if (PointB.Captured == "2")
                    EffectManager.sendUIEffectText((short)StatusUI, uplayer.CSteamID, true, "txtpoint2", $"<color={colorTeam2}>B</color>");

                if (PointA.CapturProgress == "1")
                    EffectManager.sendUIEffectImageURL((short)StatusUI, uplayer.CSteamID, true, "progressimgpoint1t2", "");
                else if (PointA.CapturProgress == "2")
                    EffectManager.sendUIEffectImageURL((short)StatusUI, uplayer.CSteamID, true, "progressimgpoint1t1", "");
                if (PointB.CapturProgress == "1")
                    EffectManager.sendUIEffectImageURL((short)StatusUI, uplayer.CSteamID, true, "progressimgpoint2t2", "");
                else if (PointB.CapturProgress == "2")
                    EffectManager.sendUIEffectImageURL((short)StatusUI, uplayer.CSteamID, true, "progressimgpoint2t1", "");
                
                EffectManager.sendUIEffectText((short)StatusUI, uplayer.CSteamID, true, "progresstxtpoint1t1", ProgressCapture("a"));
                EffectManager.sendUIEffectText((short)StatusUI, uplayer.CSteamID, true, "progresstxtpoint2t1", ProgressCapture("b"));
                EffectManager.sendUIEffectText((short)StatusUI, uplayer.CSteamID, true, "progresstxtpoint1t2", ProgressCapture("a"));
                EffectManager.sendUIEffectText((short)StatusUI, uplayer.CSteamID, true, "progresstxtpoint2t2", ProgressCapture("b"));
            }
        }


        public static string ProgressCapture(string namePoint)
        {
            float procent = 0;
            if (namePoint.ToLower() == "a")
                procent = (100 * PointA.SecOnCapture) / Plugin.Instance.Configuration.Instance.PointA.SecOnCapture;
            if (namePoint.ToLower() == "b")
                procent = (100 * PointB.SecOnCapture) / Plugin.Instance.Configuration.Instance.PointB.SecOnCapture;
            string txt = "";
            for (int i = 0; i < Math.Round(procent / 10); i++)
            {
                txt += " ";
            }
            return txt;
        }

        public static string DVTimeSpan(int seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            if (seconds >= 3600)
            {
                return ts.Hours + ":" + ts.Minutes;
            }
            else if (seconds >= 60)
            {
                if (ts.Seconds < 10)
                {
                    return ts.Minutes + ":0" + ts.Seconds;
                }
                return ts.Minutes + ":" + ts.Seconds;
            }
            else
            {
                return ts.Seconds + "с.";
            }
        }

    }
}
