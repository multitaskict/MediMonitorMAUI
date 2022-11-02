using MediMonitor.Service.Models;

using System.Collections;
using System.ComponentModel;

namespace MediMonitor.ViewModels;

public class SurveyViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private DateTime _dateTime;
    private int? _score;

    public SurveyViewModel()
    {
        DateTime = DateTime.Now;
    }

    public SurveyViewModel(Survey survey)
    {
        DateTime = survey.DateTime;
        Score = survey.Score;
        Id = survey.Id;
        UserId = survey.UserId;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;


    private void InvokePropertyChanged(string property)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    public IEnumerable GetErrors(string propertyName)
    {
        var errors = new List<string>();

        if (propertyName != nameof(Score))
            return errors;

        if(Score < 1 || Score > 7 || (Id > 0 && Score == null))
        {
            errors.Add("Score must be between 1 and 7");
        }

        return errors;
    }

    public DateTime DateTime
    {
        get => _dateTime;
        set
        {
            _dateTime = value;

            InvokePropertyChanged(nameof(DateTime));
        }
    }

    public int? Score
    {
        get => _score;
        set
        {
            _score = value;

            if (_score < 1 || _score > 7)
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Score)));

            InvokePropertyChanged(nameof(Score));
        }
    }

    public bool HasErrors => false;

    public int Id { get; }

    public int UserId { get; }
}
