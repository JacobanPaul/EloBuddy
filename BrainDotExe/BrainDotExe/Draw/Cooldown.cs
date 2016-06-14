using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Menu;
using BrainDotExe.Util;
using SharpDX;
using Color = System.Drawing.Color;

namespace BrainDotExe.Draw
{
    class Cooldown
    {
        #region Vars

        public static Menu CooldonMenu;
        private static int X;  // HPBar Screen X Position
        private static int Y;  // HPBar Screen Y Position
        private static int SummonerSpellX; // Coor of X Summoner Spell
        private static int SummonerSpellY; // Coor of Y Summoner Spell

        //static Font DisplayTextFont = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Tahoma", 10)); // Text Font
        private static string GetSummonerSpellName;

        public static SpellSlot[] SummonerSpellSlots = { SpellSlot.Summoner1, SpellSlot.Summoner2 };
        public static SpellSlot[] SpellSlots = { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };

        private static Text Text { get; set; }

        public static void Init()
        {
            Text = new Text("", new Font(FontFamily.GenericSansSerif, 9, FontStyle.Regular)) { Color = Color.White };
            CooldonMenu = Program.Menu.AddSubMenu("Tracker ", "cooldownDraw");
            CooldonMenu.AddGroupLabel("Tracker Cooldown");
            CooldonMenu.Add("drawCoolDowns", new CheckBox("Draw Cooldown of abilities", true));

            Drawing.OnDraw += Cooldown_OnDraw;
        }

        #endregion
        public static void Cooldown_OnDraw(EventArgs args)
        {
            if (Misc.isChecked(Program.DrawMenu, "drawDisable")) return;
            if (!Misc.isChecked(CooldonMenu, "drawCoolDowns")) return;

            // some menu verification here
            foreach (
                var Heroes in ObjectManager.Get<AIHeroClient>()
                .Where(h => h.IsValid && !h.IsMe && h.IsHPBarRendered))
            {

                for (int spell = 0; spell < SpellSlots.Count(); spell++)
                {
                    var getSpell = Heroes.Spellbook.GetSpell(SpellSlots[spell]);
                    X = (int)Heroes.HPBarPosition.X + 5 + (spell * 25);
                    Y = (int)Heroes.HPBarPosition.Y + 25;
                    var getSpellCD = getSpell.CooldownExpires - Game.Time;
                    var spellString = string.Format(getSpellCD < 1f ? "{0:0.0}" : "{0:0}", getSpellCD);

                    Text.Draw(getSpellCD > 0 ? spellString : SpellSlots[spell].ToString(), getSpell.Level < 1 ? Color.Gray : getSpellCD > 0 && getSpellCD <= 4 ? Color.Red : getSpellCD > 0 ? Color.Yellow : Color.White, new Vector2(X, Y));
                }

                for (int summoner = 0; summoner < SummonerSpellSlots.Count(); summoner++)
                {
                    SummonerSpellX = (int)Heroes.HPBarPosition.X - 15;
                    SummonerSpellY = (int)Heroes.HPBarPosition.Y + 1 + (summoner * 20);

                    var getSummoner = Heroes.Spellbook.GetSpell(SummonerSpellSlots[summoner]);
                    var getSummonerCD = getSummoner.CooldownExpires - Game.Time;
                    var summonerString = string.Format(getSummonerCD < 1f ? "{0:0.0}" : "{0:0}", getSummonerCD);

                    switch (getSummoner.Name.ToLower())
                    {
                        case "summonerflash":
                            GetSummonerSpellName = "F";
                            break;
                        case "summonerdot":
                            GetSummonerSpellName = "I";
                            break;

                        case "summonerheal":
                            GetSummonerSpellName = "H";
                            break;

                        case "summonerteleport":
                            GetSummonerSpellName = "T";
                            break;

                        case "summonerexhaust":
                            GetSummonerSpellName = "E";
                            break;

                        case "summonerhaste":
                            GetSummonerSpellName = "G";
                            break;

                        case "summonerbarrier":
                            GetSummonerSpellName = "B";
                            break;

                        case "summonerboost":
                            GetSummonerSpellName = "C";
                            break;

                        case "summonermana":
                            GetSummonerSpellName = "C";
                            break;

                        case "summonerclairvoyance":
                            GetSummonerSpellName = "C";
                            break;

                        case "summonerodingarrison":
                            GetSummonerSpellName = "G";
                            break;

                        case "summonersnowball":
                            GetSummonerSpellName = "M";
                            break;//keke

                        default:
                            GetSummonerSpellName = "S";
                            break;
                    }
                    Text.Draw(getSummonerCD > 0 ? summonerString : GetSummonerSpellName, getSummonerCD > 0 ?
                        Color.Red : Color.White, new Vector2(SummonerSpellX, SummonerSpellY));
                }

            }
        }
    }
}
