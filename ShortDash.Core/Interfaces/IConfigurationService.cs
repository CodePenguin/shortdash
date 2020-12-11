namespace ShortDash.Core.Interfaces
{
    public interface IConfigurationService
    {
        public string GetSection(string sectionId);

        public T GetSection<T>(string sectionId) where T : new();

        public string GetSecureSection(string sectionId);

        public T GetSecureSection<T>(string sectionId) where T : new();

        public void RemoveSection(string sectionId);

        public void SetSection(string sectionId, string sectionData);

        public void SetSection(string sectionId, object data);

        public void SetSecureSection(string sectionId, string sectionData);

        public void SetSecureSection(string sectionId, object data);

        public void SetSecureSectionAsync(string sectionId, string sectionData);
    }
}
