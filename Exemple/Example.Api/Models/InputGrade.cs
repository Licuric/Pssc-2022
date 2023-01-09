using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Exemple.Domain.Models;

namespace Example.Api.Models
{
    public class InputGrade
    {
        [Required]
        public string RegistrationNumber { get; set; }

        [Required]
        public string Exam { get; set; }

        [Required]
        public string Activity { get; set; }
    }
}
