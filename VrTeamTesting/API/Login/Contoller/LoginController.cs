using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VrTeamTesting.API.Login.Model;
using VrTeamTesting.API.Login.Service;
using VrTeamTesting.API.Login.Validator;
using VrTeamTesting.Core;
using VrTeamTesting.Extensions;

namespace VrTeamTesting.API.Login.Contoller
{
    public class LoginController : Base.Controller
    {
        public LoginController(Bootstrap _) : base(_)
        { }

        [FunctionName("Auth_Login")]
        public async Task<IActionResult> Login(
       [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequest req, ILogger log)
        {

            return await Function.Execute(req, async () =>
            {
                var request = await req.Parse<LoginRequest, LoginValidator>();

                var service = new LoginService();

                var requestIP = req.HttpContext.Connection.RemoteIpAddress.ToString();

                var response = await service.Login(request.Value, requestIP);
                return new APIResult(HttpStatusCode.OK, response);
            });

        }
        [FunctionName("Auth_Status")]
        public async Task<IActionResult> Status(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/status")] HttpRequest req, ILogger log)
        {
            {
                return await Function.Execute(req, async () =>
                {
                    var service = new LoginService();
                    var token = req.Headers.ContainsKey("token") ?
                        req.Headers["token"] :
                        throw new Exceptions.UnauthorizedException("Session Expired");

                    var response = await service.Status(token);

                    return new APIResult(HttpStatusCode.OK, response);
                });
            }
        }
    }
}
