using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Exemple.Domain.Models;

namespace Example.Api.Models
{
    public class CancelGrade
    {
        [Required]
        public string CommandID { get; set; }

        [Required]
        public string Quantity { get; set; }

        [Required]
        public string Subtotal { get; set; }
    }
}
