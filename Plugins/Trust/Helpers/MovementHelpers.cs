﻿using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Trust.Helpers;


namespace Trust.Helpers
{
    /// <summary>
    /// Miscellaneous functions related to movement.
    /// </summary>
    internal static class MovementHelpers
    {
        public static readonly HashSet<uint> PartyMemberIds = new HashSet<uint>()
        {
            729, // Y'shtola       :: 雅·修特拉
            1492, // Urianger       :: 于里昂热
            4130, // Alphinaud      :: 阿尔菲诺
            5239, // Alisaie        :: 阿莉塞
            8378, // Y'shtola       :: 雅·修特拉
            8889, // Ryne           :: 琳
            8917, // Minfilia       :: 敏菲利亚
            8919, // Lyna           :: 莱楠
            11264, // Alphinaud's avatar
            11265, // Alisaie's avatar
            11267, // Urianger's avatar
            11268, // Y'shtola's avatar
            11269, // Ryne's avatar
            11270, // Estinien's avatar
            10586, // Venat
            10587, // Venat's Phantom
            10898, // Emet-Selch
            10899, // Hythlodaeus

            // 713,  // Thancred       :: 桑克瑞德
            // 8650, // Crystal Exarch :: 水晶公
            // 11266, // Thancred's avatar
            // 11271, // G'raha Tia's avatar
        };

        public static readonly HashSet<uint> AllPartyDpsIds = new HashSet<uint>()
        {
            729, // Y'shtola       :: 雅·修特拉
            //1492, // Urianger       :: 于里昂热
            //4130, // Alphinaud      :: 阿尔菲诺
            5239, // Alisaie        :: 阿莉塞
            8378, // Y'shtola       :: 雅·修特拉
            8889, // Ryne           :: 琳
            8917, // Minfilia       :: 敏菲利亚
            8919, // Lyna           :: 莱楠
            //11264, // Alphinaud's avatar
            11265, // Alisaie's avatar
            //11267, // Urianger's avatar
            11268, // Y'shtola's avatar
            11269, // Ryne's avatar
            11270, // Estinien's avatar
            //10586, // Venat
            //10587, // Venat's Phantom
            //10898, // Emet-Selch
            //10899, // Hythlodaeus
            //9363,  G'raha Tia
            //713,  // Thancred       :: 桑克瑞德
            //8650, // Crystal Exarch :: 水晶公
            //11266, // Thancred's avatar
            //11271, // G'raha Tia's avatar
        };

        public static readonly HashSet<uint> AllPartyTankIds = new HashSet<uint>()
        {
            713, // Thancred       :: 桑克瑞德
            8650, // Crystal Exarch :: 水晶公
            11266, // Thancred's avatar
            11271, // G'raha Tia's avatar
            9363, // G'raha Tia
        };

        public static readonly HashSet<uint> AllPartyMemberIds = new HashSet<uint>()
        {
            729, // Y'shtola       :: 雅·修特拉
            1492, // Urianger       :: 于里昂热
            4130, // Alphinaud      :: 阿尔菲诺
            5239, // Alisaie        :: 阿莉塞
            8378, // Y'shtola       :: 雅·修特拉
            8889, // Ryne           :: 琳
            8917, // Minfilia       :: 敏菲利亚
            8919, // Lyna           :: 莱楠
            11264, // Alphinaud's avatar
            11265, // Alisaie's avatar
            11267, // Urianger's avatar
            11268, // Y'shtola's avatar
            11269, // Ryne's avatar
            11270, // Estinien's avatar
            10586, // Venat
            10587, // Venat's Phantom
            10898, // Emet-Selch
            10899, // Hythlodaeus
            9363, // G'raha Tia
            713, // Thancred       :: 桑克瑞德
            8650, // Crystal Exarch :: 水晶公
            11266, // Thancred's avatar
            11271, // G'raha Tia's avatar
        };

        private static readonly HashSet<ClassJobType> Tanks = new HashSet<ClassJobType>()
        {
            ClassJobType.Gladiator,
            ClassJobType.Marauder,
            ClassJobType.Paladin,
            ClassJobType.Gunbreaker,
            ClassJobType.Warrior,
            ClassJobType.DarkKnight,
        };

        private static readonly HashSet<ClassJobType> DPS = new HashSet<ClassJobType>()
        {
            ClassJobType.Lancer,
            ClassJobType.Archer,
            ClassJobType.Thaumaturge,
            ClassJobType.Pugilist,
            ClassJobType.Monk,
            ClassJobType.Dragoon,
            ClassJobType.Bard,
            ClassJobType.BlackMage,
            ClassJobType.Arcanist,
            ClassJobType.Summoner,
            ClassJobType.Rogue,
            ClassJobType.Ninja,
            ClassJobType.Machinist,
            ClassJobType.Samurai,
            ClassJobType.RedMage,
            ClassJobType.Dancer,
            ClassJobType.Reaper,
        };

