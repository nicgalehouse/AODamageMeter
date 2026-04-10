namespace AODamageMeter.Buffs
{
    public static class TotalMirrorShield
    {
        private const string TMS = "Total Mirror Shield Mk";
        private const string AMS = "Augmented Mirror Shield MK";

        public static readonly Buff TMSMkI =    new Buff($"{TMS} I",    shortName: "TMS I",    durationSeconds: 20, iconPath: "/Icons/TotalMirrorShieldMkIToII.png",      color: "#6E9DFF");
        public static readonly Buff TMSMkII =   new Buff($"{TMS} II",   shortName: "TMS II",   durationSeconds: 26, iconPath: "/Icons/TotalMirrorShieldMkIToII.png",      color: "#6E9DFF");
        public static readonly Buff TMSMkIII =  new Buff($"{TMS} III",  shortName: "TMS III",  durationSeconds: 31, iconPath: "/Icons/TotalMirrorShieldMkIIIToIV.png",    color: "#9FF4FF");
        public static readonly Buff TMSMkIV =   new Buff($"{TMS} IV",   shortName: "TMS IV",   durationSeconds: 35, iconPath: "/Icons/TotalMirrorShieldMkIIIToIV.png",    color: "#9FF4FF");
        public static readonly Buff TMSMkV =    new Buff($"{TMS} V",    shortName: "TMS V",    durationSeconds: 41, iconPath: "/Icons/TotalMirrorShieldMkV.png",          color: "#C54242");
        public static readonly Buff TMSMkVI =   new Buff($"{TMS} VI",   shortName: "TMS VI",   durationSeconds: 46, iconPath: "/Icons/TotalMirrorShieldMkVI.png",         color: "#B377FF");
        public static readonly Buff TMSMkVII =  new Buff($"{TMS} VII",  shortName: "TMS VII",  durationSeconds: 52, iconPath: "/Icons/TotalMirrorShieldMkVIIToVIII.png",  color: "#FF9C4F");
        public static readonly Buff TMSMkVIII = new Buff($"{TMS} VIII", shortName: "TMS VIII", durationSeconds: 60, iconPath: "/Icons/TotalMirrorShieldMkVIIToVIII.png",  color: "#FF9C4F");
        public static readonly Buff TMSMkIX =   new Buff($"{TMS} IX",   shortName: "TMS IX",   durationSeconds: 70, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Buff TMSMkX =    new Buff($"{TMS} X",    shortName: "TMS X",    durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Buff AMSMkI =    new Buff($"{AMS} I",    shortName: "AMS I",    durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Buff AMSMkII =   new Buff($"{AMS} II",   shortName: "AMS II",   durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Buff AMSMkIII =  new Buff($"{AMS} III",  shortName: "AMS III",  durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Buff AMSMkIV =   new Buff($"{AMS} IV",   shortName: "AMS IV",   durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Buff AMSMkV =    new Buff($"{AMS} V",    shortName: "AMS V",    durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");

        public static readonly Nanoline Nanoline = new Nanoline("Total Mirror Shield",
            TMSMkI, TMSMkII, TMSMkIII, TMSMkIV, TMSMkV, TMSMkVI, TMSMkVII, TMSMkVIII, TMSMkIX, TMSMkX, AMSMkI, AMSMkII, AMSMkIII, AMSMkIV, AMSMkV);
    }
}
