﻿/**
 *   Copyright (C) 2021 okaygo
 *
 *   https://github.com/misterokaygo/MapAssist/
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 **/

using MapAssist.Helpers;
using MapAssist.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;
using YamlDotNet.Serialization;

namespace MapAssist.Types
{
    [Flags]
    public enum ItemFlags : uint
    {
        IFLAG_NEWITEM = 0x00000001,
        IFLAG_TARGET = 0x00000002,
        IFLAG_TARGETING = 0x00000004,
        IFLAG_DELETED = 0x00000008,
        IFLAG_IDENTIFIED = 0x00000010,
        IFLAG_QUANTITY = 0x00000020,
        IFLAG_SWITCHIN = 0x00000040,
        IFLAG_SWITCHOUT = 0x00000080,
        IFLAG_BROKEN = 0x00000100,
        IFLAG_REPAIRED = 0x00000200,
        IFLAG_UNK1 = 0x00000400,
        IFLAG_SOCKETED = 0x00000800,
        IFLAG_NOSELL = 0x00001000,
        IFLAG_INSTORE = 0x00002000,
        IFLAG_NOEQUIP = 0x00004000,
        IFLAG_NAMED = 0x00008000,
        IFLAG_ISEAR = 0x00010000,
        IFLAG_STARTITEM = 0x00020000,
        IFLAG_UNK2 = 0x00040000,
        IFLAG_INIT = 0x00080000,
        IFLAG_UNK3 = 0x00100000,
        IFLAG_COMPACTSAVE = 0x00200000,
        IFLAG_ETHEREAL = 0x00400000,
        IFLAG_JUSTSAVED = 0x00800000,
        IFLAG_PERSONALIZED = 0x01000000,
        IFLAG_LOWQUALITY = 0x02000000,
        IFLAG_RUNEWORD = 0x04000000,
        IFLAG_ITEM = 0x08000000
    }

    public enum ItemQuality : uint
    {
        INFERIOR = 0x01, //0x01 Inferior
        NORMAL = 0x02, //0x02 Normal
        SUPERIOR = 0x03, //0x03 Superior
        MAGIC = 0x04, //0x04 Magic
        SET = 0x05, //0x05 Set
        RARE = 0x06, //0x06 Rare
        UNIQUE = 0x07, //0x07 Unique
        CRAFT = 0x08, //0x08 Crafted
        TEMPERED = 0x09 //0x09 Tempered
    }

    public enum InvPage : byte
    {
        INVENTORY = 0,
        EQUIP = 1,
        TRADE = 2,
        CUBE = 3,
        STASH = 4,
        BELT = 5,
        NULL = 255,
    }

    public enum StashType : byte
    {
        Body = 0,
        Personal = 1,
        Shared1 = 2,
        Shared2 = 3,
        Shared3 = 4,
        Belt = 5
    }

    public enum BodyLoc : byte
    {
        NONE, //Not Equipped
        HEAD, //Helm
        NECK, //Amulet
        TORSO, //Body Armor
        RARM, //Right-Hand
        LARM, //Left-Hand
        RRIN, //Right Ring
        LRIN, //Left Ring
        BELT, //Belt
        FEET, //Boots
        GLOVES, //Gloves
        SWRARM, //Right-Hand on Switch
        SWLARM //Left-Hand on Switch
    };

    public enum ItemMode : uint
    {
        STORED, //Item is in Storage (inventory, cube, Stash?)
        EQUIP, //Item is Equippped
        INBELT, //Item is in Belt Rows
        ONGROUND, //Item is on Ground
        ONCURSOR, //Item is on Cursor
        DROPPING, //Item is Being Dropped
        SOCKETED //Item is Socketed in another Item
    };

    public class Items
    {
        public static Dictionary<int, HashSet<string>> ItemUnitHashesSeen = new Dictionary<int, HashSet<string>>();
        public static Dictionary<int, HashSet<uint>> ItemUnitIdsSeen = new Dictionary<int, HashSet<uint>>();
        public static Dictionary<int, List<UnitAny>> ItemLog = new Dictionary<int, List<UnitAny>>();
        public static List<UnitAny> CurrentItemLog = new List<UnitAny>();
        public static Dictionary<string, LocalizedObj> LocalizedItems = new Dictionary<string, LocalizedObj>();
        public static Dictionary<int, List<Timer>> ItemLogTimers = new Dictionary<int, List<Timer>>();

        public static string ItemNameFromKey(string key)
        {
            LocalizedObj localItem;
            if (!LocalizedItems.TryGetValue(key, out localItem))
            {
                return "ItemNotFound";
            }

            var lang = MapAssistConfiguration.Loaded.LanguageCode;
            var prop = localItem.GetType().GetProperty(lang.ToString()).GetValue(localItem, null);

            return prop.ToString();
        }

        public static string ItemNameDisplay(uint txtFileNo)
        {
            string itemCode;
            if (!_ItemCodes.TryGetValue(txtFileNo, out itemCode))
            {
                return "ItemNotFound";
            }

            LocalizedObj localItem;
            if (!LocalizedItems.TryGetValue(itemCode, out localItem))
            {
                return "ItemNotFound";
            }

            var lang = MapAssistConfiguration.Loaded.LanguageCode;
            var prop = localItem.GetType().GetProperty(lang.ToString()).GetValue(localItem, null);

            return prop.ToString();
        }

        public static string ItemName(uint txtFileNo)
        {
            string itemCode;
            if (!_ItemCodes.TryGetValue(txtFileNo, out itemCode))
            {
                return "ItemNotFound";
            }

            LocalizedObj localItem;
            if (!LocalizedItems.TryGetValue(itemCode, out localItem))
            {
                return "ItemNotFound";
            }

            return localItem.enUS;
        }

        public static string UniqueName(uint txtFileNo)
        {
            string itemCode;
            if (!_ItemCodes.TryGetValue(txtFileNo, out itemCode))
            {
                return "Unique";
            }

            if (!_UniqueFromCode.TryGetValue(itemCode, out itemCode))
            {
                return "Unique";
            }

            LocalizedObj localItem;
            if (!LocalizedItems.TryGetValue(itemCode, out localItem))
            {
                return "Unique";
            }

            var lang = MapAssistConfiguration.Loaded.LanguageCode;
            var prop = localItem.GetType().GetProperty(lang.ToString()).GetValue(localItem, null);

            return prop.ToString();
        }

        public static string SetName(uint txtFileNo)
        {
            string itemCode;
            if (!_ItemCodes.TryGetValue(txtFileNo, out itemCode))
            {
                return "Set";
            }

            if (!_SetFromCode.TryGetValue(itemCode, out itemCode))
            {
                return "Set";
            }

            LocalizedObj localItem;
            if (!LocalizedItems.TryGetValue(itemCode, out localItem))
            {
                return "Set";
            }

            var lang = MapAssistConfiguration.Loaded.LanguageCode;
            var prop = localItem.GetType().GetProperty(lang.ToString()).GetValue(localItem, null);

            return prop.ToString();
        }

        public static void LogItem(UnitAny unit, int processId)
        {
            if ((!ItemUnitHashesSeen[processId].Contains(unit.ItemHash()) &&
                !ItemUnitIdsSeen[processId].Contains(unit.UnitId)))
            {
                (var pickupItem, _) = LootFilter.Filter(unit);
                if (!pickupItem)
                {
                    return;
                }
                if (MapAssistConfiguration.Loaded.ItemLog.PlaySoundOnDrop)
                {
                    AudioPlayer.PlayItemAlert();
                }

                ItemUnitHashesSeen[processId].Add(unit.ItemHash());
                ItemUnitIdsSeen[processId].Add(unit.UnitId);
                ItemLog[processId].Add(unit);
                var timer = new Timer(MapAssistConfiguration.Loaded.ItemLog.DisplayForSeconds * 1000);
                timer.Elapsed += (sender, args) => ItemLogTimerElapsed(sender, args, timer, processId);
                timer.Start();
                //keep track of timers in each d2r process
                if (ItemLogTimers.TryGetValue(processId, out var _))
                {
                    ItemLogTimers[processId].Add(timer);
                }
                else
                {
                    ItemLogTimers.Add(processId, new List<Timer>());
                    ItemLogTimers[processId].Add(timer);
                }
            }
        }

        public static string ItemLogDisplayName(UnitAny unit)
        {
            var itemBaseName = ItemName(unit.TxtFileNo);
            var itemSpecialName = "";
            var itemPrefix = "";
            var itemSuffix = "";

            (_, var rule) = LootFilter.Filter(unit);

            if (unit.IsInStore())
            {
                // TODO: List the name of seller
                itemPrefix += $"[Vendor] ";
            }

            if (rule == null) return itemPrefix + itemBaseName;

            if ((unit.ItemData.ItemFlags & ItemFlags.IFLAG_ETHEREAL) == ItemFlags.IFLAG_ETHEREAL)
            {
                itemPrefix += "[Eth] ";
            }

            if (unit.Stats.TryGetValue(Stat.NumSockets, out var numSockets))
            {
                itemPrefix += "[" + numSockets + " S] ";
            }

            if (unit.ItemData.ItemQuality == ItemQuality.SUPERIOR)
            {
                itemPrefix += "Sup. ";
            }

            if (rule.AllResist != null)
            {
                var itemAllRes = GetItemStatAllResist(unit);
                if (itemAllRes > 0)
                {
                    itemSuffix += $" ({itemAllRes} all res)";
                }
            }

            if (rule.AllSkills != null)
            {
                var itemAllSkills = GetItemStat(unit, Stat.AllSkills);
                if (itemAllSkills > 0)
                {
                    itemSuffix += $" (+{itemAllSkills} all skills)";
                }
            }

            if (rule.ClassSkills != null)
            {
                foreach (var subrule in rule.ClassSkills)
                {
                    var classSkills = GetItemStatAddClassSkills(unit, subrule.Key);
                    if (classSkills > 0)
                    {
                        itemSuffix += $" (+{classSkills} {subrule.Key} skills)";
                    }
                }
            }

            if (rule.ClassTabSkills != null)
            {
                foreach (var subrule in rule.ClassTabSkills)
                {
                    var classTabSkills = GetItemStatAddClassTabSkills(unit, subrule.Key);
                    if (classTabSkills > 0)
                    {
                        itemSuffix += $" (+{classTabSkills} {subrule.Key.Name()} skills)";
                    }
                }
            }

            if (rule.Skills != null)
            {
                foreach (var subrule in rule.Skills)
                {
                    var singleSkills = GetItemStatSingleSkills(unit, subrule.Key);
                    if (singleSkills > 0)
                    {
                        itemSuffix += $" (+{singleSkills} {subrule.Key.Name()})";
                    }
                }
            }

            if (rule.SkillCharges != null)
            {
                foreach (var subrule in rule.SkillCharges)
                {
                    var skillChanges = GetItemStatAddSkillCharges(unit, subrule.Key);
                    if (skillChanges > 0)
                    {
                        itemSuffix += $" ({skillChanges} {subrule.Key.Name()} charges)";
                    }
                }
            }

            foreach (var property in rule.GetType().GetProperties())
            {
                var yamlAttribute = property.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(YamlMemberAttribute));
                var propName = property.Name;

                if (yamlAttribute != null) propName = yamlAttribute.NamedArguments.FirstOrDefault(x => x.MemberName == "Alias").TypedValue.Value.ToString();

                if (property.PropertyType == typeof(int?) && Enum.TryParse<Stat>(property.Name, out var stat))
                {
                    var propertyValue = rule.GetType().GetProperty(property.Name).GetValue(rule, null);
                    var statValue = GetItemStat(unit, stat);

                    if (propertyValue != null && statValue > 0)
                    {
                        itemSuffix += $" ({statValue} {propName})";
                    }
                }
            }

