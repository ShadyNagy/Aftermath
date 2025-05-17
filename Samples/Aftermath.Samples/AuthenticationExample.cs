using Aftermath.Attributes;
using Aftermath.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Aftermath.Samples;

public class AuthenticationExample
{
	public static async Task Main()
	{
		var services = new ServiceCollection();

		services.AddAftermath();

		services.AddScoped<IUserManager, UserManager>();
		services.AddSingleton<IAuthenticationLogger, AuthenticationLogger>();
		services.AddSingleton<SecurityAuditService>();
		services.AddSingleton<BruteForceDetector>();

		services.AddHookedScoped<IUserService, UserService>();

		var serviceProvider = services.BuildServiceProvider();

		var userService = serviceProvider.GetRequiredService<IUserService>();

		try
		{
			var successResult = await userService.AuthenticateAsync("john.doe", "correct-password");
			Console.WriteLine($"Authentication result: {(successResult ? "Success" : "Failure")}");

			var failureResult = await userService.AuthenticateAsync("john.doe", "wrong-password");
			Console.WriteLine($"Authentication result: {(failureResult ? "Success" : "Failure")}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Authentication error: {ex.Message}");
		}
	}

	public interface IUserService
	{
		[CallAfter(typeof(IAuthenticationLogger), nameof(IAuthenticationLogger.LogAuthenticationAttemptAsync))]
		[CallAfter(typeof(SecurityAuditService), nameof(SecurityAuditService.RecordAuthenticationAttempt))]
		[CallAfter(typeof(BruteForceDetector), nameof(BruteForceDetector.CheckForBruteForceAttack))]
		[MapParameter("username", "username")]
		[MapReturnValue("success")]
		Task<bool> AuthenticateAsync(string username, string password);
		Task<User> GetUserByIdAsync(int userId);
	}

	public interface IUserManager
	{
		Task<User> GetUserByUsernameAsync(string username);
		Task<bool> ValidatePasswordAsync(User user, string password);
	}

	public interface IAuthenticationLogger
	{
		Task LogAuthenticationAttemptAsync(string username, bool success);
	}

	public class User
	{
		public int Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public DateTime LastLogin { get; set; }
		public int LoginAttempts { get; set; }
	}

	public class UserService : IUserService
	{
		private readonly IUserManager _userManager;

		public UserService(IUserManager userManager)
		{
			_userManager = userManager;
		}

		[CallAfter(typeof(IAuthenticationLogger), nameof(IAuthenticationLogger.LogAuthenticationAttemptAsync))]
		[CallAfter(typeof(SecurityAuditService), nameof(SecurityAuditService.RecordAuthenticationAttempt))]
		[CallAfter(typeof(BruteForceDetector), nameof(BruteForceDetector.CheckForBruteForceAttack))]
		[MapParameter("username", "username")]
		[MapReturnValue("success")]
		public async Task<bool> AuthenticateAsync(string username, string password)
		{
			var user = await _userManager.GetUserByUsernameAsync(username);

			if (user == null)
				return false;

			bool isValid = await _userManager.ValidatePasswordAsync(user, password);

			return isValid;
		}

		public async Task<User> GetUserByIdAsync(int userId)
		{
			return new User { Id = userId };
		}
	}

	public class UserManager : IUserManager
	{
		public async Task<User> GetUserByUsernameAsync(string username)
		{
			if (username == "john.doe")
			{
				return new User
				{
					Id = 1,
					Username = "john.doe",
					Email = "john.doe@example.com",
					PasswordHash = "hashed-password",
					LastLogin = DateTime.UtcNow.AddDays(-1),
					LoginAttempts = 0
				};
			}

			return null;
		}

		public Task<bool> ValidatePasswordAsync(User user, string password)
		{
			return Task.FromResult(password == "correct-password");
		}
	}

	public class AuthenticationLogger : IAuthenticationLogger
	{
		public Task LogAuthenticationAttemptAsync(string username, bool success)
		{
			Console.WriteLine($"AUTH LOG: User '{username}' authentication {(success ? "succeeded" : "failed")} at {DateTime.UtcNow}");
			return Task.CompletedTask;
		}
	}

	public class SecurityAuditService
	{
		public void RecordAuthenticationAttempt(bool success, string username, MethodExecutionContext context)
		{
			var ipAddress = "192.168.1.1";
			Console.WriteLine($"SECURITY AUDIT: Authentication attempt from IP {ipAddress} for user '{username}' {(success ? "succeeded" : "failed")}");
		}
	}

	public class BruteForceDetector
	{
		private int _failedAttempts = 0;
		private readonly int _maxAttempts = 5;

		public void CheckForBruteForceAttack(bool success, string username)
		{
			if (!success)
			{
				_failedAttempts++;
				Console.WriteLine($"SECURITY: Failed login attempt #{_failedAttempts} for user '{username}'");

				if (_failedAttempts >= _maxAttempts)
				{
					Console.WriteLine($"SECURITY ALERT: Possible brute force attack detected for user '{username}'");
				}
			}
			else
			{
				_failedAttempts = 0;
			}
		}
	}
}