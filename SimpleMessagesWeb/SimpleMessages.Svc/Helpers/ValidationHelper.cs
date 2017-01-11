using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Svc.Helpers
{
    internal static class ValidationHelper
    {
        internal static string[] Validate<T>(T instanceToValidate)
            where T: class
        {
            var context = new ValidationContext(instanceToValidate);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(instanceToValidate, context, results, true);

            if (!isValid)
            {
                var textResults = new List<string>();
                foreach (var validationResult in results)
                {
                    textResults.Add(validationResult.ErrorMessage);
                }
                return textResults.ToArray();
            }

            return new string[0];
        }

    }
}
