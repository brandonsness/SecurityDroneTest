using System;
using System.Security.Cryptography;

namespace SecurityDroneTest
{
	public class DroneRSA
	{
		private RSA rsa;
		public RSAParameters rsaKeyInfo;

		public DroneRSA()
		{
			// Creates public/private key pair
			rsa = RSA.Create();

			// Change paramter to false to keep private key a secret.
			// Only keeping public to demonstrate key is generated.
			rsaKeyInfo = rsa.ExportParameters(true);
		}
	}
}
