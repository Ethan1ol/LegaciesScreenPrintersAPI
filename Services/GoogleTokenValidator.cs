using Google.Apis.Auth;

namespace LegaciesPrintersApi.Services
{
    /// <summary>The verified facts we use from a Google ID token.</summary>
    public record GoogleIdentity(string Subject, string Email, string? Name);

    /// <summary>Verifies Google ID tokens sent by the app (Credential Manager) and the website (Google Identity Services).</summary>
    // Reference: Google (2025) Verify the Google ID token on your server side - https://developers.google.com/identity/gsi/web/guides/verify-google-id-token
    // Reference: Google (2025) Google.Apis.Auth: GoogleJsonWebSignature - https://googleapis.dev/dotnet/Google.Apis.Auth/latest/api/Google.Apis.Auth.GoogleJsonWebSignature.html
    public class GoogleTokenValidator
    {
        private readonly string? _webClientId;
        private readonly ILogger<GoogleTokenValidator> _logger;

        public GoogleTokenValidator(IConfiguration config, ILogger<GoogleTokenValidator> logger)
        {
            _webClientId = config["Google:WebClientId"];
            _logger = logger;
        }

        /// <summary>True when a web client id is configured, so Google sign-in can work.</summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_webClientId);

        /// <summary>Returns the identity if the token is genuine and the email is verified; otherwise null.</summary>
        public async Task<GoogleIdentity?> ValidateAsync(string idToken)
        {
            if (!IsConfigured) return null;

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                    new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { _webClientId! } });

                if (!payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email))
                {
                    _logger.LogWarning("Google token rejected: email missing or not verified.");
                    return null;
                }

                return new GoogleIdentity(payload.Subject, payload.Email, payload.Name);
            }
            catch (InvalidJwtException ex)
            {
                // Never log the token itself: it's a credential.
                _logger.LogWarning("Google token rejected: {Reason}", ex.Message);
                return null;
            }
        }
    }
}
