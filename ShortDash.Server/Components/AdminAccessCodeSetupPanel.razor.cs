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

        private EditContext AdminCodeEditContext { get; set; }
        private string DynamicCode { get; set; }
        private string DynamicCodeUrl => $"otpauth://totp/ShortDash?secret={DynamicCode}&issuer=Shortdash";
        private bool IsStaticSelected { get; set; }
        private AdminCodeModel Model { get; set; }
        private bool ShowRetryMessage { get; set; }
        private string StaticCode { get; set; }
        private string TabDynamicClass => IsStaticSelected ? "" : "active";
        private string TabStaticClass => IsStaticSelected ? "active" : "";

        protected override Task OnParametersSetAsync()
        {
            Model = new AdminCodeModel();
            AdminCodeEditContext = new EditContext(Model);
            DynamicCode = GenerateDynamicCode();
            StaticCode = GenerateStaticCode();
            ShowRetryMessage = false;

            return base.OnParametersSetAsync();
        }

        protected void PairDevice()
        {
            ShowRetryMessage = false;
            if (!AdminCodeEditContext.Validate())
            {
                return;
            }

            string adminCode;
            string compareCode;
            AdminAccessCodeType accessCodeType;
            if (IsStaticSelected)
            {
                accessCodeType = AdminAccessCodeType.Static;
                adminCode = StaticCode;
                compareCode = StaticCode;
            }
            else
            {
                var base32Bytes = Base32Encoding.ToBytes(DynamicCode);
                var otp = new Totp(base32Bytes);

                accessCodeType = AdminAccessCodeType.DynamicTotp;
                adminCode = DynamicCode;
                compareCode = otp.ComputeTotp();
            }

            var userCode = Model.UserCode.Replace(" ", "");
            if (!compareCode.Equals(userCode))
            {
                ShowRetryMessage = true;
                return;
            }

            AdminAccessCodeService.SaveAccessCode(accessCodeType, adminCode);
            OnCompleted?.Invoke(this, new EventArgs());
        }

        private string GenerateDynamicCode()
        {
            var code = KeyGeneration.GenerateRandomKey(10);
            return Base32Encoding.ToString(code);
        }

        private string GenerateStaticCode()
        {
            const int AdminCodeLength = 9;
            var baseCode = Math.Abs(Guid.NewGuid().ToString().GetHashCode() % Math.Pow(10, AdminCodeLength));
            return baseCode.ToString().PadLeft(AdminCodeLength, '1');
        }

        private void TabDynamicClick()
        {
            IsStaticSelected = false;
        }

        private void TabStaticClick()
        {
            IsStaticSelected = true;
        }

        private class AdminCodeModel
        {
            [Required]
            public string UserCode { get; set; }
        }
    }
}
