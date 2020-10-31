using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OtpNet;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ShortDash.Server.Components
{
    public partial class AdminAccessCodeSetupPanel : ComponentBase
    {
        [Parameter]
        public EventHandler OnCompleted { get; set; }

        [Inject]
        private AdminAccessCodeService AdminAccessCodeService { get; set; }

        private string AdminCode { get; set; }

        private string DynamicCodeUrl => $"otpauth://totp/ShortDash:{Environment.MachineName}?secret={AdminCode}&issuer=ShortDash";

        private DynamicCodeModel DynamicModel { get; set; }

        private AdminCodeModel Model { get; set; }

        private bool ShowRetryMessage { get; set; }
        private bool VerifiedDynamicCode { get; set; }
        private string VerifiedDynamicCodeButtonClass => VerifiedDynamicCode ? (WasValidDynamicCode ? "btn-success" : "btn-danger") : "btn-outline-secondary";
        private bool WasValidDynamicCode { get; set; }

        protected override Task OnParametersSetAsync()
        {
            Model = new AdminCodeModel();
            DynamicModel = new DynamicCodeModel();
            AdminCode = GenerateAdminCode();
            ShowRetryMessage = false;
            VerifiedDynamicCode = false;
            WasValidDynamicCode = false;

            return base.OnParametersSetAsync();
        }

        private void ChangedDynamicUserCode()
        {
            VerifiedDynamicCode = false;
            WasValidDynamicCode = false;
        }

        private string GenerateAdminCode()
        {
            var code = KeyGeneration.GenerateRandomKey(10);
            return Base32Encoding.ToString(code);
        }

        private void PairDevice()
        {
            ShowRetryMessage = false;

            var compareCode = AdminCode;
            var userCode = Model.UserCode.Replace(" ", "");
            if (!compareCode.Equals(userCode))
            {
                ShowRetryMessage = true;
                return;
            }

            AdminAccessCodeService.SaveAccessCode(AdminCode);
            OnCompleted?.Invoke(this, new EventArgs());
        }

        private void VerifyDynamicCode()
        {
            if (WasValidDynamicCode || string.IsNullOrWhiteSpace(DynamicModel.UserCode))
            {
                return;
            }
            var base32Bytes = Base32Encoding.ToBytes(AdminCode);
            var otp = new Totp(base32Bytes);
            var compareCode = otp.ComputeTotp();
            var userCode = DynamicModel.UserCode.Replace(" ", "");
            VerifiedDynamicCode = true;
            WasValidDynamicCode = compareCode.Equals(userCode);
        }

        private class AdminCodeModel
        {
            [Required]
            [Display(Name = "Full Access Code")]
            public string UserCode { get; set; }
        }

        private class DynamicCodeModel
        {
            [Display(Name = "Generated Code")]
            public string UserCode { get; set; }
        }
    }
}
