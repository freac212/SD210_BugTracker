using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using SD210_BugTracker_DGrouette.Models;

namespace SD210_BugTracker_DGrouette
{
    public class EmailService : IIdentityMessageService
    {
        private string SmtpHost = ConfigurationManager.AppSettings["SmtpHost"];
        private int SmtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
        private string SmtpUserName = ConfigurationManager.AppSettings["SmtpUsername"];
        private string SmtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
        private string SmtpFrom = ConfigurationManager.AppSettings["SmtpFrom"];

        public void Send(string to, string body, string subject)
        {
            // Create the message for the email
            var message = new MailMessage(SmtpFrom, to)
            {
                Body = body,
                Subject = subject,
                IsBodyHtml = true
            };

            // Opening a connection with the SMTP client in order to send emails out.
            var smtpClient = new SmtpClient(SmtpHost, SmtpPort)
            {
                Credentials = new NetworkCredential(SmtpUserName, SmtpPassword),
                EnableSsl = true
            };

            // Have that client send the message.
            smtpClient.Send(message);
        }

        public Task SendAsync(IdentityMessage message)
        {
            // This method is required for EntityFramework to send an email with their default methods.


            // Plug in your email service here to send an email.
            // Because this is async, but we dont want it to be, we use run.
            return Task.Run(() => Send(message.Destination, message.Body, message.Subject));
        }
}

public class SmsService : IIdentityMessageService
{
    public Task SendAsync(IdentityMessage message)
    {
        // Plug in your SMS service here to send a text message.
        return Task.FromResult(0);
    }
}

// Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
public class ApplicationUserManager : UserManager<ApplicationUser>
{
    public ApplicationUserManager(IUserStore<ApplicationUser> store)
        : base(store)
    {
    }

    public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
    {
        var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
        // Configure validation logic for usernames
        manager.UserValidator = new UserValidator<ApplicationUser>(manager)
        {
            AllowOnlyAlphanumericUserNames = false, //++Q
            RequireUniqueEmail = true
        };

        // Configure validation logic for passwords
        manager.PasswordValidator = new PasswordValidator
        {
            RequiredLength = 6,
            RequireNonLetterOrDigit = true,
            RequireDigit = true,
            RequireLowercase = true,
            RequireUppercase = true,
        };

        // Configure user lockout defaults
        manager.UserLockoutEnabledByDefault = true;
        manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
        manager.MaxFailedAccessAttemptsBeforeLockout = 5;

        // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
        // You can write your own provider and plug it in here.
        manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
        {
            MessageFormat = "Your security code is {0}"
        });
        manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
        {
            Subject = "Security Code",
            BodyFormat = "Your security code is {0}"
        });
        manager.EmailService = new EmailService();
        manager.SmsService = new SmsService();
        var dataProtectionProvider = options.DataProtectionProvider;
        if (dataProtectionProvider != null)
        {
            manager.UserTokenProvider =
                new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
        }
        return manager;
    }
}

// Configure the application sign-in manager which is used in this application.
public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
{
    public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
        : base(userManager, authenticationManager)
    {
    }

    public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
    {
        return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
    }

    public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
    {
        return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
    }
}
}
