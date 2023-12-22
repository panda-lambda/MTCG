using MTCG.Models;
using MTCG.Repositories;
using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;


namespace MTCG.Services
{
    internal class UserService : IUserService
    {
        private readonly UserRepository _userRepository; // = new UserRepository();
        private const int SaltSize = 16; // 128 bit 
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 10000;  // Number of iterations


        public UserService(IUserRepository userRepository)
        {
            _userRepository = (UserRepository?)userRepository;
        }



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




        public bool CreateUser(string username, string password)
        {


            string hashedPassword = HashPassword(password);

            Console.WriteLine(hashedPassword);
            try
            {
                UserCredentials userCredentials = new UserCredentials { Username = username, Password = hashedPassword };
                if (_userRepository.registerUser(userCredentials))
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("e:");
                Console.WriteLine(e);
                Console.WriteLine("Soemthing went wrong in the service");
                return false;
            }


        }
    }
}

