namespace MedAlert.model
{
    public class IntegrationSpec
    {
        public IntegrationData Data { get; set; }
    }

    public class IntegrationData
    {
        public DateInfo Date { get; set; }
        public IntegrationDescriptions Descriptions { get; set; }
        public string IntegrationCategory { get; set; }
        public string IntegrationType { get; set; }
        public bool IsActive { get; set; }
        public List<IntegrationOutput> Output { get; set; }
        public List<string> KeyFeatures { get; set; }
        public IntegrationPermissions Permissions { get; set; }
        public List<IntegrationSetting> Settings { get; set; }
        public string TickUrl { get; set; }
        public string TargetUrl { get; set; }
    }

    public class DateInfo
    {
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }

    public class IntegrationDescriptions
    {
        public string AppDescription { get; set; }
        public string AppLogo { get; set; }
        public string AppName { get; set; }
        public string AppUrl { get; set; }
        public string BackgroundColor { get; set; }
    }

    public class IntegrationOutput
    {
        public string Label { get; set; }
        public bool Value { get; set; }
    }

    public class IntegrationPermissions
    {
        public ReminderUser ReminderUser { get; set; }
    }

    public class ReminderUser
    {
        public bool AlwaysOnline { get; set; }
        public string DisplayName { get; set; }
    }

    public class IntegrationSetting
    {
        public string Label { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public string Default { get; set; }
        public List<string> Options { get; set; } // Only for multi-checkbox settings
    }

}
