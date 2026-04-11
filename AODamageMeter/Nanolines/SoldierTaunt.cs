namespace AODamageMeter.Nanolines
{
    public static class SoldierTaunt
    {
        public static readonly Nano DrawAttention =         new Nano("Draw Attention",         iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 199);
        public static readonly Nano EnragedMind =           new Nano("Enraged Mind",           iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 349);
        public static readonly Nano AnnoyingPresence =      new Nano("Annoying Presence",      iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 712);
        public static readonly Nano AggressiveCaptivation = new Nano("Aggressive Captivation", iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 890);
        public static readonly Nano IdAssault =             new Nano("Id Assault",             iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 1244);
        public static readonly Nano OnlyYouOnlyMe =         new Nano("Only You, Only Me",      iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 1617);
        public static readonly Nano ObviousVictim =         new Nano("Obvious Victim",         iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 2250);
        public static readonly Nano ClearVictim =           new Nano("Clear Victim",           iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 4580);
        public static readonly Nano DistinctVictim =        new Nano("Distinct Victim",        iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 6125);
        public static readonly Nano UndeniableVictim =      new Nano("Undeniable Victim",      iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 8600);
        public static readonly Nano UnmistakableVictim =    new Nano("Unmistakable Victim",    iconPath: "/Icons/Taunt.png", color: "#D4A030", tauntAmount: 12000);

        public static readonly Nanoline Nanoline = new Nanoline("Soldier Taunt",
            DrawAttention, EnragedMind, AnnoyingPresence, AggressiveCaptivation, IdAssault,
            OnlyYouOnlyMe, ObviousVictim, ClearVictim, DistinctVictim, UndeniableVictim, UnmistakableVictim);
    }
}
