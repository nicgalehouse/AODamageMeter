namespace AODamageMeter.Nanolines
{
    public static class TotalMirrorShield
    {
        private const string TMS = "Total Mirror Shield Mk";
        private const string AMS = "Augmented Mirror Shield MK";

        public static readonly Nano TMSMkI =    new Nano($"{TMS} I",    shortName: "TMS I",    durationSeconds: 20, iconPath: "/Icons/TotalMirrorShieldMkIToII.png",      color: "#6E9DFF");
        public static readonly Nano TMSMkII =   new Nano($"{TMS} II",   shortName: "TMS II",   durationSeconds: 26, iconPath: "/Icons/TotalMirrorShieldMkIToII.png",      color: "#6E9DFF");
        public static readonly Nano TMSMkIII =  new Nano($"{TMS} III",  shortName: "TMS III",  durationSeconds: 31, iconPath: "/Icons/TotalMirrorShieldMkIIIToIV.png",    color: "#9FF4FF");
        public static readonly Nano TMSMkIV =   new Nano($"{TMS} IV",   shortName: "TMS IV",   durationSeconds: 35, iconPath: "/Icons/TotalMirrorShieldMkIIIToIV.png",    color: "#9FF4FF");
        public static readonly Nano TMSMkV =    new Nano($"{TMS} V",    shortName: "TMS V",    durationSeconds: 41, iconPath: "/Icons/TotalMirrorShieldMkV.png",          color: "#C54242");
        public static readonly Nano TMSMkVI =   new Nano($"{TMS} VI",   shortName: "TMS VI",   durationSeconds: 46, iconPath: "/Icons/TotalMirrorShieldMkVI.png",         color: "#B377FF");
        public static readonly Nano TMSMkVII =  new Nano($"{TMS} VII",  shortName: "TMS VII",  durationSeconds: 52, iconPath: "/Icons/TotalMirrorShieldMkVIIToVIII.png",  color: "#FF9C4F");
        public static readonly Nano TMSMkVIII = new Nano($"{TMS} VIII", shortName: "TMS VIII", durationSeconds: 60, iconPath: "/Icons/TotalMirrorShieldMkVIIToVIII.png",  color: "#FF9C4F");
        public static readonly Nano TMSMkIX =   new Nano($"{TMS} IX",   shortName: "TMS IX",   durationSeconds: 70, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Nano TMSMkX =    new Nano($"{TMS} X",    shortName: "TMS X",    durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Nano AMSMkI =    new Nano($"{AMS} I",    shortName: "AMS I",    durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Nano AMSMkII =   new Nano($"{AMS} II",   shortName: "AMS II",   durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Nano AMSMkIII =  new Nano($"{AMS} III",  shortName: "AMS III",  durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Nano AMSMkIV =   new Nano($"{AMS} IV",   shortName: "AMS IV",   durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");
        public static readonly Nano AMSMkV =    new Nano($"{AMS} V",    shortName: "AMS V",    durationSeconds: 80, iconPath: "/Icons/TotalMirrorShieldMkIXAndAbove.png", color: "#FF5D40");

        public static readonly Nanoline Nanoline = new Nanoline("Total Mirror Shield",
            TMSMkI, TMSMkII, TMSMkIII, TMSMkIV, TMSMkV, TMSMkVI, TMSMkVII, TMSMkVIII, TMSMkIX, TMSMkX, AMSMkI, AMSMkII, AMSMkIII, AMSMkIV, AMSMkV);
    }
}
