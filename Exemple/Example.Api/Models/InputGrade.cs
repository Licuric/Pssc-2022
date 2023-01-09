using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Exemple.Domain.Models;

namespace Example.Api.Models
{
    public class InputGrade
    {
        public string RegistrationNumber { get; set; }

        public decimal Exam { get; set; }

        public decimal Activity { get; set; }
    }
}
