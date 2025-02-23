using System;
using System.Threading.Tasks;

public class MedAlertService
{
    private readonly WebhookService _webhookService;
    private string _reminderMessage = "Time for your medication!";
    private string[] _alertRecipients = { "Patient" };

    public MedAlertService(WebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    public async Task TriggerTick(MonitorPayload payload)
    {
        var reminderMessage = payload.Settings?.Find(s => s.Label == "Reminder Message")?.Default ?? "Time for your medication!";
        var alertRecipients = payload.Settings?.Find(s => s.Label == "Alert Recipients")?.Default ?? "Patient";

        Console.WriteLine($"[MedAlertService] Sending reminder: {reminderMessage} to {alertRecipients}");

        await _webhookService.SendWebhookNotification(
            "Medication Reminder",
            $"{reminderMessage} to {alertRecipients}"
        );
    }
}