            switch (unit.ItemData.ItemQuality)
            {
                case ItemQuality.UNIQUE:
                    itemSpecialName = UniqueName(unit.TxtFileNo) + " ";
                    break;

                case ItemQuality.SET:
                    itemSpecialName = SetName(unit.TxtFileNo) + " ";
                    break;
            }

            return itemPrefix + itemSpecialName + itemBaseName + itemSuffix;
        }

        public static Color ItemNameColor(UnitAny unit)
        {
            Color fontColor;
            if (unit == null || !Items.ItemColors.TryGetValue(unit.ItemData.ItemQuality, out fontColor))
            {
                // Invalid item quality
                return Color.Empty;
            }

            var isEth = (unit.ItemData.ItemFlags & ItemFlags.IFLAG_ETHEREAL) == ItemFlags.IFLAG_ETHEREAL;
            if (isEth && fontColor == Color.White)
            {
                return Items.ItemColors[ItemQuality.SUPERIOR];
            }

            if (unit.Stats.ContainsKey(Stat.NumSockets) && fontColor == Color.White)
            {
                return Items.ItemColors[ItemQuality.SUPERIOR];
            }

            if (unit.TxtFileNo >= 610 && unit.TxtFileNo <= 642)
            {
                // Runes
                return Items.ItemColors[ItemQuality.CRAFT];
            }

            switch (unit.TxtFileNo)
            {
                case 647: // Key of Terror
                case 648: // Key of Hate
                case 649: // Key of Destruction
                case 653: // Token of Absolution
                case 654: // Twisted Essence of Suffering
                case 655: // Charged Essense of Hatred
                case 656: // Burning Essence of Terror
                case 657: // Festering Essence of Destruction
                    return Items.ItemColors[ItemQuality.CRAFT];
            }

            return fontColor;
        }

        public static int GetItemStat(UnitAny unitAny, Stat stat)
        {
            return unitAny.Stats.TryGetValue(stat, out var statValue) ? statValue : 0;
        }

        public static int GetItemStatShifted(UnitAny unitAny, Stat stat, int shift)
        {
            return unitAny.Stats.TryGetValue(stat, out var statValue) ? statValue >> shift : 0;
        }

        public static int GetItemStatAllResist(UnitAny unitAny)
        {
            unitAny.Stats.TryGetValue(Stat.FireResist, out var fireRes);
            unitAny.Stats.TryGetValue(Stat.LightningResist, out var lightRes);
            unitAny.Stats.TryGetValue(Stat.ColdResist, out var coldRes);
            unitAny.Stats.TryGetValue(Stat.PoisonResist, out var psnRes);
            return new[] { fireRes, lightRes, coldRes, psnRes }.Min();
        }

        public static int GetItemStatAllAttributes(UnitAny unitAny)
        {
            unitAny.Stats.TryGetValue(Stat.Strength, out var strength);
            unitAny.Stats.TryGetValue(Stat.Dexterity, out var dexterity);
            unitAny.Stats.TryGetValue(Stat.Vitality, out var vitality);
            unitAny.Stats.TryGetValue(Stat.Energy, out var energy);
            return new[] { strength, dexterity, vitality, energy }.Min();
        }

        public static int GetItemStatAddClassSkills(UnitAny unitAny, Structs.PlayerClass playerClass)
        {
            if (unitAny.ItemStats.TryGetValue(Stat.AddClassSkills, out var itemStats) &&
                itemStats.TryGetValue((ushort)playerClass, out var addClassSkills))
            {
                return addClassSkills;
            }
            return 0;
        }

        public static int GetItemStatAddClassTabSkills(UnitAny unitAny, ClassTabs classTab)
        {
            if (unitAny.ItemStats.TryGetValue(Stat.AddSkillTab, out var itemStats) &&
                itemStats.TryGetValue((ushort)classTab, out var addSkillTab))
            {
                return addSkillTab;
            }
            return 0;
        }

        public static int GetItemStatAddSkillCharges(UnitAny unitAny, Skill skill)
        {
            if (unitAny.ItemStats.TryGetValue(Stat.ItemChargedSkill, out var itemStats))
            {
                foreach (var stat in itemStats)
                {
                    var skillId = stat.Key >> 6;
                    var level = stat.Key % (1 << 6);
                    if (skillId == (int)skill && itemStats.TryGetValue(stat.Key, out var data))
                    {
                        var maxCharges = data >> 8;
                        var currentCharges = data % (1 << 8);

                        return maxCharges;
                    }
                }
            }
            return 0;
        }

        public static int GetItemStatSingleSkills(UnitAny unitAny, Skill skill)
        {
            var itemSkillsStats = new List<Stat>()
            {
                Stat.SingleSkill,
                Stat.NonClassSkill,
            };

            foreach (var statType in itemSkillsStats)
            {
                if (unitAny.ItemStats.TryGetValue(statType, out var itemStats) &&
                    itemStats.TryGetValue((ushort)skill, out var skillLevel))
                {
                    return skillLevel;
                }
            }
            return 0;
        }

        public static void ItemLogTimerElapsed(object sender, ElapsedEventArgs args, Timer self, int procId)
        {
            if (ItemLog.TryGetValue(procId, out var itemLog))
            {
                if (itemLog.Count > 0)
                {
                    itemLog.RemoveAt(0);
                }

                if (ItemLogTimers.TryGetValue(procId, out var timer) && timer.Contains(self))
                {
                    try { timer.Remove(self); } catch (Exception) { } // This still randomly errors, even with the proper checks in place, hence the try catch block
                }
            }

            self.Dispose();
        }

        public static readonly Dictionary<ItemQuality, Color> ItemColors = new Dictionary<ItemQuality, Color>()
        {
            {ItemQuality.INFERIOR, Color.White},
            {ItemQuality.NORMAL, Color.White},
            {ItemQuality.SUPERIOR, Color.Gray},
            {ItemQuality.MAGIC, ColorTranslator.FromHtml("#4169E1")},
            {ItemQuality.SET, ColorTranslator.FromHtml("#00FF00")},
            {ItemQuality.RARE, ColorTranslator.FromHtml("#FFFF00")},
            {ItemQuality.UNIQUE, ColorTranslator.FromHtml("#A59263")},
            {ItemQuality.CRAFT, ColorTranslator.FromHtml("#FFAE00")},
        };

