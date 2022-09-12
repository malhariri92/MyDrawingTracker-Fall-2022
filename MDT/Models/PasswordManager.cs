using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;

namespace MDT.Models
{
    /// <summary>
    /// Handles Non AD user logins and password resets. 
    /// </summary>
    public static class PasswordManager
    {
        private static int saltLength = 25;
        private static int hashLength = 48;

        /// <summary>
        /// Results of login attempt
        /// </summary>
        public enum Result
        {
            /// <summary>
            /// Username does not match existing app users
            /// </summary>
            UserNotFound,

            /// <summary>
            /// User has exceeded log in attempts
            /// </summary>
            UserLocked,

            /// <summary>
            /// Entered password is not correct
            /// </summary>
            BadPassword,

            /// <summary>
            /// User must reset their password
            /// </summary>
            MustResetPassword,

            /// <summary>
            /// User login attempt is successful
            /// </summary>
            SuccessfulLogin

        }

        /// <summary>
        /// Hashes the provided string
        /// </summary>
        /// <param name="pass">The string to hash</param>
        /// <param name="salt">The salt of an existing password</param>
        /// <returns>A string represtentation of the hashed password and salt</returns>
        private static string GetHash(string pass, byte[] salt = null)
        {
            if (salt == null)
            {
                RandomNumberGenerator.Create().GetBytes(salt = new byte[saltLength]);
            }

            var crypted = new Rfc2898DeriveBytes(pass, salt, 10000);

            byte[] hash = crypted.GetBytes(hashLength);
            byte[] hashBytes = new byte[saltLength + hashLength];
            Array.Copy(salt, 0, hashBytes, 0, saltLength);
            Array.Copy(hash, 0, hashBytes, saltLength, hashLength);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Gets the salt from a saved password hash
        /// </summary>
        /// <param name="hash">The hash of the existing password</param>
        /// <returns>A byte array of the salt</returns>
        private static byte[] GetSalt(string hash)
        {
            byte[] salt, hashBytes = Convert.FromBase64String(hash); ;
            salt = new byte[saltLength];
            Array.Copy(hashBytes, 0, salt, 0, saltLength);

            return salt;
        }

        /// <summary>
        /// Converts the provided hashes to byte arrays and compares each byte.
        /// </summary>
        /// <param name="hash1">First hash</param>
        /// <param name="hash2">Second hash</param>
        /// <returns>True/False if hashes are exact matches</returns>
        private static bool CompareHashes(string hash1, string hash2)
        {
            byte[] byte1 = Convert.FromBase64String(hash1);
            byte[] byte2 = Convert.FromBase64String(hash2);

            if (byte1.Length != byte2.Length)
            {
                return false;
            }

            for (int i = 0; i < byte1.Length; i++)
            {
                if (byte1[i] != byte2[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determine if the provided username and password combination are valid for user login. The Result and UserDTO properties
        /// of the NonADUserLogin object will be changed based on the result of this attempt.
        /// </summary>
        /// <param name="usr">A NonADUserLogin object with the username and password properties set.</param>
        public static void AttemptLogin(LoginDTO usr)
        {
            using (var db = new DbEntities())
            {
                User user = db.Users.Where(u => u.EmailAddress.Equals(usr.EmailAddress))
                                    .FirstOrDefault();

                if (user == null)
                {
                    usr.LoginResult = Result.UserNotFound;
                    return;
                }

                user.Hash = db.Users.Find(user.UserId).Hash;


                if (user.Hash == null)
                {
                    usr.LoginResult = Result.MustResetPassword;
                    return;
                }

                string pass = GetHash(usr.Password);
                bool passMatch = CompareHashes(GetHash(usr.Password, GetSalt(user.Hash)), user.Hash);

                if (passMatch)
                {
                    usr.User = user;
                    usr.LoginResult = Result.SuccessfulLogin;
                }
                else
                {
                    usr.LoginResult = Result.BadPassword;
                }

               

                //usr.UserLocked = au.LockedOut;
                //usr.FailedCount = au.FailedCount;
                //usr.FirstFailed = au.FirstFailedAttempt;

                //if (au.LockedOut)
                //{
                //    usr.LoginResult = Result.UserLocked;
                //}
            }
        }

        /// <summary>
        /// Tests equality of a string to a previous hash
        /// </summary>
        /// <param name="unhashed">unhashed string to check</param>
        /// <param name="hash">the has to compare the string to</param>
        /// <returns>True if hashed version of unhashed matches the specified hash, otherwise false</returns>
        public static bool TestHashMatch(string unhashed, string hash)
        {
            return CompareHashes(GetHash(unhashed, GetSalt(hash)), hash);
        }

        /// <summary>
        /// Updates the hash in the Users table and adds the current hash to the history if the current hash is not null.
        /// </summary>
        /// <param name="userId">The Entity id of the user to update</param>
        /// <param name="str">The unhashed string to be hashed and saved</param>
        /// <returns>True if successful, otherwise false</returns>
        public static bool SetNewHash(int userId, string str)
        {
            try
            {
                using (var db = new DbEntities())
                {
                    User user = db.Users.Find(userId);
                    if (user == null)
                    {
                        return false;
                    }
                    //if (user.Hash != null)
                    //{
                    //    db.HashHistories.Add(new HashHistory()
                    //    {
                    //        UserId = user.UserId,
                    //        Hash = user.Hash,
                    //        LastDate = DateTime.Now
                    //    });
                    //}
                    user.Hash = GetHash(str);
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

    }
}
