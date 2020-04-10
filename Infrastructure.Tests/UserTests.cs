using System;
using Infrastructure.Controller;
using Infrastructure.Model;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NUnit.Framework;

namespace Infrastructure.Tests
{
    public class UserControllerTests
    {
        [Test]
        public void TestHash()
        {
            var password = "password";

            (string salt, string hash) = UserController.Hash(password);

            Assert.AreEqual(Convert.ToBase64String(Convert.FromBase64String(hash)), hash);

            Console.WriteLine(salt + " - " + hash);

            string computed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password,
                Convert.FromBase64String(hash),
                KeyDerivationPrf.HMACSHA1,
                10000,
                256 / 8
            ));

            Assert.AreEqual(computed, hash);
        }

        // [Test]
        // public void TestCreate()
        // {
        //     User? old = UserModel.Find("ajruckman");
        //     if (old != null)
        //         UserModel.Remove(old);
        //
        //     User user = UserController.Create("ajruckman", "password", UserRole.Administrator);
        //
        //     UserModel.Submit(user);
        //
        //     User? match = UserModel.Find("ajruckman");
        //     Assert.NotNull(match);
        //
        //     bool passwordCorrect = UserController.ComparePassword(match, "password");
        //     Assert.True(passwordCorrect);
        //
        //     bool passwordIncorrect = !UserController.ComparePassword(match, "wrong");
        //     Assert.True(passwordIncorrect);
        // }
    }
}