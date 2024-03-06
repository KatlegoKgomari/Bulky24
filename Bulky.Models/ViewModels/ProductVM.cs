using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class ProductVM //We have a viewModel for our product and based on that, we arealready binding our category as well
    {
        public Product Product { get; set; }
        //We want an IEnumerable to hold the dropdown
        [ValidateNever] // We don't ever want it to be valdated
        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}
