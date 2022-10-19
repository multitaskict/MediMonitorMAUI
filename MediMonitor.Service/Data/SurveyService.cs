using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediMonitor.Service.Models;
using System.Linq.Expressions;

namespace MediMonitor.Service.Data
{
    public class SurveyService
	{
        private readonly AppData appData;
        private readonly User user;

        /// <summary>
        /// Create a survey service
        /// </summary>
        /// <param name="appData">The Database connection class.</param>
        /// <param name="user">The current user.</param>
        public SurveyService(AppData appData, User user)
		{
            this.appData = appData;
            this.user = user;
        }

        /// <summary>
        /// Get the Surveys for the current <see cref="User"/>.
        /// </summary>
        /// <param name="expression">Optional expression to filter on.</param>
        /// <returns>A list of <see cref="Survey"/>.</returns>
        public async Task<List<Survey>> GetSurveysAsync(Expression<Func<Survey, bool>> expression = null)
        {
            var surveyQuery = appData.TableQuery<Survey>().Where(s => s.UserId == user.Id);

            if(expression != null)
            {
                surveyQuery = surveyQuery.Where(expression);
            }

            return await surveyQuery.ToListAsync();
        }

        public async Task<Survey> GetSurveyForDateAsync(DateTime date)
        {
            var survey = await appData.TableQuery<Survey>()
                                      .Where(s => s.UserId == user.Id && s.DateTime == date)
                                      .FirstOrDefaultAsync();

            if(survey == null)
            {
                survey = new Survey
                {
                    DateTime = date,
                    ModifiedDateTime = null,
                    PatientId = user.PatientId,
                    UserId = user.Id,
                    Score = null
                };
            }

            return survey;
        }

        /// <summary>
        /// Save a <see cref="Survey"/>.
        /// </summary>
        /// <param name="survey">The survey you want to save.</param>
        /// <returns>The amount of rows changed.</returns>
        public async Task<int> SaveAsync(Survey survey)
        {
            if (survey.Id > 0)
                survey.ModifiedDateTime = DateTime.Now;

            return await appData.SaveAsync(survey);
        }

        /// <summary>
        /// Save the sync of the surveys
        /// </summary>
        /// <param name="surveys"></param>
        /// <param name="syncDateTime"></param>
        /// <returns></returns>
        public async Task<int> SaveSyncAsync(IEnumerable<Survey> surveys, DateTime syncDateTime)
        {
            var results = 0;

            foreach(var survey in surveys)
            {
                survey.SyncDateTime = syncDateTime;

                results  += await appData.SaveAsync(survey);
            }

            return results;
        }

        /// <summary>
        /// Get the last Survey that hasn't been synced.
        /// </summary>
        /// <returns></returns>
        public async Task<Survey> GetLastNotSynced()
        {
            return await appData.TableQuery<Survey>().Where(s => s.SyncDateTime == null ||
                                                           (s.ModifiedDateTime != null &&  s.ModifiedDateTime > s.SyncDateTime))
                                .FirstOrDefaultAsync();
        }

	}
}

