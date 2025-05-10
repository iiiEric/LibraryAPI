using LibraryAPI.DTOs;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace LibraryAPI.Services
{
    public class HashServicies : IHashServicies
    {
        public HashResultDTO Hash(string input)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Hash(input, salt);
        }

        public HashResultDTO Hash(string input, byte[] isalt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: input,
                salt: isalt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
                ));

            return new HashResultDTO { Hash = hashed, Salt = isalt };
        }
    }
}
