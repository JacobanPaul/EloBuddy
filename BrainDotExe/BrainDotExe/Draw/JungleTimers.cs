﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BrainDotExe.Util;
using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;
using Utility = BrainDotExe.Common.Utility;

namespace BrainDotExe.Draw
{
    public static class JungleTimers
    {
        public static Menu JungleTimersMenu;

        public static Text Text { get; set; }

        public static List<Tuple<float, Camp>> Times = new List<Tuple<float, Camp>>();

        public static List<Tuple<float, Vector3>> TimesHealth = new List<Tuple<float, Vector3>>();

        public static void Init()
        {
            JungleTimersMenu = Program.Menu.AddSubMenu("Jungle Timers", "jungleTimersDraw");
            JungleTimersMenu.AddGroupLabel("Jungle Timers");
            JungleTimersMenu.Add("drawTimers", new CheckBox("Show Times", true));
            JungleTimersMenu.Add("drawTimersHealth", new CheckBox("Show Times of health Howling Abyss", true));
            JungleTimersMenu.Add("drawFontSize", new Slider("Font Size - F5 To Reload", 8, 5, 14));
            Text = new Text("", new Font(FontFamily.GenericSansSerif, Misc.getSliderValue(JungleTimersMenu, "drawFontSize"), FontStyle.Regular)) { Color = Color.White };

            GameObject.OnCreate += GameObjectOnCreate;
            GameObject.OnDelete += GameObjectOnDelete;
            Drawing.OnEndScene += JungleTimers_OnDraw;
            Game.OnUpdate += OnGameUpdate;
        }

        public static void JungleTimers_OnDraw(EventArgs args)
        {
            if (Misc.isChecked(Program.DrawMenu, "drawDisable")) return;

            if (Misc.isChecked(JungleTimersMenu, "drawTimers"))
            {
                var auxtimes = new List<Tuple<float, Camp>>();
                foreach (var timer in Times)
                {
                    var diffTime = timer.Item1 - Game.Time;

                    if (diffTime > 0)
                    {
                        var seconds = Math.Floor(diffTime % 60);
                        var minutes = Math.Floor(diffTime / 60);

                        if (diffTime < 60)
                        {
                            minutes = 0;
                        }

                        var spellString = minutes <= 0 ? seconds + "" : minutes + ":" + seconds;

                        Text.Draw(spellString, Color.White, new Vector2(Drawing.WorldToMinimap(timer.Item2.Position).X - 5, Drawing.WorldToMinimap(timer.Item2.Position).Y - 5));
                    }
                    else
                    {
                        auxtimes.Add(timer);
                    }
                }

                if (auxtimes.Count > 0)
                {
                    Times = Times.Except(auxtimes).ToList();
                }
            }

            if (Misc.isChecked(JungleTimersMenu, "drawTimersHealth"))
            {
                var auxtimes = new List<Tuple<float, Vector3>>();
                foreach (var timer in TimesHealth)
                {
                    var diffTime = timer.Item1 - Game.Time;

                    if (diffTime > 0)
                    {
                        var seconds = Math.Floor(diffTime % 60);
                        var minutes = Math.Floor(diffTime / 60);

                        if (diffTime < 60)
                        {
                            minutes = 0;
                        }

                        var spellString = minutes <= 0 ? seconds + "" : minutes + ":" + seconds;

                        Text.Draw(spellString, Color.White, new Vector2(Drawing.WorldToMinimap(timer.Item2).X - 5, Drawing.WorldToMinimap(timer.Item2).Y - 5));
                    }
                    else
                    {
                        auxtimes.Add(timer);
                    }
                }

                if (auxtimes.Count > 0)
                {
                    TimesHealth = TimesHealth.Except(auxtimes).ToList();
                }
            }
        }

        public static void OnGameUpdate(EventArgs args)
        {
            foreach (var camp in Jungle.Camps.Where(camp => camp.MapType.ToString() == Game.MapId.ToString() && !camp.IsDead))
            {
                var campIsDead = false;
                var mobCount = 0;
                foreach (var mob in camp.Mobs)
                {
                    if (mob.IsDead)
                    {
                        mobCount++;
                    }

                    if (mobCount == camp.Mobs.Count)
                    {
                        campIsDead = true;
                    }
                }

                if (campIsDead)
                {
                    var timer = new Tuple<float, Camp>(Game.Time + camp.RespawnTimer - 5, camp);
                    Times.Add(timer);
                }

                camp.IsDead = campIsDead;
            }
        }

