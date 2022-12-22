using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MediMonitor.Service.ApiModels;
using MediMonitor.Service.Data;
using MediMonitor.Service.Exceptions;
using MediMonitor.Service.Models;
using Newtonsoft.Json;

namespace MediMonitor.Service.Web
{
    public class Connection
    {

        private const string ASPXAUTH_COOKIE = ".ASPXAUTH";
        private const string ASP_SESSION = "ASP.NET_SessionId";

        //Content type
        private const string ContentType = "application/x-www-form-urlencoded";
        public const string JsonContentType = "application/json; charset=utf-8";

        public const string LiveUrl = "https://medicijnverstrekking.regenboogapotheek.nl";
        public const string TestUrl = "https://testmedicatie.multitaskict.nl";
        public const string LocalUrl = "http://localhost:{0}";

        public List<JSONModel> JSONModels { get; } = new List<JSONModel>();

        public string AppVersion { get; private set; }
        public string Application { get; private set; }
        public string Platform { get; private set; }

        private const string SurveyController = "Survey";

        /// <summary>
        /// Connect class constructor
        /// </summary>
        /// <param name="appVersion">Version of the mobile app.</param>
        /// <param name="application">Name of the mobile app</param>
        /// <param name="platform">Current platform (iOS, Android, Windows)</param>
        public Connection(string appVersion, string application, string platform)
        {
            AppVersion = appVersion;
            Application = application;
            Platform = platform;
        }

        /// <summary>
        /// Call this method to initiate a connection
        /// </summary>
        /// <param name="url">The url to connect to</param>
        /// <param name="timeOut">The time out to wait for the server to respond.  Default = 3000 (30 sec)</param>
        /// <returns>The initiated HttpWebRequest</returns>
        private HttpWebRequest CreateRequest(string url, int timeOut = 30000)
        {
            var request = WebRequest.CreateHttp(url);
            request.Timeout = timeOut;
           
            request.UserAgent = $"Multitask ICT MediMonitor (APP; {AppVersion}; {Application}; {Platform})";
            return request;
        }


        /// <summary>
        /// Call this method to initiate a connection
        /// </summary>
        /// <param name="medicijnVerstrekking">The MedicijnVerstrekking you want to connect to</param>
        /// <param name="url">The Relative Url you want to connect to</param>
        /// <returns>The initiated HttpWebRequest</returns>
        private HttpWebRequest CreateRequest(MedicijnVerstrekking medicijnVerstrekking, string url)
        {
            //Detect if a trailing backslash is added
            if (medicijnVerstrekking.Url.EndsWith("/"))
                medicijnVerstrekking.Url = medicijnVerstrekking.Url.Substring(0, medicijnVerstrekking.Url.Length - 1);

            //Create the webreqest
            var request = CreateRequest(string.Concat(medicijnVerstrekking.Url, url));

            //Prepair capturing cookies
            request.CookieContainer = new CookieContainer();

            //If any cookies are stored in the CombinatiePlanner...
            if (medicijnVerstrekking.Cookies?.Count > 0)
            {
                //Pass them to the request
                request.CookieContainer.Add(medicijnVerstrekking.UriUrl, medicijnVerstrekking.Cookies);
            }

            return request;
        }
        /// <summary>
        /// Call this method to asynchronous initiate a POST connection
        /// </summary>
        /// <param name="medicijnVerstrekking">The Medicijnverstrekking you want to connect to</param>
        /// <param name="url">The Relative Url you want to connect to</param>
        /// <param name="data">The data you want to post</param>
        /// <returns>The initiated HttpWebRequest as a Task</returns>
        private async Task<HttpWebRequest> CreatePostRequestAsync(MedicijnVerstrekking medicijnVerstrekking, string url, string data, string contentType = ContentType)
        {
            //Create the webrequest
            var request = CreateRequest(medicijnVerstrekking, url);

            //Set the request to a POST method
            request.Method = "POST";
            request.ContentType = contentType;

            //Prepare the data to be sent to the server
            var encoding = new UTF8Encoding();
            var postBytes = encoding.GetBytes(data);

            //Set the ContentLength of the Request to the amount of data sent (in bytes)
            request.ContentLength = postBytes.LongLength;

            using (var stream = await request.GetRequestStreamAsync()) //Await the RequestStream
            {
                await stream.WriteAsync(postBytes, 0, postBytes.Length); //Asynchronous write the data
            }

            return request;
        }

