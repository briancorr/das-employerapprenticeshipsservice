﻿using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.EmployerApprenticeshipsService.Web.Authentication;
using SFA.DAS.EmployerApprenticeshipsService.Web.Models;
using SFA.DAS.EmployerApprenticeshipsService.Web.Orchestrators;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.Controllers
{
    [Authorize]
    public class EmployerAccountController : BaseController
    {
        
        private readonly IOwinWrapper _owinWrapper;
        private readonly EmployerAccountOrchestrator _employerAccountOrchestrator;
        
        public EmployerAccountController(IOwinWrapper owinWrapper, EmployerAccountOrchestrator employerAccountOrchestrator)
        {
            if (employerAccountOrchestrator == null)
                throw new ArgumentNullException(nameof(employerAccountOrchestrator));
            
            _owinWrapper = owinWrapper;
            _employerAccountOrchestrator = employerAccountOrchestrator;
        }

        // GET: EmployerAccount
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(bool accepted)
        {
            return RedirectToAction("GovernmentGatewayConfirm");
        }

        [HttpGet]
        public ActionResult GovernmentGatewayConfirm()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SelectEmployer()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SelectEmployer(SelectEmployerModel model)
        {
            var response = await _employerAccountOrchestrator.GetCompanyDetails(model);

            if (string.IsNullOrWhiteSpace(response.Data.CompanyNumber))
                return View(response);

            return RedirectToAction("VerifyEmployer", response.Data);
        }

        [HttpGet]
        public ActionResult VerifyEmployer(SelectEmployerViewModel model)
        {
            var data = _employerAccountOrchestrator.GetCookieData(HttpContext);

            data.CompanyNumber = model.CompanyNumber;
            data.CompanyName = model.CompanyName;
            data.DateOfIncorporation = model.DateOfIncorporation;
            data.RegisteredAddress = model.RegisteredAddress;
            
            _employerAccountOrchestrator.SetCookieData(HttpContext,data);
            
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Gateway()
        {
            return Redirect(await _employerAccountOrchestrator.GetGatewayUrl(Url.Action("GateWayResponse","EmployerAccount",null,Request.Url.Scheme)));
        }

        public async Task<ActionResult> GateWayResponse()
        {
            var response = await _employerAccountOrchestrator.GetGatewayTokenResponse(Request.Params["code"], Url.Action("GateWayResponse", "EmployerAccount", null, Request.Url.Scheme), System.Web.HttpContext.Current?.Request.QueryString);
            if (response.Status != HttpStatusCode.OK)
            {
                response.Status = HttpStatusCode.OK;
                return View("InvalidSummary", response);
            }

            var empref = await _employerAccountOrchestrator.GetHmrcEmployerInformation(response.Data.AccessToken);
            
            var enteredData = _employerAccountOrchestrator.GetCookieData(HttpContext);

            enteredData.EmployerRef = empref.Empref;
            enteredData.AccessToken = response.Data.AccessToken;
            enteredData.RefreshToken = response.Data.RefreshToken;
            _employerAccountOrchestrator.SetCookieData(HttpContext, enteredData);

            if (string.IsNullOrEmpty(empref.Empref))
            {
                return View("Summary", new OrchestratorResponse<SummaryViewModel> {
                    Data = ToSummaryViewModel(enteredData),
                    FlashMessage = new FlashMessageViewModel()
                    {
                        Message = "The PAYE scheme is already associated with an account. Please use a different gateway login",
                    }
                });
            }
            return RedirectToAction("Summary");
        }

        [HttpGet]
        public ActionResult Summary()
        {
            var enteredData = _employerAccountOrchestrator.GetCookieData(HttpContext);
            
            var model = new OrchestratorResponse<SummaryViewModel>
            {
                Data = ToSummaryViewModel(enteredData)
            };

            return View(model);
        }
        private SummaryViewModel ToSummaryViewModel(EmployerAccountData enteredData)
        {
            return new SummaryViewModel
            {
                CompanyName = enteredData.CompanyName,
                CompanyNumber = enteredData.CompanyNumber,
                DateOfIncorporation = enteredData.DateOfIncorporation,
                EmployerRef = enteredData.EmployerRef,
                RegisteredAddress = enteredData.RegisteredAddress
            };
        }

        [HttpPost]
        public async Task<ActionResult> CreateAccount()
        {
            var enteredData = _employerAccountOrchestrator.GetCookieData(HttpContext);

            await _employerAccountOrchestrator.CreateAccount(new CreateAccountModel
            {
                UserId = GetUserId(),
                CompanyNumber = enteredData.CompanyNumber,
                CompanyName = enteredData.CompanyName,
                CompanyRegisteredAddress = enteredData.RegisteredAddress,
                CompanyDateOfIncorporation = enteredData.DateOfIncorporation,
                EmployerRef = enteredData.EmployerRef,
                AccessToken = enteredData.AccessToken,
                RefreshToken = enteredData.RefreshToken
            });

            TempData["successHeader"] = "Account Created";
            TempData["successCompany"] = enteredData.CompanyName;

            return RedirectToAction("Index", "Home");
        }

        private string GetUserId()
        {
            var userIdClaim = _owinWrapper.GetClaimValue(@"sub");
            return userIdClaim ?? "";
        }
    }
}