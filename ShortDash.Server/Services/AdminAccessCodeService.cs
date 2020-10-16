using OtpNet;
using ShortDash.Server.Data;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class AdminAccessCodeService
    {
        private readonly ConfigurationService configurationService;

        public AdminAccessCodeService(ConfigurationService configurationService)
        {
            this.configurationService = configurationService;
        }

        public bool IsInitialized()
        {
            var adminAccessCode = configurationService.GetSecureSection<AdminAccessCode>(ConfigurationSections.AdminAccessCode);
            return adminAccessCode != null && !string.IsNullOrWhiteSpace(adminAccessCode.Data);
        }

        public bool IsValidAccessCode(string accessCode)
        {
            var administratorAccessCode = configurationService.GetSecureSection<AdminAccessCode>(ConfigurationSections.AdminAccessCode);
            var data = administratorAccessCode.Data;
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }
            return administratorAccessCode.AccessCodeType switch
            {
                AdminAccessCodeType.DynamicTotp => IsValidDynamicTotpAccessCode(accessCode, data),
                AdminAccessCodeType.Static => IsValidStaticAccessCode(accessCode, data),
                _ => false,
            };
        }

        public void SaveAccessCode(AdminAccessCodeType accessCodeType, string data)
        {
            var adminAccessCode = new AdminAccessCode
            {
                AccessCodeType = accessCodeType,
                Data = data
            };
            configurationService.SetSecureSection(ConfigurationSections.AdminAccessCode, adminAccessCode);
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