        /// <summary>
        /// Prepare the URL you want to call.
        /// </summary>
        /// <param name="medicijnVerstrekking">The Medicijnverstrekking you want to connect to</param>
        /// <param name="controller">The name of the controller</param>
        /// <param name="action">The name of the action</param>
        /// <param name="urlParams">The optional parameters you want to pass.</param>
        /// <returns>The url</returns>
        public string PrepareUrl(MedicijnVerstrekking medicijnVerstrekking, string controller, string action, IDictionary<string, string> urlParams = null)
        {
            var paramsUrlPart = urlParams != null ? "?" + string.Join("&", urlParams.Select(x => $"{x.Key}={x.Value}")) : "";

            if (medicijnVerstrekking.Url.EndsWith("/"))
                medicijnVerstrekking.Url = medicijnVerstrekking.Url.Substring(0, medicijnVerstrekking.Url.Length - 1);

            var url = string.Join("/", medicijnVerstrekking.Url, controller, action);

            return url + paramsUrlPart;
        }

        /// <summary>
        /// Prepare the URL you want to call.
        /// </summary>
        /// <param name="baseUrl">The Url of the Medicijnverstrekking you want to connect to</param>
        /// <param name="controller">The name of the controller</param>
        /// <param name="action">The name of the action</param>
        /// <param name="urlParams">The optional parameters you want to pass.</param>
        /// <returns>The url</returns>
        public string PrepareUrl(string baseUrl, string controller, string action, IDictionary<string, string> urlParams = null)
        {
            var paramsUrlPart = urlParams != null ? "?" + string.Join("&", urlParams.Select(x => $"{x.Key}={x.Value}")) : "";

            var url = string.Join("/", baseUrl, controller, action) + paramsUrlPart;

            return url;
        }

        /// <summary>
        /// Prepare the URL you want to call.
        /// </summary>
        /// <param name="controller">The name of the controller</param>
        /// <param name="action">The name of the action</param>
        /// <param name="urlParams">The optional parameters you want to pass.</param>
        /// <returns>The url</returns>
        public string PrepareUrl(string controller, string action, IDictionary<string, string> urlParams = null)
        {
            var paramsUrlPart = urlParams != null ? "?" + string.Join("&", urlParams.Select(x => $"{x.Key}={x.Value}")) : "";

            var url = "/" + string.Join("/", controller, action) + paramsUrlPart;

            return url;
        }

        /// <summary>
        /// Get the Context of the Medicijnverstrekking webapplication
        /// </summary>
        /// <param name="mvUrl">The Url to the application</param>
        /// <returns>The context</returns>
        public async Task<MedicijnVerstrekking> GetContextAsync(string mvUrl)
        {
            var url = PrepareUrl(mvUrl, "Home", "GetContext");

            var request = CreateRequest(url, 5000);

            var result = await GetResponseJsonAsync<ResultViewModel<MedicijnVerstrekkingContextModel>>(request);

            if (result.Success)
            {
                var contextModel = result.Data[0];

                var mv = MedicijnVerstrekking.Instance;

                mv.AppMode = contextModel.AppMode;
                mv.Url = mvUrl;
                mv.GebruikerId = contextModel.GebruikerId;
                mv.Version = new WebAppVersion(new WebAppContext { Version = contextModel.Version, Branche = contextModel.Branche });

                return mv;
            }
            else
            {
                throw new Exception(result.Error);
            }
        }

        /// <summary>
        /// Sign in to the WebApp.
        /// </summary>
        /// <param name="medicijnVerstrekking"></param>
        /// <param name="qrCode"></param>
        /// <returns></returns>
        public async Task<User> SignInAsync(MedicijnVerstrekking medicijnVerstrekking, Action<CookieCollection> addCookies, QrCode qrCode, AppData appData)
        {
            //var url = 

            var request = await CreatePostRequestAsync(medicijnVerstrekking, PrepareUrl(SurveyController, "SignIn"), "qrcode=" + qrCode.Code, ContentType);

            var response = await GetResponseJsonAsync<ResultViewModel<User>>(request);

            if(response.Success)
            {
                addCookies(request.CookieContainer.GetCookies(medicijnVerstrekking.UriUrl));

                var userService = new UserService(appData);
                var user = await userService.GetByPatientIdAsync(response.Data[0].PatientId) ??
                    new User {  FirstName = response.Data[0].FirstName, LastName = response.Data[0].LastName, PatientId = response.Data[0].PatientId };

                if (user.Id < 1)
                    await appData.SaveAsync(user);

                return user;
            }
            else
            {
                throw new Exception(response.Error);
            } 
        }

