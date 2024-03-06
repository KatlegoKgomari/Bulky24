using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Utility
{
	public class StripeSettings
	{

		//Spelling must be exactly the same. Getting these keys from app settings.json
		public string SecretKey { get; set; }
		public string PublishableKey { get; set; }
	}
}
