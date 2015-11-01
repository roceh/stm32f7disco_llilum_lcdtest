namespace Managed.Misc
{
    public static class InteropHelper
    {
        /// <summary>
        /// Convert string to ASCII null terminated byte array
        /// </summary>
        /// <param name="value">string to convert</param>
        /// <returns>byte array</returns>
        public static byte[] GetNullTerminated(string value)
        {
            var result = new byte[value.Length + 1];

            for (int i = 0; i < value.Length; i++)
            {
                result[i] = (byte)value[i];
            }

            result[value.Length] = 0;

            return result;
        }
    }
}
