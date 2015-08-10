using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace GuestListManagerWeb.Models
{
    public class GuestListMetadata
    {
        [StringLength(50)]
        [Required]
        public string FirstName;
        [StringLength(50)]
        [Required]
        public string LastName;
    }
}