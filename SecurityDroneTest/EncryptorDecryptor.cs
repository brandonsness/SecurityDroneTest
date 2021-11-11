namespace SecurityDroneTest
{
    public static class EncryptorDecryptor
    {
        //returns prandNum XOR command XOR clientKey
        public static int Encrypt(int randNum, int cmmd, int clientKey)
        {
            return randNum ^ cmmd ^ clientKey;
        }

        //returns decrypted command from prandNum XOR encrypted input XOR clientKey
        public static int Decrypt(int randNum, int input, int clientKey)
        {
            return randNum ^ input ^ clientKey;
        }
    }
}
