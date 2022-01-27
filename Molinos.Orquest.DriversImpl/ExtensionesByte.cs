namespace Molinos.Orquest.DriversImpl
{
    public static class ExtensionesByte
    {
        public static bool BitAt(this byte aByte, int bitNumber)
        {
            //0 -> 7
            return (aByte & (1 << (7 - bitNumber))) != 0;
        }
    }
}
