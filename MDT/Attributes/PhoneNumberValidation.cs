using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace MDT
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class PhoneNumberValidation : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {


            string PhoneNumber = value == null ? String.Empty : value.ToString();
            if (String.IsNullOrEmpty(PhoneNumber) || PhoneNumber.Length != 10)
            {
                return new ValidationResult($"PhoneNumbers must be 10 digits.");
            }

            Regex validatePhoneNumberRegex = new Regex("^\\+?\\d{1,4}?[-.\\s]?\\(?\\d{1,3}?\\)?[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,9}$");
            if (!validatePhoneNumberRegex.IsMatch(PhoneNumber))
            {
                return new ValidationResult($"Invalid phone number.");
            }

            return ValidationResult.Success;
        }
    }
}