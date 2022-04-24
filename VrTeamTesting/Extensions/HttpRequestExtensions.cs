using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using VrTeamTesting.Common.Models;
using VrTeamTesting.Common.ValidateRequest;

namespace VrTeamTesting.Extensions
{
    public static class HttpRequestExtensions
    {
        public static async Task<ValidatableRequest<T>> Parse<T, V>(this HttpRequest request)
            where V : AbstractValidator<T>, new()
            where T : class, new()
        {
            var baseRequest = await request.Parse<T>();
            var validator = new V();
            var validationResult = await validator.ValidateAsync(baseRequest.Value);

            if (!validationResult.IsValid)
            {
                throw new Exceptions.BadRequestException(validationResult.Errors.FirstOrDefault().ErrorMessage);
            }

            return new ValidatableRequest<T>
            {
                Value = baseRequest.Value,
                Files = baseRequest.Files,
                IsValid = true
            };

        }

        public static async Task<BaseRequest<T>> Parse<T>(this HttpRequest request)
        {

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };

            //var settings = new JsonSerializerOptions
            //{
            //    IgnoreNullValues = true,
            //};

            //settings.Converters.Add(new GuidToModel<T>());

            request.EnableBuffering();

            try
            {
                if (request.HasFormContentType) //De-serializing form-data
                {
                    var formdata = await request.ReadFormAsync();
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    foreach (var attribute in formdata)
                    {
                        dictionary.Add(attribute.Key, attribute.Value[0]);
                    }

                    var json = JsonConvert.SerializeObject(dictionary);
                    //var json = JsonSerializer.Serialize(dictionary);

                    return new BaseRequest<T>
                    {
                        //Value = JsonSerializer.Deserialize<T>(json),
                        Value = JsonConvert.DeserializeObject<T>(json, settings),
                        Files = request.Form.Files.Select(file => new FileModel(file)).ToList(),
                    };

                }
                else //Deserializing JSON 
                {
                    if (request.Method.Equals("POST") || request.Method.Equals("PUT") || request.Method.Equals("PATCH"))
                    {
                        var body = await new System.IO.StreamReader(request.Body).ReadToEndAsync();
                        request.Body.Position = 0;
                        return new BaseRequest<T>
                        {
                            Value = JsonConvert.DeserializeObject<T>(body, settings),
                            //Value = JsonSerializer.Deserialize<T>(body, settings)
                        };
                    }
                    else
                    {
                        var dict = HttpUtility.ParseQueryString(request.QueryString.ToString());
                        object x = dict.Cast<string>().ToDictionary(k => k, v => dict[v]);
                        string json = JsonConvert.SerializeObject(x);

                        //string json = JsonSerializer.Serialize(x);

                        return new BaseRequest<T>
                        {
                            Value = JsonConvert.DeserializeObject<T>(json, settings),
                            //Value = JsonSerializer.Deserialize<T>(json, settings),
                            Files = request.Form.Files.Select(file => new FileModel(file)).ToList(),
                        };
                    }
                }
            }
            catch (JsonException exception)
            {
                throw new Exceptions.BadRequestException("Couldn't parse your request", exception);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}