        public static void GameObjectOnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Odin"))
            {
                TimesHealth.Add(new Tuple<float, Vector3>(40 + Game.Time, sender.Position));
            }

            if (!(sender is Obj_AI_Minion) || sender.Team != GameObjectTeam.Neutral)
            {
                return;
            }

            var minion = (Obj_AI_Minion)sender;

            foreach (var camp in Jungle.Camps.Where(camp => camp.MapType.ToString() == Game.MapId.ToString()))
            {
                foreach (var mob in camp.Mobs)
                {
                    if (mob.Name == minion.Name)
                    {
                        mob.IsDead = false;
                        camp.IsDead = false;
                    }
                }
            }
        }

        public static void GameObjectOnDelete(GameObject sender, EventArgs args)
        {

            if (!(sender is Obj_AI_Minion) || sender.Team != GameObjectTeam.Neutral)
            {
                return;
            }

            var minion = (Obj_AI_Minion)sender;

            foreach (var camp in Jungle.Camps.Where(camp => camp.MapType.ToString() == Game.MapId.ToString()))
            {
                foreach (var mob in camp.Mobs)
                {
                    if (mob.Name == minion.Name)
                    {
                        mob.IsDead = true;
                    }
                }
            }
        }

        public class Jungle
        {
            public static List<Camp> Camps;

