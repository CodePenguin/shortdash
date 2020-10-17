using Microsoft.AspNetCore.DataProtection;
using ShortDash.Server.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ShortDash.Server.Data
{
    public class DataSignatureManager
    {
        private const int CurrentSignatureVersion = 1;
        private readonly IDataProtector dataProtector;
        private readonly byte[] protectedKey;

        public DataSignatureManager(ApplicationDbContext dbContext, IDataProtectionProvider dataProtectionProvider)
        {
            dataProtector = dataProtectionProvider.CreateProtector("DataSignatureManager");
            protectedKey = InitializeKey(dbContext);
        }

        public void GenerateSignature(object data)
        {
            var originalSignature = GetSignatureFromObject(data);
            var signature = GenerateSignature(data, CurrentSignatureVersion);
            if (originalSignature != signature)
            {
                SetSignatureForObject(data, signature);
            }
        }

        public bool VerifySignature(object data)
        {
            var signature = GetSignatureFromObject(data);
            var signatureParts = Array.Empty<string>();
            if (!string.IsNullOrWhiteSpace(signature))
            {
                signatureParts = signature.Split(":");
            }
            if (signatureParts.Length != 2 || !int.TryParse(signatureParts[0], out var signatureVersion))
            {
                signatureVersion = CurrentSignatureVersion;
            }
            var generatedSignature = GenerateSignature(data, signatureVersion);
            return signature == generatedSignature;
        }

        private string GenerateSignature(object data, int signatureVersion)
        {
            var signatureData = GenerateSignatureData(data);
            if (string.IsNullOrEmpty(signatureData))
            {
                return null;
            }
            using var hmac = new HMACSHA256(dataProtector.Unprotect(protectedKey));
            return signatureVersion.ToString() + ":" + Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureData)));
        }

        private string GenerateSignatureData(DashboardDevice device)
        {
            return $"{device.DashboardDeviceId}:{device.DeviceClaims}";
        }

        private string GenerateSignatureData(DashboardActionTarget target)
        {
            if (target.DashboardActionTargetId == DashboardActionTarget.ServerTargetId && string.IsNullOrEmpty(target.PublicKey))
            {
                return null;
            }
            return $"{target.DashboardActionTargetId}:{target.PublicKey}";
        }

        private string GenerateSignatureData(object data)
        {
            return data switch
            {
                DashboardAction action => GenerateSignatureData(action),
                DashboardActionTarget target => GenerateSignatureData(target),
                DashboardDevice device => GenerateSignatureData(device),
                _ => throw new NotImplementedException($"Unhandled type: {data.GetType().Name}"),
            };
        }

        private string GenerateSignatureData(DashboardAction action)
        {
            return $"{action.ActionTypeName}:{action.DashboardActionId}:{action.Parameters}";
        }

        private string GetSignatureFromObject(object data)
        {
            return data switch
            {
                DashboardAction action => action.DataSignature,
                DashboardActionTarget target => target.DataSignature,
                DashboardDevice device => device.DataSignature,
                _ => throw new NotImplementedException($"Unhandled type: {data.GetType().Name}"),
            };
        }

        private byte[] InitializeKey(ApplicationDbContext dbContext)
        {
            var sectionId = ConfigurationSections.Key("DataSignature");
            var configurationSection = dbContext.ConfigurationSections
                .Where(s => s.ConfigurationSectionId == sectionId)
                .FirstOrDefault();
            var sectionData = configurationSection?.Data;
            if (string.IsNullOrEmpty(sectionData))
            {
                using var hmac = new HMACSHA256();
                sectionData = Convert.ToBase64String(dataProtector.Protect(hmac.Key));
                configurationSection = new ConfigurationSection
                {
                    ConfigurationSectionId = sectionId,
                    Data = sectionData
                };
                dbContext.Add(configurationSection);
                dbContext.SaveChanges();
            }
            return Convert.FromBase64String(sectionData);
        }

        private void SetSignatureForObject(object data, string signature)
        {
            switch (data)
            {
                case DashboardAction action: action.DataSignature = signature; break;
                case DashboardActionTarget target: target.DataSignature = signature; break;
                case DashboardDevice device: device.DataSignature = signature; break;
                default: throw new NotImplementedException($"Unhandled type: {data.GetType().Name}");
            }
        }
    }
}
