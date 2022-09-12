using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace MDT
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class PasswordValidation : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {

            
            string Password = value == null ? String.Empty : value.ToString();
            if (String.IsNullOrEmpty(Password) || Password.Length < 10)
            {
                return new ValidationResult($"Passwords must be at least 10 characters long.");
            }

            if (Password.Length > 24)
            {
                return new ValidationResult($"Password length cannot exceed 24 characters.");
            }

            List<string> missingItems = new List<string>();
            Regex reSymbol = new Regex("[A-Z]");
            if (!reSymbol.IsMatch(Password))
            {
               missingItems.Add("1 uppercase letter");
            }

            reSymbol = new Regex("[a-z]");
            if (!reSymbol.IsMatch(Password))
            {
                missingItems.Add("1 lowercase letter");
            }

            reSymbol = new Regex("[0-9]");
            if (!reSymbol.IsMatch(Password))
            {
                missingItems.Add("1 number");
            }

            reSymbol = new Regex("[^a-zA-Z0-9]");
            if (!reSymbol.IsMatch(Password))
            {
                missingItems.Add("1 symbol character");
            }

            if (missingItems.Count == 0)
            {
                Regex pwvariation = new Regex("[pP][aA@4][sS$5]{2}[wW][oO0][rR][dD]");
                if (pwvariation.IsMatch(Password))
                {
                    return new ValidationResult($"Passwords cannot contain any variation of the word 'password'");
                }

                return ValidationResult.Success;
            }
            else
            {
                if (missingItems.Count > 2)
                {
                    missingItems[missingItems.Count - 1] = $"and {missingItems[missingItems.Count - 1]}";
                    return new ValidationResult($"Passwords must contain at least {string.Join(", ", missingItems)}.");
                }
                else
                {
                    return new ValidationResult($"Passwords must contain at least {string.Join(" and ", missingItems)}.");
                }
            }

        }
    }
}