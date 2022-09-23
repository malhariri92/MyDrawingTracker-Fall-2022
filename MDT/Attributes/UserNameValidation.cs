using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace MDT
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class UserNameValidation : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {

            string UserName = value == null ? String.Empty : value.ToString();
            if (String.IsNullOrEmpty(UserName))
            {
                return new ValidationResult($"Username field cannot be empty.");
            }
            if (UserName.Length > 50)
            {
                return new ValidationResult($"UserName length cannot exceed 50 characters.");
            }

            return ValidationResult.Success;

        }
    }
}