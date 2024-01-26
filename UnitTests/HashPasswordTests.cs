

using MTCG.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Security.Cryptography;

namespace UnitTests
{
    public class HashPasswordTests
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;

        internal static string HashPassword(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                return $"{Iterations}.{salt}.{key}";
            }
        }

        [Test]
        public void PasswordDiffINputDiffOutput()
        {
            string password1 = "TestPassword1";
            string password2 = "TestPassword2";
            string hash1 = HashPassword(password1);
            string hash2 = HashPassword(password2);

            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void PasswordSameINputDiffOutput()
        {
            string password1 = "TestPassword1";
            string password2 = "TestPassword1";
            string hash1 = HashPassword(password1);
            string hash2 = HashPassword(password2);

            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void PasswordEmptyValidHashs()
        {
            string password = "";
            string hash = HashPassword(password);

            StringAssert.IsMatch(@"^\d+\.[\w+/=]+\.[\w+/=]+$", hash);

        }


        [Test]
        public void PasswordSpecialChards()
        {
            string password = "@\"§ßw[rd!";
            string hash = HashPassword(password);
            StringAssert.IsMatch(@"^\d+\.[\w+/=]+\.[\w+/=]+$", hash);
        }

        [Test]
        public void PasswordNullException()
        {
            Assert.Throws<ArgumentNullException>(() => HashPassword(null));
        }


    }
}
