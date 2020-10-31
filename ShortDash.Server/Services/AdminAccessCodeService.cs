using OtpNet;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Services;
using ShortDash.Server.Data;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class AdminAccessCodeService
    {
        private readonly ConfigurationService configurationService;
        private readonly IDataProtectionService dataProtectionService;

        public AdminAccessCodeService(ConfigurationService configurationService, IDataProtectionService dataProtectionService)
        {
            this.configurationService = configurationService;
            this.dataProtectionService = dataProtectionService;
        }

        public bool IsInitialized()
        {
            var adminAccessCode = configurationService.GetSecureSection(ConfigurationSections.AdminAccessCode);
            return !string.IsNullOrWhiteSpace(adminAccessCode);
        }

        public bool IsValidAccessCode(string userCode)
        {
            var adminAccessCode = configurationService.GetSecureSection(ConfigurationSections.AdminAccessCode);
            if (string.IsNullOrWhiteSpace(adminAccessCode))
            {
                return false;
            }
            return IsValidDynamicTotpAccessCode(userCode, adminAccessCode) || IsValidStaticAccessCode(userCode, adminAccessCode);
        }

        public void SaveAccessCode(string data)
        {
            // Derive a data protection key from the admin access code
            using var derivedBytes = new Rfc2898DeriveBytes(data, saltSize: 16, iterations: 100000, HashAlgorithmName.SHA256);
            using var aes = Aes.Create();
            // Persist the new data protection scheme
            dataProtectionService.SetKey(derivedBytes.GetBytes(aes.Key.Length));
            configurationService.SetSecureSection(ConfigurationSections.DataProtectionSalt, Convert.ToBase64String(derivedBytes.Salt));
            configurationService.SetSecureSection(ConfigurationSections.AdminAccessCode, data);
        }

        private bool IsValidDynamicTotpAccessCode(string accessCode, string data)
        {
            var base32Bytes = Base32Encoding.ToBytes(data);
            var otp = new Totp(base32Bytes);
            return otp.ComputeTotp().Equals(accessCode);
        }

        private bool IsValidStaticAccessCode(string accessCode, string data)
        {
            return data.Equals(accessCode);
        }
    }
}