        /// <summary>
        /// Save a Survey
        /// </summary>
        /// <param name="medicijnVerstrekking">The Medicijnverstrekking instance to save to.</param>
        /// <param name="survey"></param>
        /// <returns></returns>
        public async Task<ResultViewModel<Survey>> SendSurveyAsync(MedicijnVerstrekking medicijnVerstrekking, Survey survey)
        {
            var surveyData = GetJson(survey);

            var request = await CreatePostRequestAsync(medicijnVerstrekking, PrepareUrl(SurveyController, "Save"), surveyData, JsonContentType);

            var response = await GetResponseJsonAsync<ResultViewModel<Survey>>(request);

            if (response.Error == "NoSession")
                throw new NoSessionException(response);

            return response;
        }

        public async Task<IEnumerable<Survey>> SyncSurveysAsync(MedicijnVerstrekking medicijnVerstrekking, AppData appData, User user, IEnumerable<Survey> surveys, DateTime? lastSync) 
        {
            var model = new SyncModel
            {
                PatientId = user.PatientId,
                Surveys = surveys,
                LastSync = lastSync
            };

            var postRequest = await CreatePostRequestAsync(medicijnVerstrekking, PrepareUrl(SurveyController, "Sync"), GetJson(model), JsonContentType);
            var response = await GetResponseJsonAsync<ResultViewModel<Survey>>(postRequest);

            var sync = new Sync { Result = GetJson(response), Success = response.Success, SyncDateTime = DateTime.Now, UserId = user.Id };
            await appData.SaveAsync(sync);

            if(response.Success)
            {
                var surveyList = new List<Survey>();

                foreach (var survey in response.Data)
                {
                    var surveyEntity = await appData.FirstOrDefaultAsync<Survey>(a => a.DateTime == survey.DateTime) ?? 
                                       new Survey { DateTime = survey.DateTime, PatientId = user.PatientId, UserId = user.Id };

                    if (surveyEntity.SyncDateTime == null || surveyEntity.SyncDateTime < survey.SyncDateTime)
                    {
                        surveyEntity.SyncDateTime = survey.SyncDateTime;
                        surveyEntity.Score = survey.Score;
                        surveyEntity.ModifiedDateTime = survey.ModifiedDateTime;

                        await appData.SaveAsync(surveyEntity);

                        surveyList.Add(surveyEntity);
                    }
                }
                
                return surveyList;
            }
            else
            {
                if (response.Error == "NoSession")
                    throw new NoSessionException(response);
                else
                    throw new Exception(response.Error);
            }
        }

        public async Task<bool> DeleteDataAsync(MedicijnVerstrekking medicijnVerstrekking, AppData appData, User user)
        {
            var model = new {
                user.PatientId
            };

            var postRequest = await CreatePostRequestAsync(medicijnVerstrekking, PrepareUrl(SurveyController, "DeleteData"), GetJson(model), JsonContentType);
            var response = await GetResponseJsonAsync<ResultViewModel<object>>(postRequest);

            if(!response.Success && response.Error == "NoSession")
            {
                throw new NoSessionException(response);
            }

            return response.Success;
        }

        public async Task<IEnumerable<Medicijn>> GetMedicijnAsync(MedicijnVerstrekking medicijnVerstrekking, AppData appData)
        {
            var getRequest = CreateRequest(medicijnVerstrekking, PrepareUrl("Survey", "GetMedicijnen"));
            var result = await GetResponseJsonAsync<ResultViewModel<AppMedicijn>>(getRequest);

            if (!result.Success && result.Error == "NoSession")
            {
                throw new NoSessionException(result);
            }
            else if (!result.Success)
            {
                throw new Exception(result.Error);
            }
            else
            {
                var list = new List<Medicijn>();
                foreach (var m in result.Data)
                {
                    var medicijn = await appData.TableQuery<Medicijn>().Where(mx => mx.ApiId == m.Id).FirstOrDefaultAsync() ??
                            new Medicijn
                            {
                                ApiId = m.Id
                            };


                    medicijn.EngelseNaam = m.EngelseNaam;
                    medicijn.Naam = m.Naam;

                    await appData.SaveAsync(medicijn);

                    list.Add(medicijn);
                }

                return list;
            }

        }

