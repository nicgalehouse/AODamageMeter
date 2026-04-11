namespace AODamageMeter.Nanolines
{
    public static class SoldierDetaunt
    {
        public static readonly Nano BypassMe =      new Nano("Bypass Me",        iconPath: "/Icons/Taunt.png", color: "#D4A030", detauntAmount: 2133);
        public static readonly Nano CircumventMe =  new Nano("Circumvent Me",    iconPath: "/Icons/Taunt.png", color: "#D4A030", detauntAmount: 4586);
        public static readonly Nano EludeMe =       new Nano("Elude Me",         iconPath: "/Icons/Taunt.png", color: "#D4A030", detauntAmount: 6080);
        public static readonly Nano KeepClearOfMe = new Nano("Keep Clear of Me", iconPath: "/Icons/Taunt.png", color: "#D4A030", detauntAmount: 7140);
        public static readonly Nano ShakeOffMe =    new Nano("Shake Off Me",     iconPath: "/Icons/Taunt.png", color: "#D4A030", detauntAmount: 9400);
        public static readonly Nano DesistMe =      new Nano("Desist Me",        iconPath: "/Icons/Taunt.png", color: "#D4A030", detauntAmount: 11000);

        public static readonly Nanoline Nanoline = new Nanoline("Soldier Detaunt", BypassMe, CircumventMe, EludeMe, KeepClearOfMe, ShakeOffMe, DesistMe);
    }
}
