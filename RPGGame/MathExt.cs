namespace RPGGame
{
    public static class MathExt
    {
        public static long Mod(long a, long b)
        {
            return ((a % b) + b) % b;
        }
    }
}
