using System;
namespace Libmemo
{
    public class AuthCredentials
    {
		public AuthCredentials(string Email, string Password)
		{
			this.Email = Email;
			this.Password = Password;
		}

		public string Email { get; set; }
		public string Password { get; set; }

		class Serializable
		{
			public string email { get; set; }
			public string password { get; set; }
		}

		public string Serialize()
		{
			var serializable = new Serializable()
			{
				email = Email,
				password = Password
			};

			return Newtonsoft.Json.JsonConvert.SerializeObject(serializable);
		}

		public static AuthCredentials Deserialize(string serialized)
		{
            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<Serializable>(serialized);

			return new AuthCredentials(
				Email: deserialized.email,
				Password: deserialized.password
			);
		}
    }
}
