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
        private readonly SHA256 hash = SHA256.Create();

        public string HashString(string str)
        {
            return string.Concat(this.hash
                .ComputeHash(Encoding.UTF8.GetBytes(str))
                .Select(b => b.ToString("X2")));

            ////    using (var hash = SHA256.Create())
            ////    {
            ////        return string.Concat(hash
            ////            .ComputeHash(Encoding.UTF8.GetBytes(str))
            ////            .Select(b => b.ToString("X2")));
            ////    }
        }
    }
}