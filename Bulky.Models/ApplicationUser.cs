using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class ApplicationUser:IdentityUser
    {
        //We have all the defaukt settings of Identity user
        [Required]
        public string Name { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        public int? CompanyId { get; set; } //Nullable because it is possile that the use is a customer and that means that they do not belong to a company. But if they are a company user, they will have a company id assigned
        //We then need teh navigation proprty  (company id is the navigation property for this company that we have )
        [ForeignKey("CompanyId")]
        [ValidateNever] //Not validating because users won't always be part of a company
        public Company Company { get; set; } //We made a change to our core model so we have to add a new migration
    }
}
