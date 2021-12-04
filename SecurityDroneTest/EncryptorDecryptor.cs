using System;

namespace SecurityDroneTest
{
    public static class EncryptorDecryptor
    {
        //returns prandNum XOR command XOR clientKey
        public static byte [] Encrypt(byte [] randNum, byte [] cmmd, byte [] clientKey)
        {
            return XOR(XOR(randNum, cmmd), clientKey);
        }

        //returns decrypted command from prandNum XOR encrypted input XOR clientKey
        public static byte [] Decrypt(byte [] randNum, byte [] input, byte [] clientKey)
        {
            return XOR(XOR(randNum, input), clientKey);
        }

        //Function from https://stackoverflow.com/questions/20802857/xor-function-for-two-hex-byte-arrays/20802965
        //by user TypeIA
        //XORs two byte arrays
        public static byte[] XOR(byte[] arr1, byte[] arr2)
        {
            if (arr1.Length != arr2.Length)
                throw new ArgumentException("arrs are not the same length");

            byte[] result = new byte[arr1.Length];

            for (int i = 0; i < arr1.Length; ++i)
                result[i] = (byte)(arr1[i] ^ arr2[i]);

            return result;
        }
    }
}
