using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BrainDotExe.Common;
using BrainDotExe.Util;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace BrainDotExe.Draw
{
    class LastSeenPosition
    {
        public static Menu LastSeenPositionMenu;

        private static readonly IList<HeroTracker> _heroTrackers = new List<HeroTracker>();
        public static Text Text { get; set; }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Init()
        {
            Text = new Text("", new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold)) { Color = Color.White };
            LastSeenPositionMenu = Program.Menu.AddSubMenu("Last Seen Position ", "lastSeenDraw");
            LastSeenPositionMenu.AddGroupLabel("Last Seen Position");
            LastSeenPositionMenu.Add("drawEnd", new CheckBox("Show the last enemy position", true));
            LastSeenPositionMenu.Add("drawSeconds", new CheckBox("Show missing seconds ", true));

            foreach (var aiHeroClient in EntityManager.Heroes.Enemies)
            {
                _heroTrackers.Add(new HeroTracker(aiHeroClient, ImageLoader.Load(aiHeroClient.ChampionName)));
            }

            Game.OnEnd += GameOnOnEnd;
        }

        private static void GameOnOnEnd(GameEndEventArgs args)
        {
            Environment.Exit(1);
        }
    }

    public class HeroTracker
    {
        public HeroTracker(AIHeroClient hero, Bitmap bmp)
        {
            Hero = hero;
            var image = new Render.Sprite(bmp, new Vector2(0, 0));
            image.GrayScale();
            image.Scale = new Vector2(0.5f, 0.5f);
            image.VisibleCondition = sender => !hero.IsHPBarRendered && !hero.IsDead && Misc.isChecked(LastSeenPosition.LastSeenPositionMenu, "drawEnd");
            image.PositionUpdate = delegate
            {
                var v2 = Drawing.WorldToMinimap(LastLocation);
                v2.X -= image.Width / 2f;
                v2.Y -= image.Height / 2f;
                return v2;
            };
            image.Add(0);
            LastSeen = 0;
            LastLocation = hero.ServerPosition;
            PredictedLocation = hero.ServerPosition;
            BeforeRecallLocation = hero.ServerPosition;
            Pinged = false;

            Drawing.OnEndScene += LastSeenPosition_OnDraw;
            Game.OnTick += LastSeenPosition_OnTick;
        }

        private  void LastSeenPosition_OnTick(EventArgs args)
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(enemy => enemy.ChampionName == Hero.ChampionName))
            {
                if (enemy.IsHPBarRendered)
                {
                    LastLocation = enemy.ServerPosition;
                    LastSeen = Game.Time;
                }
                else
                {
                    if (LastSeen - Game.Time > 30 || Pinged)
                    {
                        TacticalMap.ShowPing(PingCategory.Fallback, LastLocation, true);
                        Pinged = true;
                    }
                }
            }
        }

        public static string Format(float f)
        {
            var t = TimeSpan.FromSeconds(f);
            if (t.Minutes < 1)
            {
                return t.Seconds + "";
            }
            if (t.Seconds >= 10)
            {
                return t.Minutes + ":" + t.Seconds;
            }
            return t.Minutes + ":0" + t.Seconds;
        }

        public void LastSeenPosition_OnDraw(EventArgs args)
        {
            if (Misc.isChecked(Program.DrawMenu, "drawDisable")) return;

            if (!Misc.isChecked(LastSeenPosition.LastSeenPositionMenu, "drawEnd"))
            {
                return;
            }

            if (!Hero.IsHPBarRendered)
            {
                ImageLoader.Load(Hero.ChampionName);
            }

            if (Misc.isChecked(LastSeenPosition.LastSeenPositionMenu, "drawSeconds"))
            {
                if (!Hero.IsHPBarRendered && !Hero.IsDead)
                {
                    LastSeenPosition.Text.Draw(Format(Game.Time - LastSeen), Color.White, new Vector2(Drawing.WorldToMinimap(LastLocation).X - 5, Drawing.WorldToMinimap(LastLocation).Y + 10));
                }
            }
        }

        public AIHeroClient Hero { get; set; }
        private float LastSeen { get; set; }
        private Vector3 LastLocation { get; set; }
        private Vector3 PredictedLocation { get; set; }
        private Vector3 BeforeRecallLocation { get; set; }
        private bool Pinged { get; set; }
    }

}