            static Jungle()
            {
                try
                {
                    Camps = new List<Camp>
                    {
                        // Order: Blue
                        new Camp("Blue",
                            115, 300, new Vector3(3872f, 7900f, 51f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Blue1.1.1"),
                                    new Mob("SRU_BlueMini1.1.2", true),
                                    new Mob("SRU_BlueMini21.1.3", true)
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Order,
                            Color.Cyan, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true),
                        //Order: Wolves
                        new Camp("Wolves",
                            115, 100, new Vector3(3825f, 6491f, 52f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Murkwolf2.1.1"),
                                    new Mob("SRU_MurkwolfMini2.1.2"),
                                    new Mob("SRU_MurkwolfMini2.1.3")
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Order,
                            Color.White, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Order: Raptor
                        new Camp("Raptor",
                            115, 100, new Vector3(6954f, 5458f, 53f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Razorbeak3.1.1", true),
                                    new Mob("SRU_RazorbeakMini3.1.2"),
                                    new Mob("SRU_RazorbeakMini3.1.3"),
                                    new Mob("SRU_RazorbeakMini3.1.4")
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Order,
                            Color.Salmon, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true),
                        //Order: Red
                        new Camp("Red",
                            115, 300, new Vector3(7862f, 4111f, 54f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Red4.1.1"),
                                    new Mob("SRU_RedMini4.1.2", true),
                                    new Mob("SRU_RedMini4.1.3", true)
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Order,
                            Color.Red, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true),

                        //Order: Krug
                        new Camp("Krug",
                            115, 100, new Vector3(8381f, 2711f, 51f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Krug5.1.2"),
                                    new Mob("SRU_KrugMini5.1.1")
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Order,
                            Color.White, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Order: Gromp
                        new Camp("Gromp",
                            115, 100, new Vector3(2091f, 8428f, 52f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Gromp13.1.1", true)
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Order,
                            Color.Green, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true),
                        //Chaos: Blue
                        new Camp("Blue",
                            115, 300, new Vector3(10930f, 6992f, 52f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Blue7.1.1"),
                                    new Mob("SRU_BlueMini7.1.2", true),
                                    new Mob("SRU_BlueMini27.1.3", true)
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Chaos,
                            Color.Cyan, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true),
                        //Chaos: Wolves
                        new Camp("Wolves",
                            115, 100, new Vector3(10957f, 8350f, 62f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Murkwolf8.1.1"),
                                    new Mob("SRU_MurkwolfMini8.1.2"),
                                    new Mob("SRU_MurkwolfMini8.1.3")
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Chaos,
                            Color.White, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Chaos: Raptor
                        new Camp("Raptor",
                            115, 100, new Vector3(7857f, 9471f, 52f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Razorbeak9.1.1", true),
                                    new Mob("SRU_RazorbeakMini9.1.2"),
                                    new Mob("SRU_RazorbeakMini9.1.3"),
                                    new Mob("SRU_RazorbeakMini9.1.4")
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Chaos,
                            Color.Salmon, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true),
                        //Chaos: Red
                        new Camp("Red",
                            115, 300, new Vector3(7017f, 10775f, 56f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Red10.1.1"),
                                    new Mob("SRU_RedMini10.1.2", true),
                                    new Mob("SRU_RedMini10.1.3", true)
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Chaos,
                            Color.Red, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true),
                        //Chaos: Krug
                        new Camp("Krug",
                            115, 100, new Vector3(6449f, 12117f, 56f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Krug11.1.2"),
                                    new Mob("SRU_KrugMini11.1.1")
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Chaos,
                            Color.White, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Chaos: Gromp
                        new Camp("Gromp",
                            115, 100, new Vector3(12703f, 6444f, 52f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Gromp14.1.1", true)
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Chaos,
                            Color.Green, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true),
                        //Neutral: Dragon
                        new Camp("Dragon",
                            150, 360, new Vector3(9866f, 4414f, -71f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Dragon6.1.1")
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Neutral,
                            Color.Orange, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Neutral: Baron
                        new Camp("Baron",
                            120, 420, new Vector3(5007f, 10471f, -71f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("SRU_Baron12.1.1", true, false, null, 0)
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Neutral,
                            Color.DarkOrchid, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true, false , 8),
                        //Dragon: Crab
                        new Camp("Crab",
                            150, 180, new Vector3(10508f, 5271f, -62f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("Sru_Crab15.1.1")
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Neutral,
                            Color.PaleGreen, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Baron: Crab
                        new Camp("Crab",
                            150, 180, new Vector3(4418f, 9664f, -69f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("Sru_Crab16.1.1")
                                }),
                            Utility.Map.MapType.SummonersRift,
                            GameObjectTeam.Neutral,
                            Color.PaleGreen, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Order: Wraiths
                        new Camp("Wraiths",
                            95, 75, new Vector3(4373f, 5843f, -107f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("TT_NWraith1.1.1", true),
                                    new Mob("TT_NWraith21.1.2", true),
                                    new Mob("TT_NWraith21.1.3", true)
                                }),
                            Utility.Map.MapType.TwistedTreeline,
                            GameObjectTeam.Order,
                            Color.White, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true),
                        //Order: Golems
                        new Camp("Golems",
                            95, 75, new Vector3(5107f, 7986f, -108f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("TT_NGolem2.1.1"),
                                    new Mob("TT_NGolem22.1.2")
                                }),
                            Utility.Map.MapType.TwistedTreeline,
                            GameObjectTeam.Order,
                            Color.White, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Order: Wolves
                        new Camp("Wolves",
                            95, 75, new Vector3(6078f, 6094f, -99f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("TT_NWolf3.1.1"),
                                    new Mob("TT_NWolf23.1.2"),
                                    new Mob("TT_NWolf23.1.3")
                                }),
                            Utility.Map.MapType.TwistedTreeline,
                            GameObjectTeam.Order,
                            Color.White, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Chaos: Wraiths
                        new Camp("Wraiths",
                            95, 75, new Vector3(11026f, 5806f, -107f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("TT_NWraith4.1.1", true),
                                    new Mob("TT_NWraith24.1.2", true),
                                    new Mob("TT_NWraith24.1.3", true)
                                }),
                            Utility.Map.MapType.TwistedTreeline,
                            GameObjectTeam.Chaos,
                            Color.White, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true),
                        //Chaos: Golems
                        new Camp("Golems",
                            95, 75, new Vector3(10277f, 8038f, -109f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("TT_NGolem5.1.1"),
                                    new Mob("TT_NGolem25.1.2")
                                }),
                            Utility.Map.MapType.TwistedTreeline,
                            GameObjectTeam.Chaos,
                            Color.White, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Chaos: Wolves
                        new Camp("Wolves",
                            95, 75, new Vector3(9294f, 6085f, -97f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("TT_NWolf6.1.1"),
                                    new Mob("TT_NWolf26.1.2"),
                                    new Mob("TT_NWolf26.1.3")
                                }),
                            Utility.Map.MapType.TwistedTreeline,
                            GameObjectTeam.Chaos,
                            Color.White, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0))),
                        //Neutral: Spider
                        new Camp("Spider",
                            600, 360, new Vector3(7738f, 10080f, -62f),
                            new List<Mob>(
                                new[]
                                {
                                    new Mob("TT_Spiderboss8.1.1")
                                }),
                            Utility.Map.MapType.TwistedTreeline,
                            GameObjectTeam.Neutral,
                            Color.DarkOrchid, new Timers(new Vector3(0, 0, 0), new Vector2(0, 0)), true)
                    };
                }
                catch (Exception)
                {
                    Camps = new List<Camp>();
                }
            }
        }
    }

    public class Camp
    {
        public Camp(string name,
            float spawnTime,
            int respawnTimer,
            Vector3 position,
            List<Mob> mobs,
            Utility.Map.MapType mapType,
            GameObjectTeam team,
            Color colour,
            Timers timer,
            bool isRanged = false,
            bool isDead = false,
            int state = 0,
            int respawnTime = 0,
            int lastChangeOnState = 0,
            bool shouldping = true,
            int lastPing = 0)
        {
            Name = name;
            IsDead = isDead;
            SpawnTime = spawnTime;
            RespawnTimer = respawnTimer;
            Position = position;
            ScreenPosition = Drawing.WorldToScreen(Position);
            MinimapPosition = Drawing.WorldToMinimap(Position);
            Mobs = mobs;
            MapType = mapType;
            Team = team;
            Colour = colour;
            IsRanged = isRanged;
            State = state;
            RespawnTime = respawnTime;
            LastChangeOnState = lastChangeOnState;
            Timer = timer;
            ShouldPing = shouldping;
            LastPing = lastPing;
        }

        public string Name { get; set; }
        public bool IsDead { get; set; }
        public float SpawnTime { get; set; }
        public int RespawnTimer { get; set; }
        public Vector3 Position { get; set; }
        public Vector2 MinimapPosition { get; set; }
        public Vector2 ScreenPosition { get; set; }
        public List<Mob> Mobs { get; set; }
        public Utility.Map.MapType MapType { get; set; }
        public GameObjectTeam Team { get; set; }
        public Color Colour { get; set; }
        public bool IsRanged { get; set; }
        public int State { get; set; }
        public int RespawnTime { get; set; }
        public int LastChangeOnState { get; set; }
        public Timers Timer { get; set; }
        public bool ShouldPing { get; set; }
        public int LastPing { get; set; }
    }

    public class Mob
    {
        public Mob(string name, bool isRanged = false, bool isDead = false, Obj_AI_Minion unit = null, int state = 0, int networkId = 0,
            int lastChangeOnState = 0, bool justDied = false)
        {
            Name = name;
            IsDead = isDead;
            IsRanged = isRanged;
            Unit = unit;
            State = state;
            NetworkId = networkId;
            LastChangeOnState = lastChangeOnState;
            JustDied = justDied;
        }

        public Obj_AI_Minion Unit { get; set; }
        public string Name { get; set; }
        public bool IsDead { get; set; }
        public bool IsRanged { get; set; }
        public int State { get; set; }
        public int NetworkId { get; set; }
        public int LastChangeOnState { get; set; }
        public bool JustDied { get; set; }
    }

    public class Timers
    {
        public Timers(Vector3 position, Vector2 minimapPosition, string textOnMap = "", string textOnMinimap = "")
        {
            TextOnMap = textOnMap;
            TextOnMinimap = textOnMinimap;
            Position = position;
            MinimapPosition = minimapPosition;
        }

        public string TextOnMap { get; set; }
        public string TextOnMinimap { get; set; }
        public Vector3 Position { get; set; }
        public Vector2 MinimapPosition { get; set; }
    }

}
