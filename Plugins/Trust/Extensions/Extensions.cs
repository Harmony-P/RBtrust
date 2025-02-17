﻿using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.AClasses;
using ff14bot.Interfaces;
using ff14bot.Enums;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Trust.Helpers;

namespace Trust.Extensions
{
    /// <summary>
    /// Various extension methods.
    /// </summary>
    internal static class Extensions
    {
        /* public enum TrustNPC : uint
        {
          
            // Yshtola = 729,
            Urianger = 1492,
            Alphinaud = 4130, 
            Alisaie = 5239,
            Yshtola = 8378,
            Ryne = 8889,
            Minfilia = 8917,
            Lyna = 8919,
            AlphinaudAvatar = 11264, 
            AlisaieAvatar = 11265, 
            UriangerAvatar = 11267,
            YshtolaAvatar = 11268, 
            RyneAvatar = 11269, 
            EstinienAvatar = 11270,
            Venat = 10586, 
            VenatPhantom = 10587,
            EmetSelch = 10898, 
            Hythlodaeus = 10899,
            Thancred = 713,
            CrystalExarch = 8650,
            ThancredsAvatar = 11266,
            GrahaTiasAvatar = 11271,
            GrahaTia = 9363,
        }; */

        private static HashSet<ClassJobType> Tanks = new HashSet<ClassJobType>()
        {
            ClassJobType.Gladiator,
            ClassJobType.Marauder,
            ClassJobType.Paladin,
            ClassJobType.Gunbreaker,
            ClassJobType.Warrior,
            ClassJobType.DarkKnight,
        };

        private static HashSet<ClassJobType> DPS = new HashSet<ClassJobType>()
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

        private static HashSet<ClassJobType> Healers = new HashSet<ClassJobType>()
        {

           ClassJobType.Sage,
           ClassJobType.Astrologian,
           ClassJobType.WhiteMage,
           ClassJobType.Scholar,
           ClassJobType.Conjurer,

        };

        public static bool IsTank(this BattleCharacter bc)
        {
            return Tanks.Contains(bc.CurrentJob) ? true : false;
        }

        public static bool IsDPS(this BattleCharacter bc)
        {
            return DPS.Contains(bc.CurrentJob) ? true : false;
        }

        public static bool IsHealer(this BattleCharacter bc)
        {
            return Healers.Contains(bc.CurrentJob) ? true : false;
        }
            


        /// <summary>
        /// Checks if any nearby <see cref="BattleCharacter"/> is casting any spell ID in this collection.
        /// </summary>
        /// <param name="spellCastIds">Spell IDs to check against.</param>
        /// <returns><see langword="true"/> if any given spell is being casted.</returns>
        public static bool IsCasting(this HashSet<uint> spellCastIds)
        {
            return GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                    .Any(obj => spellCastIds.Contains(obj.CastingSpellId) && obj.Distance() < 50);
        }

         public static bool IsCastingtwo(this HashSet<uint> spellCastIds)
        {
            var actids = GameObjectManager.GetObjectsOfType<BattleCharacter>()
               ?.Where(obj => obj.IsCasting && !(bool)PartyManager.AllMembers?.Any(p => p.ObjectId == obj.ObjectId));

            if ((bool)actids?.Any())
            {
                foreach (var actid in actids)
                {
                    Logging.Write(Colors.Yellow, $@" IsCastingtwo 判断显示在使用的 actid ： {actid.CastingSpellId} {actid.SpellCastInfo.IsCasting} {actid.SpellCastInfo.SpellData.LocalizedName} {spellCastIds.Contains(actid.CastingSpellId)}");

                    return spellCastIds.Contains(actid.CastingSpellId) && actid.SpellCastInfo.IsCasting;
                }
            }

            return false;
        }

        public static bool IsFw(this HashSet<uint> bossid)
        {
            return (bool)GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)?
                    .Any(obj => bossid.Contains(obj.NpcId) && obj.CurrentHealthPercent < 1 && obj.Distance() < 50);
        }



        private static Stopwatch MoveStopSw = new Stopwatch();
        private static Vector3 MoveStopDc = new Vector3();