        private static readonly HashSet<ClassJobType> Healers = new HashSet<ClassJobType>()
        {
            ClassJobType.Sage, ClassJobType.Astrologian, ClassJobType.WhiteMage, ClassJobType.Scholar,
        };

        private static readonly List<ClassJobType> Melee = new List<ClassJobType>
        {
            ClassJobType.Lancer,
            ClassJobType.Dragoon,
            ClassJobType.Pugilist,
            ClassJobType.Monk,
            ClassJobType.Rogue,
            ClassJobType.Ninja,
            ClassJobType.Samurai,
            ClassJobType.Reaper,
            ClassJobType.DarkKnight,
            ClassJobType.Gladiator,
            ClassJobType.Marauder,
            ClassJobType.Paladin,
            ClassJobType.Warrior,
            ClassJobType.Gunbreaker
        };

        /// <summary>
        /// Gets the nearest Ally to the Player.
        /// </summary>
        public static BattleCharacter GetClosestAlly => GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
            .Where(obj => !obj.IsDead && PartyMemberIds.Contains(obj.NpcId))
            .OrderBy(r => r.Distance())
            .FirstOrDefault();

        public static BattleCharacter GetClosestDps => GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
            .Where(obj => !obj.IsDead && AllPartyMemberIds.Contains(obj.NpcId) && DPS.Contains(obj.CurrentJob))
            .OrderBy(r => r.Distance())
            .FirstOrDefault();

        public static BattleCharacter GetClosestTank => GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
            .Where(obj => !obj.IsDead && AllPartyMemberIds.Contains(obj.NpcId) && Tanks.Contains(obj.CurrentJob))
            .OrderBy(r => r.Distance())
            .FirstOrDefault();

        public static BattleCharacter GetClosestMelee => GameObjectManager
            .GetObjectsOfType<BattleCharacter>(true, false)
            .Where(obj => !obj.IsDead && AllPartyMemberIds.Contains(obj.NpcId) && Melee.Contains(obj.CurrentJob))
            .OrderBy(r => r.Distance())
            .FirstOrDefault();

        public static Vector3 Vec = new Vector3("0,0,1");

