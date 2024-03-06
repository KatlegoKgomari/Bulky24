using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Utility
{
    public class EmailSender : IEmailSender //Implements the built-in IEmail sender
    {
        //We need to access our API key here so we create a property to store it
        public String SendGridSecret { get; set; }
        public EmailSender(IConfiguration _config) //With Stripe, we created a class that resembles the property and we populated in it program.cs  But we can also use Iconfiguration
        {
            SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey");
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {// Logic to send email
            var client = new SendGridClient(SendGridSecret);
            //from email address
            var from = new EmailAddress("katlego.kgomari@24.com", "Bulky Book"); //Giving a title 
            //What email do we want to send to?
            var to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            return client.SendEmailAsync(message); //This will finally send the email
        }
    }
}
