﻿using System;
using System.Collections.Specialized;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using NLog;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetEmployerInformation;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetGatewayInformation;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetGatewayToken;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetHmrcEmployerInformation;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Models.HmrcLevy;
using SFA.DAS.EmployerApprenticeshipsService.Web.Models;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.Orchestrators
{
    public abstract class EmployerVerificationOrchestratorBase
    {

        protected readonly IMediator Mediator;
        protected readonly ILogger Logger;
        protected readonly ICookieService CookieService;
        protected readonly EmployerApprenticeshipsServiceConfiguration Configuration;

        //Needed for tests
        protected EmployerVerificationOrchestratorBase()
        {

        }

        protected EmployerVerificationOrchestratorBase(IMediator mediator, ILogger logger, ICookieService cookieService, EmployerApprenticeshipsServiceConfiguration configuration)
        {
            Mediator = mediator;
            Logger = logger;
            CookieService = cookieService;
            Configuration = configuration;
        }


        public virtual async Task<string> GetGatewayUrl(string redirectUrl)
        {
            var response = await Mediator.SendAsync(new GetGatewayInformationQuery
            {
                ReturnUrl = redirectUrl
            });

            return response.Url;
        }

        public async Task<OrchestratorResponse<HmrcTokenResponse>> GetGatewayTokenResponse(string accessCode, string returnUrl, NameValueCollection nameValueCollection)
        {
            var errorResponse = nameValueCollection?["error"];
            if (errorResponse != null)
            {
                if (nameValueCollection["error_Code"] == "USER_DENIED_AUTHORIZATION" )
                {
                    return new OrchestratorResponse<HmrcTokenResponse>
                    {
                        Status = HttpStatusCode.NotAcceptable,
                        FlashMessage = new FlashMessageViewModel
                        {
                            Severity = FlashMessageSeverityLevel.Error,
                            Headline = "Account not added",
                            Message = "You need to grant authority to HMRC to add an account.",
                            RedirectButtonMessage = "Add new account",
                            RedirectButtonClass = "add_new_account"
                        }
                    };
                }
                return new OrchestratorResponse<HmrcTokenResponse>
                {
                    Status = HttpStatusCode.NotAcceptable,
                    FlashMessage = new FlashMessageViewModel
                    {
                        Severity = FlashMessageSeverityLevel.Danger,
                        Message = "Unexpected response from HMRC Government Gateway:",
                        SubMessage = nameValueCollection["error_description"]
                    }
                };
            }

            var response = await Mediator.SendAsync(new GetGatewayTokenQuery
            {
                RedirectUrl = returnUrl,
                AccessCode = accessCode
            });

            return new OrchestratorResponse<HmrcTokenResponse> { Data = response.HmrcTokenResponse };
        }


        public async Task<GetHmrcEmployerInformationResponse> GetHmrcEmployerInformation(string authToken, string email)
        {
            var response = new GetHmrcEmployerInformationResponse();
            try
            {
                response = await Mediator.SendAsync(new GetHmrcEmployerInformationQuery
                {
                    AuthToken = authToken
                });
            }
            catch (ConstraintException)
            {
                response.Empref = "";
            }
            
            return response;
        }

        public virtual async Task<OrchestratorResponse<SelectEmployerViewModel>> GetCompanyDetails(SelectEmployerModel model)
        {
            var response = await Mediator.SendAsync(new GetEmployerInformationRequest
            {
                Id = model.EmployerRef
            });

            if (response == null)
            {
                Logger.Warn("No response from SelectEmployerViewModel");
                return new OrchestratorResponse<SelectEmployerViewModel>
                {
                    Status = HttpStatusCode.BadRequest,
                    Data = new SelectEmployerViewModel()
                };
            }

            Logger.Info($"Returning Data for {model.EmployerRef}");

            return new OrchestratorResponse<SelectEmployerViewModel>
            {
                Data = new SelectEmployerViewModel
                {
                    CompanyNumber = response.CompanyNumber,
                    CompanyName = response.CompanyName,
                    DateOfIncorporation = response.DateOfIncorporation,
                    RegisteredAddress = $"{response.AddressLine1}, {response.AddressLine2}, {response.AddressPostcode}"
                }

            };
        }
    }
}