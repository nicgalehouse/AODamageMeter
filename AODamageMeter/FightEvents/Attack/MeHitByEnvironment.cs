﻿using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    public class MeHitByEnvironment : AttackEvent
    {
        public const string EventKey = "01";
        public const string EventName = "Me hit by environment";

        public static readonly Regex
            Normal = CreateRegex($"You were damaged by a toxic substance for {AMOUNT} points of damage.");

        public MeHitByEnvironment(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static MeHitByEnvironment Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new MeHitByEnvironment(fight, timestamp, description);
            attackEvent.SetTargetToOwner();
            attackEvent.AttackResult = AttackResult.IndirectHit;

            if (attackEvent.TryMatch(Normal, out Match match))
            {
                attackEvent.SetAmount(match, 1);
            }
            else throw new NotSupportedException($"{EventName}: {description}");

            return attackEvent;
        }
    }
}
