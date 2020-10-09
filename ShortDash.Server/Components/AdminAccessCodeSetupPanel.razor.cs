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

        protected EditContext AdminCodeEditContext { get; set; }
        protected string DynamicCode { get; set; }
        protected string DynamicCodeUrl => $"otpauth://totp/ShortDash?secret={DynamicCode}&issuer=Shortdash";
        protected bool IsStaticSelected { get; set; }
        protected AdminCodeModel Model { get; set; }
        protected bool PairedSuccessfully { get; set; }
        protected string StaticCode { get; set; }
        protected string TabDynamicClass => IsStaticSelected ? "" : "active";
        protected string TabStaticClass => IsStaticSelected ? "active" : "";

        [Inject]
        private AdminAccessCodeService AdminAccessCodeService { get; set; }

        protected override Task OnParametersSetAsync()
        {
            Model = new AdminCodeModel();
            AdminCodeEditContext = new EditContext(Model);
            DynamicCode = GenerateDynamicCode();
            StaticCode = GenerateStaticCode();

            return base.OnParametersSetAsync();
        }

        protected async void PairDevice()
        {
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

            if (compareCode.Equals(Model.UserCode))
            {
                await AdminAccessCodeService.SaveAccessCode(accessCodeType, adminCode);
                PairedSuccessfully = true;
                OnCompleted?.Invoke(this, new EventArgs());
            }
        }

        protected void TabDynamicClick()
        {
            IsStaticSelected = false;
        }

        protected void TabStaticClick()
        {
            IsStaticSelected = true;
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

        protected class AdminCodeModel
        {
            [Required]
            public string UserCode { get; set; }
        }
    }
}
