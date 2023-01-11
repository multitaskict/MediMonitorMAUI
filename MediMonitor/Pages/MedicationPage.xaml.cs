using MediMonitor.Resources;
using MediMonitor.Service.Data;
using MediMonitor.Service.Models;
using MediMonitor.Service.Web;

using Plugin.LocalNotification;

using System.ComponentModel;

namespace MediMonitor.Pages;

public partial class MedicationPage : ContentPage
{
    private readonly MedicationService medicationService;
    private readonly MedicineService medicineService;
    private readonly IntakeService intakeService;
    private readonly MedicijnVerstrekking medicijnVerstrekking;
    private readonly Connection connection;
    private readonly Dictionary<int, TimeSpan> notificationDictionary;

    public MedicationPage()
    {
        InitializeComponent();

        medicationService = new MedicationService(App.Database);
        medicineService = new MedicineService(App.Database);
        intakeService = new IntakeService(App.Database);

        medicijnVerstrekking = App.ApplicationContext.MedicijnVerstrekking;
        connection = App.ApplicationContext.Connection;

        notificationDictionary = new Dictionary<int, TimeSpan>();

        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await connection.GetMedicijnAsync(medicijnVerstrekking, App.Database);
            await connection.GetMedicatie(medicijnVerstrekking, App.Database, App.ApplicationContext.User);

            var medications = await medicationService.GetMedicatiesAsync();
            foreach (var m in medications)
                await ShowMedication(m);
        });

    }

    private async Task ShowMedication(Medicatie medication)
    {
        if (medication != null)
        {
            var row = new RowDefinition();
            gridMedications.RowDefinitions.Insert(gridMedications.RowDefinitions.Count - 1, row);

            var stack = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Center,
                Padding = new Thickness(15),
                ClassId = "medication-item"
            };

            var label = new Label { HorizontalOptions = LayoutOptions.Center };

            var medicineName = await medicineService.GetMedicineNameAsync(medication.MedicijnId);
            var text = $"{AppResources.Medicine}: {medicineName}";
            label.Text = text;

            stack.Children.Add(label);

            var intakes = await intakeService.GetIntakesAsync(medication.Id);
            foreach (var intake in intakes)
            {
                var labelIntake = new Label { HorizontalOptions = LayoutOptions.Center };

                if (intake.Type != Service.Models.Enums.InnamemomentType.Anders)
                {
                    var time = intake.Tijdstip.ToString(@"hh\:mm");
                    var days = medication.AfbouwPeriode;

                    var intakeText = days.HasValue ? AppResources.DailyIntakeDays : AppResources.DailyIntake;

                    labelIntake.Text = intakeText.Replace("%t", time).Replace("%d", days?.ToString());
                }
                else
                {
                    labelIntake.Text = intake.Opmerking;
                }
                stack.Children.Add(labelIntake);

                var meldingStack = new StackLayout { Orientation = StackOrientation.Horizontal, Padding = new Thickness(10) };
                meldingStack.Children.Add(new Label { Text = AppResources.Daily_Notification_At, Margin = new Thickness(5) });
                var timePicker = new TimePicker { Time = intake.Notification ?? intake.Tijdstip, ClassId = "intake_" + intake.Id, Format = AppResources.TimePickerFormat };

                meldingStack.Children.Add(timePicker);
                timePicker.PropertyChanged += TimePicker_PropertyChanged;

                stack.Children.Add(meldingStack);
            }

            gridMedications.Children.Add(stack);
            Grid.SetRow(stack, gridMedications.RowDefinitions.IndexOf(row));
        }
    }

    private void TimePicker_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time))
        {
            var timePicker = (TimePicker)sender;
            var intakeId = int.Parse(timePicker.ClassId.Replace("intake_", ""));

            if (!notificationDictionary.ContainsKey(intakeId))
            {
                notificationDictionary.Add(intakeId, timePicker.Time);
            }
            else
            {
                notificationDictionary[intakeId] = timePicker.Time;
            }
        }
    }

    private async void sendNotifications_Clicked(object sender, EventArgs e)
    {
        try
        {
            var medications = await medicationService.GetMedicatiesAsync();
            foreach (var m in medications)
            {
                var intakes = await intakeService.GetIntakesAsync(m.Id);

                foreach (var intake in intakes)
                {
                    var time = notificationDictionary.ContainsKey(intake.Id) ? notificationDictionary[intake.Id] : intake.Tijdstip;
                    await intakeService.UpdateNotification(intake, time);

                    if (!await ShowMedicationNotification(m, intake, time))
                    {
                        if (await DisplayAlert(AppResources.Medicationmessages, AppResources.Could_Not_Set_Reminder, AppResources.App_Settings, AppResources.Back))
                            AppInfo.ShowSettingsUI();

                        return;
                    }
                }

                var days = m.AfbouwPeriode;
                var medicineName = await medicineService.GetMedicineNameAsync(m.MedicijnId);
                var message = AppResources.Notifications_Enabled_For + " " + medicineName;
                if (days != null)
                {
                    message += " " + AppResources.Days.Replace("%d", days.ToString());
                }

                await DisplayAlert(AppResources.Medicationmessages, message, AppResources.OK);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppResources.Medicationmessages, ex.Message, AppResources.Cancel);
        }
    }

    private async Task<bool> ShowMedicationNotification(Medicatie medication, Innamemoment innamemoment, TimeSpan notificationTime)
    {
        var medicineName = await medicineService.GetMedicineNameAsync(medication.MedicijnId);
        var text = AppResources.Take_Medication_Notification + " " + medicineName;
        var title = AppResources.Medication_reminder;

        //If time is past today, start notification tomorrow.
        var notifyDateTime = DateTime.Now.TimeOfDay >= notificationTime ? DateTime.Today.AddDays(1).Add(notificationTime) : DateTime.Today.Add(notificationTime);
        var cancelDateTime = medication.AfbouwPeriode.HasValue ? (DateTime?)notifyDateTime.AddDays(medication.AfbouwPeriode.Value) : null;

        var notification = new NotificationRequest
        {
            Title = title,
            Description = text,
            ReturningData = $"intake_med_{medication.Id}_intake_{innamemoment.Id}",
            NotificationId = innamemoment.Id,
            CategoryType = NotificationCategoryType.Reminder,
            Group = "MediMonitor-Group",

            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyDateTime,
                RepeatType = NotificationRepeat.Daily,
                NotifyAutoCancelTime = cancelDateTime
            }
        };

        return await LocalNotificationCenter.Current.Show(notification);
    }

    private void clearNotifications_Clicked(object sender, EventArgs e)
    {
        try
        {
            LocalNotificationCenter.Current.ClearAll();
            LocalNotificationCenter.Current.CancelAll();

            DisplayAlert(AppResources.Medicationmessages, AppResources.Notifications_removed, AppResources.OK);
        }
        catch (Exception ex)
        {
            DisplayAlert(AppResources.Medicationmessages, ex.Message, AppResources.Cancel);
        }
    }
}