        public static readonly Dictionary<string, string> _SetFromCode = new Dictionary<string, string>()
        {
            {"lrg", "Civerb's Ward"},
            {"amu", "Set"}, //Amulet
            {"gsc", "Civerb's Cudgel"},
            {"mbt", "Hsarus' Iron Heel"},
            {"buc", "Hsarus' Iron Fist"},
            {"mbl", "Set"}, //Belt
            {"lsd", "Cleglaw's Tooth"},
            {"sml", "Cleglaw's Claw"},
            {"mgl", "Cleglaw's Pincers"},
            {"tgl", "Set"}, //Light Gauntlets
            {"crn", "Set"}, //Crown
            {"tbl", "Set"}, //Heavy Belt
            {"bsd", "Isenhart's Lightbrand"},
            {"gts", "Isenhart's Parry"},
            {"brs", "Isenhart's Case"},
            {"fhl", "Isenhart's Horns"},
            {"lbb", "Vidala's Barb"},
            {"tbt", "Vidala's Fetlock"},
            {"lea", "Vidala's Ambush"},
            {"kit", "Milabrega's Orb"},
            {"wsp", "Milabrega's Rod"},
            {"aar", "Milabrega's Robe"},
            {"bst", "Cathan's Rule"},
            {"chn", "Cathan's Mesh"},
            {"msk", "Cathan's Visage"},
            {"rin", "Set"}, //Ring
            {"mpi", "Tancred's Crowbill"},
            {"ful", "Tancred's Spine"},
            {"lbt", "Tancred's Hobnails"},
            {"bhm", "Tancred's Skull"},
            {"hgl", "Sigon's Gage"},
            {"ghm", "Sigon's Visor"},
            {"gth", "Sigon's Shelter"},
            {"hbt", "Sigon's Sabot"},
            {"hbl", "Sigon's Wrap"},
            {"tow", "Sigon's Guard"},
            {"cap", "Set"}, //Cap
            {"gwn", "Infernal Torch"},
            {"hlm", "Berserker's Headgear"},
            {"spl", "Berserker's Hauberk"},
            {"2ax", "Berserker's Hatchet"},
            {"lgl", "Death's Hand"},
            {"lbl", "Death's Guard"},
            {"wsd", "Death's Touch"},
            {"sbr", "Angelic Sickle"},
            {"rng", "Angelic Mantle"},
            {"swb", "Arctic Horn"},
            {"qui", "Arctic Furs"},
            {"vbl", "Arctic Binding"},
            {"wst", "Arcanna's Deathwand"},
            {"skp", "Arcanna's Head"},
            {"ltp", "Arcanna's Flesh"},
            {"xh9", "Natalya's Totem"},
            {"7qr", "Natalya's Mark"},
            {"ucl", "Natalya's Shadow"},
            {"xmb", "Natalya's Soul"},
            {"dr8", "Aldur's Stony Gaze"},
            {"uul", "Aldur's Deception"},
            {"9mt", "Aldur's Gauntlet"},
            {"xtb", "Aldur's Advance"},
            {"ba5", "Immortal King's Will"},
            {"uar", "Immortal King's Soul Cage"},
            {"zhb", "Immortal King's Detail"},
            {"xhg", "Immortal King's Forge"},
            {"xhb", "Immortal King's Pillar"},
            {"7m7", "Immortal King's Stone Crusher"},
            {"zmb", "Tal Rasha's Fire-Spun Cloth"},
            {"oba", "Tal Rasha's Lidless Eye"},
            {"uth", "Tal Rasha's Howling Wind"},
            {"xsk", "Tal Rasha's Horadric Crest"},
            {"urn", "Griswold's Valor"},
            {"xar", "Griswold's Heart"},
            {"7ws", "Griswolds's Redemption"},
            {"paf", "Griswold's Honor"},
            {"uh9", "Trang-Oul's Guise"},
            {"xul", "Trang-Oul's Scales"},
            {"ne9", "Trang-Oul's Wing"},
            {"xmg", "Trang-Oul's Claws"},
            {"utc", "Trang-Oul's Girth"},
            {"ci3", "M'avina's True Sight"},
            {"uld", "M'avina's Embrace"},
            {"xtg", "M'avina's Icy Clutch"},
            {"zvb", "M'avina's Tenet"},
            {"amc", "M'avina's Caster"},
            {"ulg", "Laying of Hands"},
            {"xlb", "Rite of Passage"},
            {"uui", "Spiritual Custodian"},
            {"umc", "Credendum"},
            {"7ma", "Dangoon's Teaching"},
            {"uts", "Heaven's Taebaek"},
            {"xrs", "Haemosu's Adament"},
            {"uhm", "Ondal's Almighty"},
            {"xhm", "Guillaume's Face"},
            {"ztb", "Wilhelm's Pride"},
            {"xvg", "Magnus' Skin"},
            {"xml", "Wihtstan's Guard"},
            {"xrn", "Hwanin's Splendor"},
            {"xcl", "Hwanin's Refuge"},
            {"9vo", "Hwanin's Justice"},
            {"7ls", "Sazabi's Cobalt Redeemer"},
            {"upl", "Sazabi's Ghost Liberator"},
            {"xhl", "Sazabi's Mental Sheath"},
            {"7gd", "Bul-Kathos' Sacred Charge"},
            {"7wd", "Bul-Kathos' Tribal Guardian"},
            {"xap", "Cow King's Horns"},
            {"stu", "Cow King's Hide"},
            {"vbt", "Set"}, //Heavy Boots
            {"6cs", "Naj's Puzzler"},
            {"ult", "Naj's Light Plate"},
            {"ci0", "Naj's Circlet"},
            {"vgl", "McAuley's Taboo"},
            {"bwn", "McAuley's Superstition"}
        };

