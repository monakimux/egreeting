using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace sendmail.Controllers
{
	public class HomeController : Controller
	{
		[HttpGet]
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Index(FormCollection form)
		{

			// IF THE SUBMISSION IS NOT VALID
			if (!await isValid(Request))
			{
				ModelState.AddModelError("g-recaptcha", "Really?!  You're not a robot?  Really??");
				return View();
			}

			//CONTENT
			var mailer = new MimeMessage();
			var recipient = new MailboxAddress(form["to-name"], form["to-email"]);
			var sender = new MailboxAddress(form["from-name"], form["from-email"]);
			var subject = form["subject"];
			var mesg = form["comments"];
			mesg += "<br>The info below is optional<hr>";
			mesg += "<br>phone: " + form["phone"];
			var host = "mail02.wonderwomencoders.com";
			var replyto = new MailboxAddress(form["email"]);
			var sender_email = "sender170802@mail02.wonderwomencoders.com";
			var sender_password = "Got2code!";

			//ENGINE
			mailer.To.Add(recipient);
			mailer.Subject = subject;
			mailer.From.Add(sender);
			mailer.ReplyTo.Add(replyto);

			mailer.Body = new TextPart("html")
			{
				Text = mesg
			};

			try
			{
				using (var client = new SmtpClient())
				{
					client.Connect(host, 25, MailKit.Security.SecureSocketOptions.None);
					client.Authenticate(sender_email, sender_password);
					client.Send(mailer);
					client.Disconnect(true);
				}
			}
			catch { };

			return View();

			// RE-CAPTCHA VALIDATION
			private async Task<bool> isValid(HttpRequestBase request)
			{
				var myGoogleSecretKey = "";
				var response = request.Form["g-recaptcha-response"];

				try
				{
					using (var client = new HttpClient())
					{
						var values = new Dictionary<string, string>();
						values.Add("secret", myGoogleSecretKey);
						values.Add("response", response);

						var query = new FormUrlEncodedContent(values);


						var post = client.PostAsync("https://www.google.com/recaptcha/api/siteverify", query);

						var json = await post.Result.Content.ReadAsStringAsync();

						if (json == null)
							return false;

						var results = JsonConvert.DeserializeObject<dynamic>(json);

						return results.success;
					}

				}
				catch (Exception ex)
				{
				}


				return false;
			}

		}
	}

}