using System;
using System.Net;
using System.Threading.Tasks;

using ITHit.WebDAV.Client;
using ITHit.WebDAV.Client.Logger;

namespace WebDavCommon
{
    /// <summary>This class provides methods for creating <see cref="ITHit.WebDAV.Client.WebDavSessionAsync"/> instances.</summary>
    public static class SessionFactory
    {
        /// <summary>The license content.</summary>
        private const string LicenseContent = @"<?xml version='1.0'...";

        /// <summary>Creates and configure session. </summary>
        /// <param name="serverSettings">The server settings.</param>
        /// <returns>The configured <see cref="ITHit.WebDAV.Client.WebDavSessionAsync"/>.</returns>
        /// <exception cref="ArgumentNullException"> If <paramref name="serverSettings"/> is null. </exception>
        /// <exception cref="ITHit.WebDAV.Client.Exceptions.InvalidLicenseException"> The license is invalid. </exception>
        public static WebDavSessionAsync Create(ServerSettings serverSettings)
        {
            if (serverSettings == null) throw new ArgumentNullException(nameof(serverSettings));
#if !DEBUG
            FileLogger.Level = LogLevel.Off;
#endif

            // Disables ssl certificate validity check.
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            var session = new WebDavSessionAsync(LicenseContent);
            if (serverSettings.HasCredential)
            {
                session.Credentials = new NetworkCredential(serverSettings.UserName, serverSettings.Password);
            }

            return session;
        }

        /// <summary>Checks possibility to open list data with provided settings. </summary>
        /// <param name="serverSettings">The server settings.</param>
        /// <exception cref="ArgumentNullException"> If <paramref name="serverSettings"/> is null. </exception>
        /// <exception cref="ITHit.WebDAV.Client.Exceptions.UnauthorizedException">Incorrect credentials provided or insufficient permissions to access the requested item.</exception>
        /// <exception cref="ITHit.WebDAV.Client.Exceptions.NotFoundException">The requested folder doesn't exist on the server.</exception>
        /// <exception cref="ITHit.WebDAV.Client.Exceptions.ForbiddenException">The server refused to fulfill the request.</exception>
        /// <exception cref="ITHit.WebDAV.Client.Exceptions.WebDavException">Unexpected error occurred.</exception>
        public static void CheckConnection(ServerSettings serverSettings)
        {
            CheckConnectionAsync(serverSettings).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>Checks possibility to open list data with provided settings. </summary>
        /// <param name="serverSettings">The server settings.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serverSettings"/> is null. </exception>
        /// <exception cref="ITHit.WebDAV.Client.Exceptions.UnauthorizedException">Incorrect credentials provided or insufficient permissions to access the requested item.</exception>
        /// <exception cref="ITHit.WebDAV.Client.Exceptions.NotFoundException">The requested folder doesn't exist on the server.</exception>
        /// <exception cref="ITHit.WebDAV.Client.Exceptions.ForbiddenException">The server refused to fulfill the request.</exception>
        /// <exception cref="ITHit.WebDAV.Client.Exceptions.WebDavException">Unexpected error occurred.</exception>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task CheckConnectionAsync(ServerSettings serverSettings)
        {
            if (serverSettings == null) throw new ArgumentNullException(nameof(serverSettings));
            WebDavSessionAsync session = Create(serverSettings);
            await session.OpenItemAsync(serverSettings.ServerUri).ConfigureAwait(false);
        }
    }
}