        public static readonly Dictionary<string, string> _UniqueFromCode = new Dictionary<string, string>()
        {
            {"hax", "The Gnasher"},
            {"axe", "Deathspade"},
            {"2ax", "Bladebone"},
            {"mpi", "Mindrend"},
            {"wax", "Rakescar"},
            {"lax", "Fechmars Axe"},
            {"bax", "Goreshovel"},
            {"btx", "The Chieftan"},
            {"gax", "Brainhew"},
            {"gix", "The Humongous"},
            {"wnd", "Iros Torch"},
            {"ywn", "Maelstromwrath"},
            {"bwn", "Gravenspine"},
            {"gwn", "Umes Lament"},
            {"clb", "Felloak"},
            {"scp", "Knell Striker"},
            {"gsc", "Rusthandle"},
            {"wsp", "Stormeye"},
            {"spc", "Stoutnail"},
            {"mac", "Crushflange"},
            {"mst", "Bloodrise"},
            {"fla", "The Generals Tan Do Li Ga"},
            {"whm", "Ironstone"},
            {"mau", "Bonesob"},
            {"gma", "Steeldriver"},
            {"ssd", "Rixots Keen"},
            {"scm", "Blood Crescent"},
            {"sbr", "Krintizs Skewer"},
            {"flc", "Gleamscythe"},
            {"crs", "Azurewrath"},
            {"bsd", "Griswolds Edge"},
            {"lsd", "Hellplague"},
            {"wsd", "Culwens Point"},
            {"2hs", "Shadowfang"},
            {"clm", "Soulflay"},
            {"gis", "Kinemils Awl"},
            {"bsw", "Blacktongue"},
            {"flb", "Ripsaw"},
            {"gsd", "The Patriarch"},
            {"dgr", "Gull"},
            {"dir", "The Diggler"},
            {"kri", "The Jade Tan Do"},
            {"bld", "Irices Shard"},
            {"spr", "The Dragon Chang"},
            {"tri", "Razortine"},
            {"brn", "Bloodthief"},
            {"spt", "Lance of Yaggai"},
            {"pik", "The Tannr Gorerod"},
            {"bar", "Dimoaks Hew"},
            {"vou", "Steelgoad"},
            {"scy", "Soul Harvest"},
            {"pax", "The Battlebranch"},
            {"hal", "Woestave"},
            {"wsc", "The Grim Reaper"},
            {"sst", "Bane Ash"},
            {"lst", "Serpent Lord"},
            {"cst", "Lazarus Spire"},
            {"bst", "The Salamander"},
            {"wst", "The Iron Jang Bong"},
            {"sbw", "Pluckeye"},
            {"hbw", "Witherstring"},
            {"lbw", "Rimeraven"},
            {"cbw", "Piercerib"},
            {"sbb", "Pullspite"},
            {"lbb", "Wizendraw"},
            {"swb", "Hellclap"},
            {"lwb", "Blastbark"},
            {"lxb", "Leadcrow"},
            {"mxb", "Ichorsting"},
            {"hxb", "Hellcast"},
            {"rxb", "Doomspittle"},
            {"cap", "Biggin's Bonnet"},
            {"skp", "Tarnhelm"},
            {"hlm", "Coif of Glory"},
            {"fhl", "Duskdeep"},
            {"bhm", "Wormskull"},
            {"ghm", "Howltusk"},
            {"crn", "Undead Crown"},
            {"msk", "The Face of Horror"},
            {"qui", "Greyform"},
            {"lea", "Blinkbats Form"},
            {"hla", "The Centurion"},
            {"stu", "Twitchthroe"},
            {"rng", "Darkglow"},
            {"scl", "Hawkmail"},
            {"chn", "Sparking Mail"},
            {"brs", "Venomsward"},
            {"spl", "Iceblink"},
            {"plt", "Boneflesh"},
            {"fld", "Rockfleece"},
            {"gth", "Rattlecage"},
            {"ful", "Goldskin"},
            {"aar", "Victors Silk"},
            {"ltp", "Heavenly Garb"},
            {"buc", "Pelta Lunata"},
            {"sml", "Umbral Disk"},
            {"lrg", "Stormguild"},
            {"bsh", "Wall of the Eyeless"},
            {"spk", "Swordback Hold"},
            {"kit", "Steelclash"},
            {"tow", "Bverrit Keep"},
            {"gts", "The Ward"},
            {"lgl", "The Hand of Broc"},
            {"vgl", "Bloodfist"},
            {"mgl", "Chance Guards"},
            {"tgl", "Magefist"},
            {"hgl", "Frostburn"},
            {"lbt", "Hotspur"},
            {"vbt", "Gorefoot"},
            {"mbt", "Treads of Cthon"},
            {"tbt", "Goblin Toe"},
            {"hbt", "Tearhaunch"},
            {"lbl", "Lenyms Cord"},
            {"vbl", "Snakecord"},
            {"mbl", "Nightsmoke"},
            {"tbl", "Goldwrap"},
            {"hbl", "Bladebuckle"},
            {"amu", "Unique"}, //Amulet
            {"rin", "Unique"}, //Ring
            {"vip", "Amulet of the Viper"},
            {"msf", "Staff of Kings"},
            {"hst", "Horadric Staff"},
            {"hfh", "Hell Forge Hammer"},
            {"qf1", "KhalimFlail"},
            {"qf2", "SuperKhalimFlail"},
            {"9ha", "Coldkill"},
            {"9ax", "Butcher's Pupil"},
            {"92a", "Islestrike"},
            {"9mp", "Pompe's Wrath"},
            {"9wa", "Guardian Naga"},
            {"9la", "Warlord's Trust"},
            {"9ba", "Spellsteel"},
            {"9bt", "Stormrider"},
            {"9ga", "Boneslayer Blade"},
            {"9gi", "The Minataur"},
            {"9wn", "Suicide Branch"},
            {"9yw", "Carin Shard"},
            {"9bw", "Arm of King Leoric"},
            {"9gw", "Blackhand Key"},
            {"9cl", "Dark Clan Crusher"},
            {"9sc", "Zakarum's Hand"},
            {"9qs", "The Fetid Sprinkler"},
            {"9ws", "Hand of Blessed Light"},
            {"9sp", "Fleshrender"},
            {"9ma", "Sureshrill Frost"},
            {"9mt", "Moonfall"},
            {"9fl", "Baezil's Vortex"},
            {"9wh", "Earthshaker"},
            {"9m9", "Bloodtree Stump"},
            {"9gm", "The Gavel of Pain"},
            {"9ss", "Bloodletter"},
            {"9sm", "Coldsteel Eye"},
            {"9sb", "Hexfire"},
            {"9fc", "Blade of Ali Baba"},
            {"9cr", "Ginther's Rift"},
            {"9bs", "Headstriker"},
            {"9ls", "Plague Bearer"},
            {"9wd", "The Atlantian"},
            {"92h", "Crainte Vomir"},
            {"9cm", "Bing Sz Wang"},
            {"9gs", "The Vile Husk"},
            {"9b9", "Cloudcrack"},
            {"9fb", "Todesfaelle Flamme"},
            {"9gd", "Swordguard"},
            {"9dg", "Spineripper"},
            {"9di", "Heart Carver"},
            {"9kr", "Blackbog's Sharp"},
            {"9bl", "Stormspike"},
            {"9sr", "The Impaler"},
            {"9tr", "Kelpie Snare"},
            {"9br", "Soulfeast Tine"},
            {"9st", "Hone Sundan"},
            {"9p9", "Spire of Honor"},
            {"9b7", "The Meat Scraper"},
            {"9vo", "Blackleach Blade"},
            {"9s8", "Athena's Wrath"},
            {"9pa", "Pierre Tombale Couant"},
            {"9h9", "Husoldal Evo"},
            {"9wc", "Grim's Burning Dead"},
            {"8ss", "Razorswitch"},
            {"8ls", "Ribcracker"},
            {"8cs", "Chromatic Ire"},
            {"8bs", "Warpspear"},
            {"8ws", "Skullcollector"},
            {"8sb", "Skystrike"},
            {"8hb", "Riphook"},
            {"8lb", "Kuko Shakaku"},
            {"8cb", "Endlesshail"},
            {"8s8", "Whichwild String"},
            {"8l8", "Cliffkiller"},
            {"8sw", "Magewrath"},
            {"8lw", "Godstrike Arch"},
            {"8lx", "Langer Briser"},
            {"8mx", "Pus Spiter"},
            {"8hx", "Buriza-Do Kyanon"},
            {"8rx", "Demon Machine"},
            {"xap", "Peasent Crown"},
            {"xkp", "Rockstopper"},
            {"xlm", "Stealskull"},
            {"xhl", "Darksight Helm"},
            {"xhm", "Valkiry Wing"},
            {"xrn", "Crown of Thieves"},
            {"xsk", "Blackhorn's Face"},
            {"xh9", "Vampiregaze"},
            {"xui", "The Spirit Shroud"},
            {"xea", "Skin of the Vipermagi"},
            {"xla", "Skin of the Flayerd One"},
            {"xtu", "Ironpelt"},
            {"xng", "Spiritforge"},
            {"xcl", "Crow Caw"},
            {"xhn", "Shaftstop"},
            {"xrs", "Duriel's Shell"},
            {"xpl", "Skullder's Ire"},
            {"xlt", "Guardian Angel"},
            {"xld", "Toothrow"},
            {"xth", "Atma's Wail"},
            {"xul", "Black Hades"},
            {"xar", "Corpsemourn"},
            {"xtp", "Que-Hegan's Wisdon"},
            {"xuc", "Visceratuant"},
            {"xml", "Mosers Blessed Circle"},
            {"xrg", "Stormchaser"},
            {"xit", "Tiamat's Rebuke"},
            {"xow", "Kerke's Sanctuary"},
            {"xts", "Radimant's Sphere"},
            {"xsh", "Lidless Wall"},
            {"xpk", "Lance Guard"},
            {"xlg", "Venom Grip"},
            {"xvg", "Gravepalm"},
            {"xmg", "Ghoulhide"},
            {"xtg", "Lavagout"},
            {"xhg", "Hellmouth"},
            {"xlb", "Infernostride"},
            {"xvb", "Waterwalk"},
            {"xmb", "Silkweave"},
            {"xtb", "Wartraveler"},
            {"xhb", "Gorerider"},
            {"zlb", "String of Ears"},
            {"zvb", "Razortail"},
            {"zmb", "Gloomstrap"},
            {"ztb", "Snowclash"},
            {"zhb", "Thudergod's Vigor"},
            {"uap", "Harlequin Crest"},
            {"uhm", "Unique"}, //Spired Helm
            {"utu", "The Gladiator's Bane"},
            {"upl", "Arkaine's Valor"},
            {"uml", "Blackoak Shield"},
            {"uit", "Stormshield"},
            {"7bt", "Hellslayer"},
            {"7ga", "Messerschmidt's Reaver"},
            {"7mt", "Baranar's Star"},
            {"7b7", "Doombringer"},
            {"7gd", "The Grandfather"},
            {"7dg", "Wizardspike"},
            {"7wc", "Stormspire"},
            {"6l7", "Eaglehorn"},
            {"6lw", "Windforce"},
            {"baa", "Arreat's Face"},
            {"nea", "Homunculus"},
            {"ama", "Titan's Revenge"},
            {"am7", "Lycander's Aim"},
            {"am9", "Lycander's Flank"},
            {"oba", "The Oculus"},
            {"pa9", "Herald of Zakarum"},
            {"9tw", "Cutthroat1"},
            {"dra", "Jalal's Mane"},
            {"9ta", "The Scalper"},
            {"7sb", "Bloodmoon"},
            {"7sm", "Djinnslayer"},
            {"9tk", "Deathbit"},
            {"7bk", "Warshrike"},
            {"6rx", "Gutsiphon"},
            {"7ha", "Razoredge"},
            {"7sp", "Demonlimb"},
            {"ulm", "Unique"}, //Armet
            {"7pa", "Tomb Reaver"},
            {"7gw", "Deaths's Web"},
            {"7cr", "Unique"}, //Phase Blade
            {"7kr", "Fleshripper"},
            {"7fl", "Unique"}, //Scourge
            {"7wh", "Unique"}, //Legendary Mallet
            {"7wb", "Jadetalon"},
            {"uhb", "Shadowdancer"},
            {"drb", "Cerebus"},
            {"uar", "Unique"}, //Sacred Armor
            {"umg", "Souldrain"},
            {"72a", "Runemaster"},
            {"7wa", "Deathcleaver"},
            {"7gi", "Executioner's Justice"},
            {"amd", "Stoneraven"},
            {"uld", "Leviathan"},
            {"7ts", "Gargoyle's Bite"},
            {"7b8", "Lacerator"},
            {"6ws", "Mang Song's Lesson"},
            {"7br", "Viperfork"},
            {"7ba", "Ethereal Edge"},
            {"bad", "Demonhorn's Edge"},
            {"7s8", "The Reaper's Toll"},
            {"drd", "Spiritkeeper"},
            {"6hx", "Hellrack"},
            {"pac", "Alma Negra"},
            {"nef", "Darkforge Spawn"},
            {"6sw", "Widowmaker"},
            {"amb", "Bloodraven's Charge"},
            {"7bl", "Ghostflame"},
            {"7cs", "Shadowkiller"},
            {"7ta", "Gimmershred"},
            {"ci3", "Griffon's Eye"},
            {"7m7", "Windhammer"},
            {"amf", "Thunderstroke"},
            {"7s7", "Demon's Arch"},
            {"nee", "Boneflame"},
            {"7p7", "Steelpillar"},
            {"urn", "Crown of Ages"},
            {"usk", "Andariel's Visage"},
            {"pae", "Dragonscale"},
            {"uul", "Steel Carapice"},
            {"uow", "Medusa's Gaze"},
            {"dre", "Ravenlore"},
            {"7bw", "Boneshade"},
            {"7gs", "Flamebellow"},
            {"obf", "Fathom"},
            {"bac", "Wolfhowl"},
            {"uts", "Spirit Ward"},
            {"ci2", "Kira's Guardian"},
            {"uui", "Ormus' Robes"},
            {"cm3", "Gheed's Fortune"},
            {"bae", "Halaberd's Reign"},
            {"upk", "Spike Thorn"},
            {"uvg", "Dracul's Grasp"},
            {"7ls", "Frostwind"},
            {"obc", "Eschuta's temper"},
            {"7lw", "Firelizard's Talons"},
            {"uvb", "Sandstorm Trek"},
            {"umb", "Marrowwalk"},
            {"ulc", "Arachnid Mesh"},
            {"uvc", "Nosferatu's Coil"},
            {"umc", "Verdugo's Hearty Cord"},
            {"uh9", "Giantskull"},
            {"7ws", "Ironward"},
            {"cm1", "Annihilus"},
            {"7sr", "Arioc's Needle"},
            {"7mp", "Cranebeak"},
            {"7cl", "Nord's Tenderizer"},
            {"7gm", "Unique"}, //Thunder Maul
            {"7gl", "Wraithflight"},
            {"7o7", "Bonehew"},
            {"6cs", "Ondal's Wisdom"},
            {"7sc", "Unique"}, //Mighty Scepter
            {"ush", "Headhunter's Glory"},
            {"uhg", "Steelrend"},
            {"jew", "Rainbow Facet"},
            {"cm2", "Hellfire Torch"}
        };

