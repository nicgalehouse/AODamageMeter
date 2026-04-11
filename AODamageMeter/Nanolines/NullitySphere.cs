namespace AODamageMeter.Nanolines
{
    public static class NullitySphere
    {
        private const string NS = "Nullity Sphere";

        public static readonly Nano NSI =  new Nano($"{NS}",       durationSeconds: 18, iconPath: "/Icons/NullitySphere.png", color: "#80BFFA");
        public static readonly Nano NSII = new Nano($"{NS} MK II", durationSeconds: 20, iconPath: "/Icons/NullitySphere.png", color: "#80BFFA");

        public static readonly Nanoline Nanoline = new Nanoline("Nullity Sphere", NSI, NSII);
    }
}
