public class MedAlertConfig
{
    public DateConfig Date { get; set; }
    public DescriptionsConfig Descriptions { get; set; }
    public string IntegrationCategory { get; set; }
    public string IntegrationType { get; set; }
    public bool IsActive { get; set; }
    public List<OutputConfig> Output { get; set; }
    public List<string> KeyFeatures { get; set; }
    public Dictionary<string, PermissionConfig> Permissions { get; set; }
    public List<SettingConfig> Settings { get; set; }
    public string TickUrl { get; set; }
}

public class DateConfig { public string CreatedAt { get; set; } public string UpdatedAt { get; set; } }
public class DescriptionsConfig { public string AppDescription { get; set; } public string AppLogo { get; set; } public string AppName { get; set; } public string AppUrl { get; set; } public string BackgroundColor { get; set; } }
public class OutputConfig { public string Label { get; set; } public bool Value { get; set; } }
public class PermissionConfig { public bool AlwaysOnline { get; set; } public string DisplayName { get; set; } }
public class SettingConfig { public string Label { get; set; } public string Type { get; set; } public bool Required { get; set; } public string Default { get; set; } }