        public static readonly Dictionary<uint, string> _ItemCodes = new Dictionary<uint, string>()
        {
            {0, "hax"},
            {1, "axe"},
            {2, "2ax"},
            {3, "mpi"},
            {4, "wax"},
            {5, "lax"},
            {6, "bax"},
            {7, "btx"},
            {8, "gax"},
            {9, "gix"},
            {10, "wnd"},
            {11, "ywn"},
            {12, "bwn"},
            {13, "gwn"},
            {14, "clb"},
            {15, "scp"},
            {16, "gsc"},
            {17, "wsp"},
            {18, "spc"},
            {19, "mac"},
            {20, "mst"},
            {21, "fla"},
            {22, "whm"},
            {23, "mau"},
            {24, "gma"},
            {25, "ssd"},
            {26, "scm"},
            {27, "sbr"},
            {28, "flc"},
            {29, "crs"},
            {30, "bsd"},
            {31, "lsd"},
            {32, "wsd"},
            {33, "2hs"},
            {34, "clm"},
            {35, "gis"},
            {36, "bsw"},
            {37, "flb"},
            {38, "gsd"},
            {39, "dgr"},
            {40, "dir"},
            {41, "kri"},
            {42, "bld"},
            {43, "tkf"},
            {44, "tax"},
            {45, "bkf"},
            {46, "bal"},
            {47, "jav"},
            {48, "pil"},
            {49, "ssp"},
            {50, "glv"},
            {51, "tsp"},
            {52, "spr"},
            {53, "tri"},
            {54, "brn"},
            {55, "spt"},
            {56, "pik"},
            {57, "bar"},
            {58, "vou"},
            {59, "scy"},
            {60, "pax"},
            {61, "hal"},
            {62, "wsc"},
            {63, "sst"},
            {64, "lst"},
            {65, "cst"},
            {66, "bst"},
            {67, "wst"},
            {68, "sbw"},
            {69, "hbw"},
            {70, "lbw"},
            {71, "cbw"},
            {72, "sbb"},
            {73, "lbb"},
            {74, "swb"},
            {75, "lwb"},
            {76, "lxb"},
            {77, "mxb"},
            {78, "hxb"},
            {79, "rxb"},
            {80, "gps"},
            {81, "ops"},
            {82, "gpm"},
            {83, "opm"},
            {84, "gpl"},
            {85, "opl"},
            {86, "d33"},
            {87, "g33"},
            {88, "leg"},
            {89, "hdm"},
            {90, "hfh"},
            {91, "hst"},
            {92, "msf"},
            {93, "9ha"},
            {94, "9ax"},
            {95, "92a"},
            {96, "9mp"},
            {97, "9wa"},
            {98, "9la"},
            {99, "9ba"},
            {100, "9bt"},
            {101, "9ga"},
            {102, "9gi"},
            {103, "9wn"},
            {104, "9yw"},
            {105, "9bw"},
            {106, "9gw"},
            {107, "9cl"},
            {108, "9sc"},
            {109, "9qs"},
            {110, "9ws"},
            {111, "9sp"},
            {112, "9ma"},
            {113, "9mt"},
            {114, "9fl"},
            {115, "9wh"},
            {116, "9m9"},
            {117, "9gm"},
            {118, "9ss"},
            {119, "9sm"},
            {120, "9sb"},
            {121, "9fc"},
            {122, "9cr"},
            {123, "9bs"},
            {124, "9ls"},
            {125, "9wd"},
            {126, "92h"},
            {127, "9cm"},
            {128, "9gs"},
            {129, "9b9"},
            {130, "9fb"},
            {131, "9gd"},
            {132, "9dg"},
            {133, "9di"},
            {134, "9kr"},
            {135, "9bl"},
            {136, "9tk"},
            {137, "9ta"},
            {138, "9bk"},
            {139, "9b8"},
            {140, "9ja"},
            {141, "9pi"},
            {142, "9s9"},
            {143, "9gl"},
            {144, "9ts"},
            {145, "9sr"},
            {146, "9tr"},
            {147, "9br"},
            {148, "9st"},
            {149, "9p9"},
            {150, "9b7"},
            {151, "9vo"},
            {152, "9s8"},
            {153, "9pa"},
            {154, "9h9"},
            {155, "9wc"},
            {156, "8ss"},
            {157, "8ls"},
            {158, "8cs"},
            {159, "8bs"},
            {160, "8ws"},
            {161, "8sb"},
            {162, "8hb"},
            {163, "8lb"},
            {164, "8cb"},
            {165, "8s8"},
            {166, "8l8"},
            {167, "8sw"},
            {168, "8lw"},
            {169, "8lx"},
            {170, "8mx"},
            {171, "8hx"},
            {172, "8rx"},
            {173, "qf1"},
            {174, "qf2"},
            {175, "ktr"},
            {176, "wrb"},
            {177, "axf"},
            {178, "ces"},
            {179, "clw"},
            {180, "btl"},
            {181, "skr"},
            {182, "9ar"},
            {183, "9wb"},
            {184, "9xf"},
            {185, "9cs"},
            {186, "9lw"},
            {187, "9tw"},
            {188, "9qr"},
            {189, "7ar"},
            {190, "7wb"},
            {191, "7xf"},
            {192, "7cs"},
            {193, "7lw"},
            {194, "7tw"},
            {195, "7qr"},
            {196, "7ha"},
            {197, "7ax"},
            {198, "72a"},
            {199, "7mp"},
            {200, "7wa"},
            {201, "7la"},
            {202, "7ba"},
            {203, "7bt"},
            {204, "7ga"},
            {205, "7gi"},
            {206, "7wn"},
            {207, "7yw"},
            {208, "7bw"},
            {209, "7gw"},
            {210, "7cl"},
            {211, "7sc"},
            {212, "7qs"},
            {213, "7ws"},
            {214, "7sp"},
            {215, "7ma"},
            {216, "7mt"},
            {217, "7fl"},
            {218, "7wh"},
            {219, "7m7"},
            {220, "7gm"},
            {221, "7ss"},
            {222, "7sm"},
            {223, "7sb"},
            {224, "7fc"},
            {225, "7cr"},
            {226, "7bs"},
            {227, "7ls"},
            {228, "7wd"},
            {229, "72h"},
            {230, "7cm"},
            {231, "7gs"},
            {232, "7b7"},
            {233, "7fb"},
            {234, "7gd"},
            {235, "7dg"},
            {236, "7di"},
            {237, "7kr"},
            {238, "7bl"},
            {239, "7tk"},
            {240, "7ta"},
            {241, "7bk"},
            {242, "7b8"},
            {243, "7ja"},
            {244, "7pi"},
            {245, "7s7"},
            {246, "7gl"},
            {247, "7ts"},
            {248, "7sr"},
            {249, "7tr"},
            {250, "7br"},
            {251, "7st"},
            {252, "7p7"},
            {253, "7o7"},
            {254, "7vo"},
            {255, "7s8"},
            {256, "7pa"},
            {257, "7h7"},
            {258, "7wc"},
            {259, "6ss"},
            {260, "6ls"},
            {261, "6cs"},
            {262, "6bs"},
            {263, "6ws"},
            {264, "6sb"},
            {265, "6hb"},
            {266, "6lb"},
            {267, "6cb"},
            {268, "6s7"},
            {269, "6l7"},
            {270, "6sw"},
            {271, "6lw"},
            {272, "6lx"},
            {273, "6mx"},
            {274, "6hx"},
            {275, "6rx"},
            {276, "ob1"},
            {277, "ob2"},
            {278, "ob3"},
            {279, "ob4"},
            {280, "ob5"},
            {281, "am1"},
            {282, "am2"},
            {283, "am3"},
            {284, "am4"},
            {285, "am5"},
            {286, "ob6"},
            {287, "ob7"},
            {288, "ob8"},
            {289, "ob9"},
            {290, "oba"},
            {291, "am6"},
            {292, "am7"},
            {293, "am8"},
            {294, "am9"},
            {295, "ama"},
            {296, "obb"},
            {297, "obc"},
            {298, "obd"},
            {299, "obe"},
            {300, "obf"},
            {301, "amb"},
            {302, "amc"},
            {303, "amd"},
            {304, "ame"},
            {305, "amf"},
            {306, "cap"},
            {307, "skp"},
            {308, "hlm"},
            {309, "fhl"},
            {310, "ghm"},
            {311, "crn"},
            {312, "msk"},
            {313, "qui"},
            {314, "lea"},
            {315, "hla"},
            {316, "stu"},
            {317, "rng"},
            {318, "scl"},
            {319, "chn"},
            {320, "brs"},
            {321, "spl"},
            {322, "plt"},
            {323, "fld"},
            {324, "gth"},
            {325, "ful"},
            {326, "aar"},
            {327, "ltp"},
            {328, "buc"},
            {329, "sml"},
            {330, "lrg"},
            {331, "kit"},
            {332, "tow"},
            {333, "gts"},
            {334, "lgl"},
            {335, "vgl"},
            {336, "mgl"},
            {337, "tgl"},
            {338, "hgl"},
            {339, "lbt"},
            {340, "vbt"},
            {341, "mbt"},
            {342, "tbt"},
            {343, "hbt"},
            {344, "lbl"},
            {345, "vbl"},
            {346, "mbl"},
            {347, "tbl"},
            {348, "hbl"},
            {349, "bhm"},
            {350, "bsh"},
            {351, "spk"},
            {352, "xap"},
            {353, "xkp"},
            {354, "xlm"},
            {355, "xhl"},
            {356, "xhm"},
            {357, "xrn"},
            {358, "xsk"},
            {359, "xui"},
            {360, "xea"},
            {361, "xla"},
            {362, "xtu"},
            {363, "xng"},
            {364, "xcl"},
            {365, "xhn"},
            {366, "xrs"},
            {367, "xpl"},
            {368, "xlt"},
            {369, "xld"},
            {370, "xth"},
            {371, "xul"},
            {372, "xar"},
            {373, "xtp"},
            {374, "xuc"},
            {375, "xml"},
            {376, "xrg"},
            {377, "xit"},
            {378, "xow"},
            {379, "xts"},
            {380, "xlg"},
            {381, "xvg"},
            {382, "xmg"},
            {383, "xtg"},
            {384, "xhg"},
            {385, "xlb"},
            {386, "xvb"},
            {387, "xmb"},
            {388, "xtb"},
            {389, "xhb"},
            {390, "zlb"},
            {391, "zvb"},
            {392, "zmb"},
            {393, "ztb"},
            {394, "zhb"},
            {395, "xh9"},
            {396, "xsh"},
            {397, "xpk"},
            {398, "dr1"},
            {399, "dr2"},
            {400, "dr3"},
            {401, "dr4"},
            {402, "dr5"},
            {403, "ba1"},
            {404, "ba2"},
            {405, "ba3"},
            {406, "ba4"},
            {407, "ba5"},
            {408, "pa1"},
            {409, "pa2"},
            {410, "pa3"},
            {411, "pa4"},
            {412, "pa5"},
            {413, "ne1"},
            {414, "ne2"},
            {415, "ne3"},
            {416, "ne4"},
            {417, "ne5"},
            {418, "ci0"},
            {419, "ci1"},
            {420, "ci2"},
            {421, "ci3"},
            {422, "uap"},
            {423, "ukp"},
            {424, "ulm"},
            {425, "uhl"},
            {426, "uhm"},
            {427, "urn"},
            {428, "usk"},
            {429, "uui"},
            {430, "uea"},
            {431, "ula"},
            {432, "utu"},
            {433, "ung"},
            {434, "ucl"},
            {435, "uhn"},
            {436, "urs"},
            {437, "upl"},
            {438, "ult"},
            {439, "uld"},
            {440, "uth"},
            {441, "uul"},
            {442, "uar"},
            {443, "utp"},
            {444, "uuc"},
            {445, "uml"},
            {446, "urg"},
            {447, "uit"},
            {448, "uow"},
            {449, "uts"},
            {450, "ulg"},
            {451, "uvg"},
            {452, "umg"},
            {453, "utg"},
            {454, "uhg"},
            {455, "ulb"},
            {456, "uvb"},
            {457, "umb"},
            {458, "utb"},
            {459, "uhb"},
            {460, "ulc"},
            {461, "uvc"},
            {462, "umc"},
            {463, "utc"},
            {464, "uhc"},
            {465, "uh9"},
            {466, "ush"},
            {467, "upk"},
            {468, "dr6"},
            {469, "dr7"},
            {470, "dr8"},
            {471, "dr9"},
            {472, "dra"},
            {473, "ba6"},
            {474, "ba7"},
            {475, "ba8"},
            {476, "ba9"},
            {477, "baa"},
            {478, "pa6"},
            {479, "pa7"},
            {480, "pa8"},
            {481, "pa9"},
            {482, "paa"},
            {483, "ne6"},
            {484, "ne7"},
            {485, "ne8"},
            {486, "ne9"},
            {487, "nea"},
            {488, "drb"},
            {489, "drc"},
            {490, "drd"},
            {491, "dre"},
            {492, "drf"},
            {493, "bab"},
            {494, "bac"},
            {495, "bad"},
            {496, "bae"},
            {497, "baf"},
            {498, "pab"},
            {499, "pac"},
            {500, "pad"},
            {501, "pae"},
            {502, "paf"},
            {503, "neb"},
            {504, "neg"},
            {505, "ned"},
            {506, "nee"},
            {507, "nef"},
            {508, "elx"},
            {509, "hpo"},
            {510, "mpo"},
            {511, "hpf"},
            {512, "mpf"},
            {513, "vps"},
            {514, "yps"},
            {515, "rvs"},
            {516, "rvl"},
            {517, "wms"},
            {518, "tbk"},
            {519, "ibk"},
            {520, "amu"},
            {521, "vip"},
            {522, "rin"},
            {523, "gld"},
            {524, "bks"},
            {525, "bkd"},
            {526, "aqv"},
            {527, "tch"},
            {528, "cqv"},
            {529, "tsc"},
            {530, "isc"},
            {531, "hrt"},
            {532, "brz"},
            {533, "jaw"},
            {534, "eyz"},
            {535, "hrn"},
            {536, "tal"},
            {537, "flg"},
            {538, "fng"},
            {539, "qll"},
            {540, "sol"},
            {541, "scz"},
            {542, "spe"},
            {543, "key"},
            {544, "luv"},
            {545, "xyz"},
            {546, "j34"},
            {547, "g34"},
            {548, "bbb"},
            {549, "box"},
            {550, "tr1"},
            {551, "mss"},
            {552, "ass"},
            {553, "qey"},
            {554, "qhr"},
            {555, "qbr"},
            {556, "ear"},
            {557, "gcv"},
            {558, "gfv"},
            {559, "gsv"},
            {560, "gzv"},
            {561, "gpv"},
            {562, "gcy"},
            {563, "gfy"},
            {564, "gsy"},
            {565, "gly"},
            {566, "gpy"},
            {567, "gcb"},
            {568, "gfb"},
            {569, "gsb"},
            {570, "glb"},
            {571, "gpb"},
            {572, "gcg"},
            {573, "gfg"},
            {574, "gsg"},
            {575, "glg"},
            {576, "gpg"},
            {577, "gcr"},
            {578, "gfr"},
            {579, "gsr"},
            {580, "glr"},
            {581, "gpr"},
            {582, "gcw"},
            {583, "gfw"},
            {584, "gsw"},
            {585, "glw"},
            {586, "gpw"},
            {587, "hp1"},
            {588, "hp2"},
            {589, "hp3"},
            {590, "hp4"},
            {591, "hp5"},
            {592, "mp1"},
            {593, "mp2"},
            {594, "mp3"},
            {595, "mp4"},
            {596, "mp5"},
            {597, "skc"},
            {598, "skf"},
            {599, "sku"},
            {600, "skl"},
            {601, "skz"},
            {602, "hrb"},
            {603, "cm1"},
            {604, "cm2"},
            {605, "cm3"},
            {606, "rps"},
            {607, "rpl"},
            {608, "bps"},
            {609, "bpl"},
            {610, "r01"},
            {611, "r02"},
            {612, "r03"},
            {613, "r04"},
            {614, "r05"},
            {615, "r06"},
            {616, "r07"},
            {617, "r08"},
            {618, "r09"},
            {619, "r10"},
            {620, "r11"},
            {621, "r12"},
            {622, "r13"},
            {623, "r14"},
            {624, "r15"},
            {625, "r16"},
            {626, "r17"},
            {627, "r18"},
            {628, "r19"},
            {629, "r20"},
            {630, "r21"},
            {631, "r22"},
            {632, "r23"},
            {633, "r24"},
            {634, "r25"},
            {635, "r26"},
            {636, "r27"},
            {637, "r28"},
            {638, "r29"},
            {639, "r30"},
            {640, "r31"},
            {641, "r32"},
            {642, "r33"},
            {643, "jew"},
            {644, "ice"},
            {645, "0sc"},
            {646, "tr2"},
            {647, "pk1"},
            {648, "pk2"},
            {649, "pk3"},
            {650, "dhn"},
            {651, "bey"},
            {652, "mbr"},
            {653, "toa"},
            {654, "tes"},
            {655, "ceh"},
            {656, "bet"},
            {657, "fed"},
            {658, "std"}
        };

