// <copyright file="HashService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Services
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    public class HashService : IHashService
    {
        public string HashString(string str)
        {
            using (var hash = SHA256.Create())
            {
                return string.Concat(hash
                    .ComputeHash(Encoding.UTF8.GetBytes(str))
                    .Select(b => b.ToString("X2")));
            }
        }
    }
}