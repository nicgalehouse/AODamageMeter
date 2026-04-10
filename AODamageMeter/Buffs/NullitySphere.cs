namespace AODamageMeter.Buffs
{
    public static class NullitySphere
    {
        private const string NS = "Nullity Sphere";

        public static readonly Buff NSI =  new Buff($"{NS}",       durationSeconds: 18, iconPath: "/Icons/NullitySphere.png", color: "#80BFFA");
        public static readonly Buff NSII = new Buff($"{NS} MK II", durationSeconds: 20, iconPath: "/Icons/NullitySphere.png", color: "#80BFFA");

        public static readonly Nanoline Nanoline = new Nanoline("Nullity Sphere", NSI, NSII);
    }
}