        public static readonly Dictionary<uint, string> _ItemNames = new Dictionary<uint, string>()
        {
            {0, "Hand Axe"},
            {1, "Axe"},
            {2, "Double Axe"},
            {3, "Military Pick"},
            {4, "War Axe"},
            {5, "Large Axe"},
            {6, "Broad Axe"},
            {7, "Battle Axe"},
            {8, "Great Axe"},
            {9, "Giant Axe"},
            {10, "Wand"},
            {11, "Yew Wand"},
            {12, "Bone Wand"},
            {13, "Grim Wand"},
            {14, "Club"},
            {15, "Scepter"},
            {16, "Grand Scepter"},
            {17, "War Scepter"},
            {18, "Spiked Club"},
            {19, "Mace"},
            {20, "Morning Star"},
            {21, "Flail"},
            {22, "War Hammer"},
            {23, "Maul"},
            {24, "Great Maul"},
            {25, "Short Sword"},
            {26, "Scimitar"},
            {27, "Saber"},
            {28, "Falchion"},
            {29, "Crystal Sword"},
            {30, "Broad Sword"},
            {31, "Long Sword"},
            {32, "War Sword"},
            {33, "Two-Handed Sword"},
            {34, "Claymore"},
            {35, "Giant Sword"},
            {36, "Bastard Sword"},
            {37, "Flamberge"},
            {38, "Great Sword"},
            {39, "Dagger"},
            {40, "Dirk"},
            {41, "Kriss"},
            {42, "Blade"},
            {43, "Throwing Knife"},
            {44, "Throwing Axe"},
            {45, "Balanced Knife"},
            {46, "Balanced Axe"},
            {47, "Javelin"},
            {48, "Pilum"},
            {49, "Short Spear"},
            {50, "Glaive"},
            {51, "Throwing Spear"},
            {52, "Spear"},
            {53, "Trident"},
            {54, "Brandistock"},
            {55, "Spetum"},
            {56, "Pike"},
            {57, "Bardiche"},
            {58, "Voulge"},
            {59, "Scythe"},
            {60, "Poleaxe"},
            {61, "Halberd"},
            {62, "War Scythe"},
            {63, "Short Staff"},
            {64, "Long Staff"},
            {65, "Gnarled Staff"},
            {66, "Battle Staff"},
            {67, "War Staff"},
            {68, "Short Bow"},
            {69, "Hunter's Bow"},
            {70, "Long Bow"},
            {71, "Composite Bow"},
            {72, "Short Battle Bow"},
            {73, "Long Battle Bow"},
            {74, "Short War Bow"},
            {75, "Long War Bow"},
            {76, "Light Crossbow"},
            {77, "Crossbow"},
            {78, "Heavy Crossbow"},
            {79, "Repeating Crossbow"},
            {80, "Rancid Gas Potion"},
            {81, "Oil Potion"},
            {82, "Choking Gas Potion"},
            {83, "Exploding Potion"},
            {84, "Strangling Gas Potion"},
            {85, "Fulminating Potion"},
            {86, "Decoy Gidbinn"},
            {87, "The Gidbinn"},
            {88, "Wirt's Leg"},
            {89, "Horadric Malus"},
            {90, "Hellforge Hammer"},
            {91, "Horadric Staff"},
            {92, "Staff of Kings"},
            {93, "Hatchet"},
            {94, "Cleaver"},
            {95, "Twin Axe"},
            {96, "Crowbill"},
            {97, "Naga"},
            {98, "Military Axe"},
            {99, "Bearded Axe"},
            {100, "Tabar"},
            {101, "Gothic Axe"},
            {102, "Ancient Axe"},
            {103, "Burnt Wand"},
            {104, "Petrified Wand"},
            {105, "Tomb Wand"},
            {106, "Grave Wand"},
            {107, "Cudgel"},
            {108, "Rune Scepter"},
            {109, "Holy Water Sprinkler"},
            {110, "Divine Scepter"},
            {111, "Barbed Club"},
            {112, "Flanged Mace"},
            {113, "Jagged Star"},
            {114, "Knout"},
            {115, "Battle Hammer"},
            {116, "War Club"},
            {117, "Martel de Fer"},
            {118, "Gladius"},
            {119, "Cutlass"},
            {120, "Shamshir"},
            {121, "Tulwar"},
            {122, "Dimensional Blade"},
            {123, "Battle Sword"},
            {124, "Rune Sword"},
            {125, "Ancient Sword"},
            {126, "Espandon"},
            {127, "Dacian Falx"},
            {128, "Tusk Sword"},
            {129, "Gothic Sword"},
            {130, "Zweihander"},
            {131, "Executioner Sword"},
            {132, "Poignard"},
            {133, "Rondel"},
            {134, "Cinquedeas"},
            {135, "Stilleto"},
            {136, "Battle Dart"},
            {137, "Francisca"},
            {138, "War Dart"},
            {139, "Hurlbat"},
            {140, "War Javelin"},
            {141, "Great Pilum"},
            {142, "Simbilan"},
            {143, "Spiculum"},
            {144, "Harpoon"},
            {145, "War Spear"},
            {146, "Fuscina"},
            {147, "War Fork"},
            {148, "Yari"},
            {149, "Lance"},
            {150, "Lochaber Axe"},
            {151, "Bill"},
            {152, "Battle Scythe"},
            {153, "Partizan"},
            {154, "Bec-de-Corbin"},
            {155, "Grim Scythe"},
            {156, "Jo Staff"},
            {157, "Quarterstaff"},
            {158, "Cedar Staff"},
            {159, "Gothic Staff"},
            {160, "Rune Staff"},
            {161, "Edge Bow"},
            {162, "Razor Bow"},
            {163, "Cedar Bow"},
            {164, "Double Bow"},
            {165, "Short Siege Bow"},
            {166, "Long Siege Bow"},
            {167, "Rune Bow"},
            {168, "Gothic Bow"},
            {169, "Arbalest"},
            {170, "Siege Crossbow"},
            {171, "Ballista"},
            {172, "Chu-Ko-Nu"},
            {173, "Khalim's Flail"},
            {174, "Khalim's Will"},
            {175, "Katar"},
            {176, "Wrist Blade"},
            {177, "Hatchet Hands"},
            {178, "Cestus"},
            {179, "Claws"},
            {180, "Blade Talons"},
            {181, "Scissors Katar"},
            {182, "Quhab"},
            {183, "Wrist Spike"},
            {184, "Fascia"},
            {185, "Hand Scythe"},
            {186, "Greater Claws"},
            {187, "Greater Talons"},
            {188, "Scissors Quhab"},
            {189, "Suwayyah"},
            {190, "Wrist Sword"},
            {191, "War Fist"},
            {192, "Battle Cestus"},
            {193, "Feral Claws"},
            {194, "Runic Talons"},
            {195, "Scissors Suwayyah"},
            {196, "Tomahawk"},
            {197, "Small Crescent"},
            {198, "Ettin Axe"},
            {199, "War Spike"},
            {200, "Berserker Axe"},
            {201, "Feral Axe"},
            {202, "Silver-edged Axe"},
            {203, "Decapitator"},
            {204, "Champion Axe"},
            {205, "Glorious Axe"},
            {206, "Polished Wand"},
            {207, "Ghost Wand"},
            {208, "Lich Wand"},
            {209, "Unearthed Wand"},
            {210, "Truncheon"},
            {211, "Mighty Scepter"},
            {212, "Seraph Rod"},
            {213, "Caduceus"},
            {214, "Tyrant Club"},
            {215, "Reinforced Mace"},
            {216, "Devil Star"},
            {217, "Scourge"},
            {218, "Legendary Mallet"},
            {219, "Ogre Maul"},
            {220, "Thunder Maul"},
            {221, "Falcata"},
            {222, "Ataghan"},
            {223, "Elegant Blade"},
            {224, "Hydra Edge"},
            {225, "Phase Blade"},
            {226, "Conquest Sword"},
            {227, "Cryptic Sword"},
            {228, "Mythical Sword"},
            {229, "Legend Sword"},
            {230, "Highland Blade"},
            {231, "Balrog Blade"},
            {232, "Champion Sword"},
            {233, "Colossal Sword"},
            {234, "Colossus Blade"},
            {235, "Bone Knife"},
            {236, "Mithral Point"},
            {237, "Fanged Knife"},
            {238, "Legend Spike"},
            {239, "Flying Knife"},
            {240, "Flying Axe"},
            {241, "Winged Knife"},
            {242, "Winged Axe"},
            {243, "Hyperion Javelin"},
            {244, "Stygian Pilum"},
            {245, "Balrog Spear"},
            {246, "Ghost Glaive"},
            {247, "Winged Harpoon"},
            {248, "Hyperion Spear"},
            {249, "Stygian Pike"},
            {250, "Mancatcher"},
            {251, "Ghost Spear"},
            {252, "War Pike"},
            {253, "Ogre Axe"},
            {254, "Colossus Voulge"},
            {255, "Thresher"},
            {256, "Cryptic Axe"},
            {257, "Great Poleaxe"},
            {258, "Giant Thresher"},
            {259, "Walking Stick"},
            {260, "Stalagmite"},
            {261, "Elder Staff"},
            {262, "Shillelagh"},
            {263, "Archon Staff"},
            {264, "Spider Bow"},
            {265, "Blade Bow"},
            {266, "Shadow Bow"},
            {267, "Great Bow"},
            {268, "Diamond Bow"},
            {269, "Crusader Bow"},
            {270, "Ward Bow"},
            {271, "Hydra Bow"},
            {272, "Pellet Bow"},
            {273, "Gorgon Crossbow"},
            {274, "Colossus Crossbow"},
            {275, "Demon Crossbow"},
            {276, "Eagle Orb"},
            {277, "Sacred Globe"},
            {278, "Smoked Sphere"},
            {279, "Clasped Orb"},
            {280, "Jared's Stone"},
            {281, "Stag Bow"},
            {282, "Reflex Bow"},
            {283, "Maiden Spear"},
            {284, "Maiden Pike"},
            {285, "Maiden Javelin"},
            {286, "Glowing Orb"},
            {287, "Crystalline Globe"},
            {288, "Cloudy Sphere"},
            {289, "Sparkling Ball"},
            {290, "Swirling Crystal"},
            {291, "Ashwood Bow"},
            {292, "Ceremonial Bow"},
            {293, "Ceremonial Spear"},
            {294, "Ceremonial Pike"},
            {295, "Ceremonial Javelin"},
            {296, "Heavenly Stone"},
            {297, "Eldritch Orb"},
            {298, "Demon Heart"},
            {299, "Vortex Orb"},
            {300, "Dimensional Shard"},
            {301, "Matriarchal Bow"},
            {302, "Grand Matron Bow"},
            {303, "Matriarchal Spear"},
            {304, "Matriarchal Pike"},
            {305, "Matriarchal Javelin"},
            {306, "Cap"},
            {307, "Skull Cap"},
            {308, "Helm"},
            {309, "Full Helm"},
            {310, "Great Helm"},
            {311, "Crown"},
            {312, "Mask"},
            {313, "Quilted Armor"},
            {314, "Leather Armor"},
            {315, "Hard Leather Armor"},
            {316, "Studded Leather"},
            {317, "Ring Mail"},
            {318, "Scale Mail"},
            {319, "Chain Mail"},
            {320, "Breast Plate"},
            {321, "Splint Mail"},
            {322, "Plate Mail"},
            {323, "Field Plate"},
            {324, "Gothic Plate"},
            {325, "Full Plate Mail"},
            {326, "Ancient Armor"},
            {327, "Light Plate"},
            {328, "Buckler"},
            {329, "Small Shield"},
            {330, "Large Shield"},
            {331, "Kite Shield"},
            {332, "Tower Shield"},
            {333, "Gothic Shield"},
            {334, "Leather Gloves"},
            {335, "Heavy Gloves"},
            {336, "Chain Gloves"},
            {337, "Light Gauntlets"},
            {338, "Gauntlets"},
            {339, "Boots"},
            {340, "Heavy Boots"},
            {341, "Chain Boots"},
            {342, "Light Plated Boots"},
            {343, "Greaves"},
            {344, "Sash"},
            {345, "Light Belt"},
            {346, "Belt"},
            {347, "Heavy Belt"},
            {348, "Plated Belt"},
            {349, "Bone Helm"},
            {350, "Bone Shield"},
            {351, "Spiked Shield"},
            {352, "War Hat"},
            {353, "Sallet"},
            {354, "Casque"},
            {355, "Basinet"},
            {356, "Winged Helm"},
            {357, "Grand Crown"},
            {358, "Death Mask"},
            {359, "Ghost Armor"},
            {360, "Serpentskin Armor"},
            {361, "Demonhide Armor"},
            {362, "Trellised Armor"},
            {363, "Linked Mail"},
            {364, "Tigulated Mail"},
            {365, "Mesh Armor"},
            {366, "Cuirass"},
            {367, "Russet Armor"},
            {368, "Templar Coat"},
            {369, "Sharktooth Armor"},
            {370, "Embossed Plate"},
            {371, "Chaos Armor"},
            {372, "Ornate Armor"},
            {373, "Mage Plate"},
            {374, "Defender"},
            {375, "Round Shield"},
            {376, "Scutum"},
            {377, "Dragon Shield"},
            {378, "Pavise"},
            {379, "Ancient Shield"},
            {380, "Demonhide Gloves"},
            {381, "Sharkskin Gloves"},
            {382, "Heavy Bracers"},
            {383, "Battle Gauntlets"},
            {384, "War Gauntlets"},
            {385, "Demonhide Boots"},
            {386, "Sharkskin Boots"},
            {387, "Mesh Boots"},
            {388, "Battle Boots"},
            {389, "War Boots"},
            {390, "Demonhide Sash"},
            {391, "Sharkskin Belt"},
            {392, "Mesh Belt"},
            {393, "Battle Belt"},
            {394, "War Belt"},
            {395, "Grim Helm"},
            {396, "Grim Shield"},
            {397, "Barbed Shield"},
            {398, "Wolf Head"},
            {399, "Hawk Helm"},
            {400, "Antlers"},
            {401, "Falcon Mask"},
            {402, "Spirit Mask"},
            {403, "Jawbone Cap"},
            {404, "Fanged Helm"},
            {405, "Horned Helm"},
            {406, "Assault Helmet"},
            {407, "Avenger Guard"},
            {408, "Targe"},
            {409, "Rondache"},
            {410, "Heraldic Shield"},
            {411, "Aerin Shield"},
            {412, "Crown Shield"},
            {413, "Preserved Head"},
            {414, "Zombie Head"},
            {415, "Unraveller Head"},
            {416, "Gargoyle Head"},
            {417, "Demon Head"},
            {418, "Circlet"},
            {419, "Coronet"},
            {420, "Tiara"},
            {421, "Diadem"},
            {422, "Shako"},
            {423, "Hydraskull"},
            {424, "Armet"},
            {425, "Giant Conch"},
            {426, "Spired Helm"},
            {427, "Corona"},
            {428, "Demonhead"},
            {429, "Dusk Shroud"},
            {430, "Wyrmhide"},
            {431, "Scarab Husk"},
            {432, "Wire Fleece"},
            {433, "Diamond Mail"},
            {434, "Loricated Mail"},
            {435, "Boneweave"},
            {436, "Great Hauberk"},
            {437, "Balrog Skin"},
            {438, "Hellforge Plate"},
            {439, "Kraken Shell"},
            {440, "Lacquered Plate"},
            {441, "Shadow Plate"},
            {442, "Sacred Armor"},
            {443, "Archon Plate"},
            {444, "Heater"},
            {445, "Luna"},
            {446, "Hyperion"},
            {447, "Monarch"},
            {448, "Aegis"},
            {449, "Ward"},
            {450, "Bramble Mitts"},
            {451, "Vampirebone Gloves"},
            {452, "Vambraces"},
            {453, "Crusader Gauntlets"},
            {454, "Ogre Gauntlets"},
            {455, "Wyrmhide Boots"},
            {456, "Scarabshell Boots"},
            {457, "Boneweave Boots"},
            {458, "Mirrored Boots"},
            {459, "Myrmidon Greaves"},
            {460, "Spiderweb Sash"},
            {461, "Vampirefang Belt"},
            {462, "Mithril Coil"},
            {463, "Troll Belt"},
            {464, "Colossus Girdle"},
            {465, "Bone Visage"},
            {466, "Troll Nest"},
            {467, "Blade Barrier"},
            {468, "Alpha Helm"},
            {469, "Griffon Headress"},
            {470, "Hunter's Guise"},
            {471, "Sacred Feathers"},
            {472, "Totemic Mask"},
            {473, "Jawbone Visor"},
            {474, "Lion Helm"},
            {475, "Rage Mask"},
            {476, "Savage Helmet"},
            {477, "Slayer Guard"},
            {478, "Akaran Targe"},
            {479, "Akaran Rondache"},
            {480, "Protector Shield"},
            {481, "Gilded Shield"},
            {482, "Royal Shield"},
            {483, "Mummified Trophy"},
            {484, "Fetish Trophy"},
            {485, "Sexton Trophy"},
            {486, "Cantor Trophy"},
            {487, "Heirophant Trophy"},
            {488, "Blood Spirit"},
            {489, "Sun Spirit"},
            {490, "Earth Spirit"},
            {491, "Sky Spirit"},
            {492, "Dream Spirit"},
            {493, "Carnage Helm"},
            {494, "Fury Visor"},
            {495, "Destroyer Helm"},
            {496, "Conqueror Crown"},
            {497, "Guardian Crown"},
            {498, "Sacred Targe"},
            {499, "Sacred Rondache"},
            {500, "Ancient Shield"},
            {501, "Zakarum Shield"},
            {502, "Vortex Shield"},
            {503, "Minion Skull"},
            {504, "Hellspawn Skull"},
            {505, "Overseer Skull"},
            {506, "Succubus Skull"},
            {507, "Bloodlord Skull"},
            {508, "Elixir"},
            {509, "Healing Potion"},
            {510, "Mana Potion"},
            {511, "Full Healing Potion"},
            {512, "Full Mana Potion"},
            {513, "Stamina Potion"},
            {514, "Antidote Potion"},
            {515, "Rejuvenation Potion"},
            {516, "Full Rejuvenation Potion"},
            {517, "Thawing Potion"},
            {518, "Tome of Town Portal"},
            {519, "Tome of Identify"},
            {520, "Amulet"},
            {521, "Amulet of the Viper"},
            {522, "Ring"},
            {523, "Gold"},
            {524, "Scroll of Inifuss"},
            {525, "Key to the Cairn Stones"},
            {526, "Arrows"},
            {527, "Torch"},
            {528, "Bolts"},
            {529, "Scroll of Town Portal"},
            {530, "Scroll of Identify"},
            {531, "Heart"},
            {532, "Brain"},
            {533, "Jawbone"},
            {534, "Eye"},
            {535, "Horn"},
            {536, "Tail"},
            {537, "Flag"},
            {538, "Fang"},
            {539, "Quill"},
            {540, "Soul"},
            {541, "Scalp"},
            {542, "Spleen"},
            {543, "Key"},
            {544, "The Black Tower Key"},
            {545, "Potion of Life"},
            {546, "A Jade Figurine"},
            {547, "The Golden Bird"},
            {548, "Lam Esen's Tome"},
            {549, "Horadric Cube"},
            {550, "Horadric Scroll"},
            {551, "Mephisto's Soulstone"},
            {552, "Book of Skill"},
            {553, "Khalim's Eye"},
            {554, "Khalim's Heart"},
            {555, "Khalim's Brain"},
            {556, "Ear"},
            {557, "Chipped Amethyst"},
            {558, "Flawed Amethyst"},
            {559, "Amethyst"},
            {560, "Flawless Amethyst"},
            {561, "Perfect Amethyst"},
            {562, "Chipped Topaz"},
            {563, "Flawed Topaz"},
            {564, "Topaz"},
            {565, "Flawless Topaz"},
            {566, "Perfect Topaz"},
            {567, "Chipped Sapphire"},
            {568, "Flawed Sapphire"},
            {569, "Sapphire"},
            {570, "Flawless Sapphire"},
            {571, "Perfect Sapphire"},
            {572, "Chipped Emerald"},
            {573, "Flawed Emerald"},
            {574, "Emerald"},
            {575, "Flawless Emerald"},
            {576, "Perfect Emerald"},
            {577, "Chipped Ruby"},
            {578, "Flawed Ruby"},
            {579, "Ruby"},
            {580, "Flawless Ruby"},
            {581, "Perfect Ruby"},
            {582, "Chipped Diamond"},
            {583, "Flawed Diamond"},
            {584, "Diamond"},
            {585, "Flawless Diamond"},
            {586, "Perfect Diamond"},
            {587, "Minor Healing Potion"},
            {588, "Light Healing Potion"},
            {589, "Healing Potion"},
            {590, "Greater Healing Potion"},
            {591, "Super Healing Potion"},
            {592, "Minor Mana Potion"},
            {593, "Light Mana Potion"},
            {594, "Mana Potion"},
            {595, "Greater Mana Potion"},
            {596, "Super Mana Potion"},
            {597, "Chipped Skull"},
            {598, "Flawed Skull"},
            {599, "Skull"},
            {600, "Flawless Skull"},
            {601, "Perfect Skull"},
            {602, "Herb"},
            {603, "Small Charm"},
            {604, "Large Charm"},
            {605, "Grand Charm"},
            {606, "Small Red Potion"},
            {607, "Large Red Potion"},
            {608, "Small Blue Potion"},
            {609, "Large Blue Potion"},
            {610, "El Rune"},
            {611, "Eld Rune"},
            {612, "Tir Rune"},
            {613, "Nef Rune"},
            {614, "Eth Rune"},
            {615, "Ith Rune"},
            {616, "Tal Rune"},
            {617, "Ral Rune"},
            {618, "Ort Rune"},
            {619, "Thul Rune"},
            {620, "Amn Rune"},
            {621, "Sol Rune"},
            {622, "Shael Rune"},
            {623, "Dol Rune"},
            {624, "Hel Rune"},
            {625, "Io Rune"},
            {626, "Lum Rune"},
            {627, "Ko Rune"},
            {628, "Fal Rune"},
            {629, "Lem Rune"},
            {630, "Pul Rune"},
            {631, "Um Rune"},
            {632, "Mal Rune"},
            {633, "Ist Rune"},
            {634, "Gul Rune"},
            {635, "Vex Rune"},
            {636, "Ohm Rune"},
            {637, "Lo Rune"},
            {638, "Sur Rune"},
            {639, "Ber Rune"},
            {640, "Jah Rune"},
            {641, "Cham Rune"},
            {642, "Zod Rune"},
            {643, "Jewel"},
            {644, "Malah's Potion"},
            {645, "Scroll of Knowledge"},
            {646, "Scroll of Resistance"},
            {647, "Key of Terror"},
            {648, "Key of Hate"},
            {649, "Key of Destruction"},
            {650, "Diablo's Horn"},
            {651, "Baal's Eye"},
            {652, "Mephisto's Brain"},
            {653, "Token of Absolution"},
            {654, "Twisted Essence of Suffering"},
            {655, "Charged Essense of Hatred"},
            {656, "Burning Essence of Terror"},
            {657, "Festering Essence of Destruction"},
            {658, "Standard of Heroes"}
        };
    }
}
