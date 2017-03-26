// <copyright file="SecurityKeyGenerator.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Services
{
    using System;
    using System.Security.Cryptography;
    using Common.Models;

    public class SecurityKeyGenerator : ISecurityKeyGenerator
    {
        private const int LengthInBytes = 16;

        public SecurityKeys CreateRandomKeys()
        {
            byte[] primaryRawRandomBytes = new byte[LengthInBytes];
            byte[] secondaryRawRandomBytes = new byte[LengthInBytes];

            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(primaryRawRandomBytes);
                rngCsp.GetBytes(secondaryRawRandomBytes);
            }

            string s1 = Convert.ToBase64String(primaryRawRandomBytes);
            string s2 = Convert.ToBase64String(secondaryRawRandomBytes);

            return new SecurityKeys(s1, s2);
        }
    }
}