        public static BattleCharacter GetClosestLocal(Vector3 vector)
        {
            if (vector == null)
            {
                return null;
            }

            return GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                .Where(obj => !obj.IsDead && PartyMemberIds.Contains(obj.NpcId))
                .OrderBy(r => r.Distance(vector))
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the furthest Ally from the Player.
        /// </summary>
        public static BattleCharacter GetFurthestAlly => GameObjectManager
            .GetObjectsOfType<BattleCharacter>(true, false)
            .Where(obj => !obj.IsDead && PartyMemberIds.Contains(obj.NpcId))
            .OrderByDescending(r => r.Distance())
            .FirstOrDefault();


        public static bool IsMelee(this GameObject tar)
        {
            var gameObject = tar as Character;

            return gameObject != null && (bool)Melee?.Contains(gameObject.CurrentJob);
        }

        private static Vector3 PlayerLoc => Core.Player.Location;

        public static async Task<bool> Spread(double TimeToSpread, float spreadDistance = 6.5f,
            bool IsSpreading = false, uint spbc = 0)
        {
            if (IsSpreading)
            {
                return true;
            }

            double CurrentMS = DateTime.Now.TimeOfDay.TotalMilliseconds;
            double EndMS = CurrentMS + (TimeToSpread);

            if (spbc != 0)
            {
                AllPartyMemberIds.Add(spbc);
            }

            //if (sidestepPlugin != null)
            //    {
            //        sidestepPlugin.Enabled = true;
            //    }

            if (!AvoidanceManager.IsRunningOutOfAvoid)
            {
                foreach (var npc in PartyManager.AllMembers.Select(p => p.BattleCharacter)
                             .OrderByDescending(obj => Core.Player.Distance(obj)))
                {
                    AvoidanceManager.AddAvoidObject<BattleCharacter>(
                        () => DateTime.Now.TimeOfDay.TotalMilliseconds <= EndMS,
                        radius: spreadDistance,
                        npc.ObjectId);
                }

                await Coroutine.Wait(300, () => AvoidanceManager.IsRunningOutOfAvoid);
            }


            if (!AvoidanceManager.IsRunningOutOfAvoid)
            {
                MovementManager.MoveStop();
            }


            return true;
        }


        public static async Task<bool> HalfSpread(double TimeToSpread, float spreadDistance = 6.5f,
            bool IsSpreading = false, uint spbc = 0)
        {
            if (IsSpreading)
            {
                return true;
            }

            double CurrentMS = DateTime.Now.TimeOfDay.TotalMilliseconds;
            double EndMS = CurrentMS + (TimeToSpread);

            if (spbc != 0)
            {
                var nobj = PartyManager.AllMembers.Select(pm => pm.BattleCharacter)
                    .OrderBy(obj => obj.Distance(Core.Player)).FirstOrDefault(obj => !obj.IsMe);

                var st = Core.Player.CurrentTarget;

                var fs = Core.Player;

                Vector3 tl = new Vector3();
                if (st != null && fs != null && st.Distance2D(fs) > 0)
                {
                    var k = (st.Z - fs.Z) / (st.X - fs.X);
                    var b = st.Z - k * st.X;

                    var plg = 100f / fs.DistanceSqr(st.Location);

                    //var plg = 2f / Math.Sqrt((fs.X - st.X) * (fs.X - st.X) +
                    //(fs.Z - st.Z) * (fs.Z - st.Z));


                    tl.X = fs.X - plg * (st.X - fs.X);
                    tl.Z = k * tl.X + b;
                    tl.Y = st.Y;
                    //Log(plg);
                    //ActionManager.DoActionLocation(188, tl);

                    //Log(tl);

                    if (nobj.Distance(tl) - 2f < Core.Player.Distance(tl))
                    {
                        Navigator.PlayerMover.MoveTowards(tl);
                        await Coroutine.Yield();
                        return false;
                    }
                }
            }

            //if (sidestepPlugin != null)
            //    {
            //        sidestepPlugin.Enabled = true;
            //    }

            foreach (var npc in GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                         .Where(obj => AllPartyMemberIds.Contains(obj.NpcId))
                         .OrderByDescending(obj => Core.Player.Distance(obj)))
            {
                AvoidanceManager.AddAvoidObject<BattleCharacter>(
                    () => DateTime.Now.TimeOfDay.TotalMilliseconds <= EndMS,
                    radius: spreadDistance,
                    npc.ObjectId);
                await Coroutine.Yield();
            }

            if (!AvoidanceManager.IsRunningOutOfAvoid)
            {
                MovementManager.MoveStop();
            }


            return true;
        }

        public static async Task<bool> SpreadSp(double TimeToSpread, Vector3 vector, float spreadDistance = 6.5f,
            bool IsSpreading = false, uint spbc = 0)
        {
            if (IsSpreading)
            {
                return true;
            }

            double CurrentMS = DateTime.Now.TimeOfDay.TotalMilliseconds;
            double EndMS = CurrentMS + (TimeToSpread);

            if (spbc != 0)
            {
                AllPartyMemberIds.Add(spbc);
            }

            //if (sidestepPlugin != null)
            //    {
            //        sidestepPlugin.Enabled = true;
            //    }

            var nobj = GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                .Where(obj => AllPartyMemberIds.Contains(obj.NpcId)).OrderBy(obj => obj.Distance(Core.Player))
                .FirstOrDefault();

            float ls = 0;
            if (vector != null)
            {
                if (PlayerLoc.X > vector.X)
                {
                    ls = -20;
                }
                else
                {
                    ls = 20;
                }
            }


            foreach (var npc in GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                         .Where(obj => AllPartyMemberIds.Contains(obj.NpcId))
                         .OrderByDescending(r => Core.Player.Distance()))
            {
                AvoidanceManager.AddAvoidObject<BattleCharacter>(
                    () => DateTime.Now.TimeOfDay.TotalMilliseconds <= EndMS,
                    () => new Vector3(PlayerLoc.X - ls, PlayerLoc.Y, PlayerLoc.Z),
                    leashRadius: 40,
                    radius: spreadDistance,
                    npc.ObjectId);

                await Coroutine.Yield();
            }

            if (!AvoidanceManager.IsRunningOutOfAvoid)
            {
                MovementManager.MoveStop();
            }

            return true;
        }

        public static async Task<bool> SpreadSpLoc(double TimeToSpread, Vector3 vector, float spreadDistance = 6.5f,
            bool IsSpreading = false, uint spbc = 0)
        {
            if (IsSpreading)
            {
                return true;
            }

            double CurrentMS = DateTime.Now.TimeOfDay.TotalMilliseconds;
            double EndMS = CurrentMS + (TimeToSpread);

            if (spbc != 0)
            {
                AllPartyMemberIds.Add(spbc);
            }

            //if (sidestepPlugin != null)
            //    {
            //        sidestepPlugin.Enabled = true;
            //    }


            if (vector == null)
            {
                vector = GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                    .Where(obj => AllPartyMemberIds.Contains(obj.NpcId)).OrderBy(obj => obj.Distance(Core.Player))
                    .FirstOrDefault().Location;
            }


            foreach (var npc in GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                         .Where(obj => AllPartyMemberIds.Contains(obj.NpcId))
                         .OrderByDescending(r => Core.Player.Distance()))
            {
                AvoidanceManager.AddAvoidObject<BattleCharacter>(
                    () => DateTime.Now.TimeOfDay.TotalMilliseconds <= EndMS,
                    () => vector,
                    leashRadius: 40,
                    radius: spreadDistance,
                    npc.ObjectId);

                await Coroutine.Yield();
            }

            if (!AvoidanceManager.IsRunningOutOfAvoid)
            {
                MovementManager.MoveStop();
            }

            return true;
        }
    }
}
