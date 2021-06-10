using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Process.Domain.Enums;
using System.Process.Domain.ValueObjects;
using System.Phoenix.DataAccess.Redis;
using gfoidl.Base64;
using System.Process.Application.DataTransferObjects;
using Microsoft.AspNetCore.Routing;

namespace System.Process.Application.AuthorizationHandlers
{
    public class UserDataAuthorizationHandler : AuthorizationHandler<UserDataRequirement>
    {
        private static readonly string Sub = "sub";
        private static readonly string SystemId = "SystemId";
        private static readonly string CustomerId = "CustomerId";
        private static readonly string CardId = "CardId";
        private static readonly string ApplicationId = "applicationId";

        private static IDictionary<ValidationProcess, Func<UserIdentity, AuthorizationHandlerDto, bool>> Validations { get; set; } =
            new Dictionary<ValidationProcess, Func<UserIdentity, AuthorizationHandlerDto, bool>>()
            {
                { ValidationProcess.ValidateConsultProcess, (userIdentity, body) => ValidateApplicationIdByRoute(userIdentity, body) },
                { ValidationProcess.ValidateSearchCreditCards, (userIdentity, body) => ValidateSystemIdByRoute(userIdentity, body) },
                { ValidationProcess.ValidateCreditCardRequest, (userIdentity, body) => ValidateSystemIdByBody(userIdentity, body) },
                { ValidationProcess.ValidateCreditCardAgreement, (userIdentity, body) => ValidateSystemIdByBody(userIdentity, body) },
                { ValidationProcess.ValidateCreditCardActivation, (userIdentity, body) => ValidateSystemIdByBody(userIdentity, body) },
                { ValidationProcess.ValidateRemoteDeposit, (userIdentity, body) => ValidateSystemIdByBody(userIdentity, body) },
                { ValidationProcess.ValidateStatementTypes, (userIdentity, body) => ValidateSystemIdByRoute(userIdentity, body) },
                { ValidationProcess.ValidateRetrieveStatement, (userIdentity, body) => ValidateApplicationIdByBody(userIdentity, body) },
                { ValidationProcess.ValidateTransfer, (userIdentity, body) => ValidateTransfer(userIdentity, body) },
                { ValidationProcess.ValidateCardOwner, (userIdentity, body) => ValidateCardOwner(userIdentity, body) }

            };

        private IHttpContextAccessor HttpContextAccessor { get; set; }
        private ILogger<UserDataAuthorizationHandler> Logger { get; set; }
        private IRedisService RedisService { get; set; }
        private ProcessConfig Config { get; set; }

        public UserDataAuthorizationHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserDataAuthorizationHandler> logger,
            IRedisService redisService,
            IOptions<ProcessConfig> config)
        {
            HttpContextAccessor = httpContextAccessor;
            Logger = logger;
            RedisService = redisService;
            Config = config.Value;
        }