        /// <summary>
        /// Follows the specified <see cref="BattleCharacter"/>.
        /// </summary>
        /// <param name="bc">Character to follow.</param>
        /// <param name="followDistance">Distance to follow at.</param>
        /// <param name="msWait">Time between movement ticks, in milliseconds.</param>
        /// <param name="useMesh">Whether to use Nav Mesh or move blindly.</param>
        /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
        public static async Task<bool> Follow(this BattleCharacter bc, float followDistance = 0.3f, int msWait = 0, bool useMesh = false)
        {
            

           if (bc == null)
            {
                return true;
            }

            float curDistance = Core.Me.Distance2D(bc);

            if (curDistance < followDistance)
            {
                return true;
            }

            MoveStopSw.Reset();

            while (!Core.Me.IsDead && Core.Me.InCombat)
            {
                curDistance = Core.Me.Distance2D(bc);
                if (curDistance < followDistance)
                {
#if RB_CN
                Logging.Write(Colors.Aquamarine, $"跟随 队友 {bc.Name} [距离: {Core.Me.Distance2D(bc.Location)}]");
#else
                    Logging.Write(Colors.Aquamarine, $"Following {bc.Name} [Distance2D: {curDistance}]");
#endif
                    break;
                }
                if (Core.Me.IsDead)
                {
                    return false;
                }
                //if (Core.Me.IsCasting)
                //{
                //    ActionManager.StopCasting();
                //}
                if (useMesh)
                {
                    await CommonTasks.MoveTo(bc.Location);
                }
                else
                {
                    Navigator.PlayerMover.MoveTowards(bc.Location);
                    await Coroutine.Yield();
                }

                curDistance = Core.Me.Distance2D(bc);

                if (curDistance < followDistance + 0.5f)
                {
                    MoveStopDc = bc.Location;

                    if (await Coroutine.Wait(100, () => bc.Distance2D() < followDistance || bc.Distance2D(MoveStopDc) > 0))
                    {
                        if (bc.Distance2D(MoveStopDc) > 0)
                        {
                            continue;
                        }
                        Navigator.PlayerMover.MoveStop();
                    }
                }
                await Coroutine.Sleep(msWait);
            }

            //return await StopMoving();
            return true;
        }

        public static async Task<bool> Follow2(this BattleCharacter bc, Stopwatch sw, double TimeToFollow = 3000, float followDistance = 0.3f, int msWait = 0, bool useMesh = false)
        {
            float curDistance = Core.Me.Location.Distance2D(bc.Location);
            if (bc == null)
            {
                return true;
            }
            if (!sw.IsRunning)
            {
                sw.Restart();
            }

            if (!Core.Me.IsDead && Core.Me.InCombat && (sw.ElapsedMilliseconds <= TimeToFollow))
            {

                if (curDistance < followDistance)
                {
                    Navigator.Stop();
                }

                else if (useMesh)
                {
                    await CommonTasks.MoveTo(bc.Location);
                }
                else
                {
                    Navigator.PlayerMover.MoveTowards(bc.Location);
                    await Coroutine.Sleep(msWait);
                }
                curDistance = Core.Me.Distance2D(bc);

                if (curDistance < 1f)
                {
                    MoveStopDc = bc.Location;

                    if (await Coroutine.Wait(100, () => bc.Distance2D() < followDistance || bc.Distance2D(MoveStopDc) > 0))
                    {
                        if (bc.Distance2D(MoveStopDc) > 0)
                        {
                            return false;
                        }
                        Navigator.PlayerMover.MoveStop();
                    }
                }
               
#if RB_CN
                Logging.Write(Colors.Aquamarine, $"跟随 队友 {bc.Name} [距离: {Core.Me.Distance(bc.Location)}]");
#else
                Logging.Write(Colors.Aquamarine, $"Following {bc.Name} [Distance: {curDistance}]");
#endif
                
            }

            return false;
        }
        /// <summary>
        /// Stops the player's movement.
        /// </summary>
        /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
        public static async Task<bool> StopMoving()
        {
            if (!MovementManager.IsMoving)
            {
                return true;
            }

            int ticks = 0;
            while (MovementManager.IsMoving && ticks < 100)
            {
                Navigator.Stop();
                await Coroutine.Sleep(100);
                ticks++;
            }

            return true;
        }

        /// <summary>
        /// Disables SideStep around certain boss-related monsters.
        /// </summary>
        /// <param name="bossIds">Boss monster IDs.</param>
        /// <param name="ignoreIds">IDs to filter out of the base list.</param>
        public static void ToggleSideStep(this HashSet<uint> bossIds, uint[] ignoreIds = null)
        {
            if (Core.Target == null)
            {
                return;
            }

            PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

            if (sidestepPlugin != null)
            {
                HashSet<uint> filteredIds = new HashSet<uint>(bossIds.Where(id => ignoreIds == null || !ignoreIds.Contains(id)));

                bool isBoss = ignoreIds != null
                ? GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                                   .Any(obj => obj.Distance() < 50 && filteredIds.Contains(obj.NpcId))
                : GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                                   .Any(obj => obj.Distance() < 50 && bossIds.Contains(obj.NpcId));

                sidestepPlugin.Enabled = !isBoss;
            }
        }
    }
}