        public async Task<IEnumerable<Medicatie>> GetMedicatie(MedicijnVerstrekking medicijnVerstrekking, AppData appData, User user)
        {
            var getRequest = CreateRequest(medicijnVerstrekking, PrepareUrl("Survey", "GetMedicatie"));
            var result = await GetResponseJsonAsync<ResultViewModel<AppMedicatieModel>>(getRequest);
            var list = new List<Medicatie>();

            if (result.Success)
            {
                foreach (var item in result.Data)
                {
                    var medicatie =
                        await appData.TableQuery<Medicatie>().FirstOrDefaultAsync(m => m.ApiId == item.Id) ??
                        new Medicatie
                        {
                            ApiId = item.Id,
                            UserId = user.Id,
                            MedicijnId = item.MedicijnId,
                            AanvraagId = item.AanvraagId,
                            PatientId = item.PatientId
                        };

                    medicatie.AfbouwPeriode = item.AfbouwPeriode;
                    medicatie.MedicatieVerpakking = item.MedicatieVerpakking;

                    await appData.SaveAsync(medicatie);

                    var innamemomenten = await appData.TableQuery<Innamemoment>().Where(im => im.MedicatieId == medicatie.Id).ToArrayAsync();
                    foreach (var im in innamemomenten)
                    {
                        if (im.ApiId.HasValue && !item.Innamemomenten.Select(ix => ix.Id).Contains(im.ApiId.Value))
                        {
                            //Innamemoment has been removed.
                            await appData.DeleteAsync(im);
                        }
                    }

                    foreach (var im in item.Innamemomenten)
                    {
                        var innamemoment = await appData.TableQuery<Innamemoment>().FirstOrDefaultAsync(i => i.ApiId == im.Id) ??
                            new Innamemoment { ApiId = im.Id, MedicatieId = medicatie.Id, Type = im.Type };

                        innamemoment.Tijdstip = im.Tijdstip;
                        innamemoment.Notification = im.Tijdstip;
                        innamemoment.Opmerking = im.Opmerking;
                        innamemoment.InnameHoeveelheid = im.InnameHoeveelheid;

                        await appData.SaveAsync(innamemoment);
                    }

                    list.Add(medicatie);
                }

                var ids = list.Select(l => l.ApiId).ToArray();
                var toRemove = await appData.TableQuery<Medicatie>().Where(m => m.UserId == user.Id && m.ApiId != null && !ids.Contains(m.ApiId)).ToArrayAsync();

                foreach (var item in toRemove)
                {
                    await appData.DeleteAsync(item);
                }

                return list;
            }
            else
            {
                throw new Exception(result.Error);
            }

        }

        /// <summary>
        /// Get the response from the request.
        /// </summary>
        /// <typeparam name="T">The object you want to cast</typeparam>
        /// <param name="request">The request you want to convert</param>
        /// <returns></returns>
        private async Task<T> GetResponseJsonAsync<T>(HttpWebRequest request)
            where T : new()
        {
            using (var response = await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream())
            using (var responseStreamReader = new StreamReader(responseStream))
            {
                var responseText = await responseStreamReader.ReadToEndAsync();

                JSONModels.Add(new JSONModel {  Uri = request.RequestUri, Data = responseText });

                return GetFromJson<T>(responseText);
            }
        }

        /// <summary>
        /// Invoke an event.
        /// </summary>
        /// <param name="eventHandler">The event you want to invoke</param>
        private void Invoke(EventHandler eventHandler)
        {
            eventHandler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Serialise an object into JSON
        /// </summary>
        /// <param name="obj">The object you want serialised</param>
        /// <returns>The generated json.</returns>
        public static string GetJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });
        }

        /// <summary>
        /// Deserialise a JSON string into an object.
        /// </summary>
        /// <typeparam name="T">The type of target object</typeparam>
        /// <param name="json">The JSON you want to deserialise</param>
        /// <returns>The generated object</returns>
        public static T GetFromJson<T>(string json)
            where T : new()
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });
        }


    }
}