        private static IDictionary<string, object> DecodeJwt(string authorizationHeader)
        {
            var encodeJWT = authorizationHeader.Replace("Bearer ", "");

            var encodedPayload = encodeJWT.Split(".")[1];

            encodedPayload = encodedPayload.Replace('+', '-').Replace('/', '_').TrimEnd('=');

            var payload = Encoding.UTF8.GetString(Base64.Url.Decode(encodedPayload));

            return JsonConvert.DeserializeObject<IDictionary<string, object>>(payload);
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserDataRequirement requirement)
        {
            if (HttpContextAccessor.HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                if (Config.ShouldDisableJwtValidation)
                {
                    Logger.LogWarning($"The JWT user identification validation is disabled.");
                }
                else
                {
                    var authorizationHeader = HttpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();

                    var routeData = HttpContextAccessor.HttpContext.GetRouteData();

                    var queryData = HttpContextAccessor.HttpContext.Request.QueryString;

                    var jwt = DecodeJwt(authorizationHeader);

                    if (jwt != null && jwt.ContainsKey(Sub))
                    {
                        var securityId = jwt[Sub].ToString();

                        Logger.LogInformation($"Starting transfer request validation for securityId={securityId}");

                        var userIdentity = RedisService.GetCache<UserIdentity>(securityId);

                        if (userIdentity == null)
                        {
                            Logger.LogError($"User session not found");
                        }
                        else
                        {
                            Logger.LogInformation($"UserIdentity session: {JsonConvert.SerializeObject(userIdentity)}");

                            HttpContextAccessor.HttpContext.Request.EnableBuffering();

                            string body = string.Empty;

                            using (var reader = new StreamReader(
                                HttpContextAccessor.HttpContext.Request.Body,
                                encoding: Encoding.UTF8,
                                detectEncodingFromByteOrderMarks: false,
                                bufferSize: 1024,
                                leaveOpen: true))
                            {
                                body = await reader.ReadToEndAsync();
                                HttpContextAccessor.HttpContext.Request.Body.Position = 0;
                            }

                            var data = new AuthorizationHandlerDto() { Body = body, Route = routeData, Query = queryData };

                            var validation = Validations[requirement.ValidationProcess].Invoke(userIdentity, data);

                            if (validation)
                            {
                                context.Succeed(requirement);
                            }
                            else
                            {
                                Logger.LogError($"Fail during {requirement.ValidationProcess} operation");
                            }
                        }
                    }
                }
            }
        }

        private static bool ValidateTransfer(UserIdentity userIdentity, AuthorizationHandlerDto body)
        {
            JToken SystemId, customerId;

            JObject.Parse(body.Body).TryGetValue(SystemId, StringComparison.InvariantCultureIgnoreCase, out SystemId);
            JObject.Parse(body.Body).TryGetValue(CustomerId, StringComparison.InvariantCultureIgnoreCase, out customerId);

            return userIdentity.SystemId == SystemId.Value<string>() &&
                userIdentity.ComplementaryIds[CustomerId] == customerId.Value<string>();
        }

        private static bool ValidateCardOwner(UserIdentity userIdentity, AuthorizationHandlerDto body)
        {
            JToken cardId, SystemId, customerId;

            JObject.Parse(body.Body).TryGetValue(CardId, StringComparison.InvariantCultureIgnoreCase, out cardId);
            JObject.Parse(body.Body).TryGetValue(SystemId, StringComparison.InvariantCultureIgnoreCase, out SystemId);
            JObject.Parse(body.Body).TryGetValue(CustomerId, StringComparison.InvariantCultureIgnoreCase, out customerId);

            //TODO: Precisa buscar na base se o SystemId/CustomerId tem acesso a esse cardid
            return false;
        }

        private static bool ValidateSystemIdByRoute(UserIdentity userIdentity, AuthorizationHandlerDto body)
        {
            var SystemId = body.Route.Values["SystemId"];

            return userIdentity.SystemId == SystemId.ToString();
        }

        private static bool ValidateSystemIdByBody(UserIdentity userIdentity, AuthorizationHandlerDto body)
        {
            JToken SystemId;

            JObject.Parse(body.Body).TryGetValue(SystemId, StringComparison.InvariantCultureIgnoreCase, out SystemId);

            return userIdentity.SystemId == SystemId.Value<string>();
        }

        private static bool ValidateApplicationIdByRoute(UserIdentity userIdentity, AuthorizationHandlerDto body)
        {
            var applicationId = body.Route.Values["ApplicationId"];

            return userIdentity.ApplicationId == applicationId.ToString();
        }

        private static bool ValidateApplicationIdByBody(UserIdentity userIdentity, AuthorizationHandlerDto body)
        {
            JToken applicationId;

            JObject.Parse(body.Body).TryGetValue(ApplicationId, StringComparison.InvariantCultureIgnoreCase, out applicationId);

            return userIdentity.ApplicationId == applicationId.Value<string>();
        }
    }
}
