﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Moq;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Interfaces;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Models.HmrcLevy;
using SFA.DAS.EmployerApprenticeshipsService.Infrastructure.Services;

namespace SFA.DAS.EAS.Infrastructure.UnitTests.Services.HmrcServiceTests
{
    public class WhenICallTheHmrcService
    {
        private HmrcService _hmrcService;
        private Mock<ILogger> _logger;
        private EmployerApprenticeshipsServiceConfiguration _configuration;
        private string ExpectedBaseUrl = "http://hmrcbase.gov.uk/auth/";
        private string ExpectedClientId = "654321";
        private string ExpectedScope = "emp_ref";
        private Mock<IHttpClientWrapper> _httpClientWrapper;
        private string ExpectedClientSecret = "my_secret";

        [SetUp]
        public void Arrange()
        {
            _configuration = new EmployerApprenticeshipsServiceConfiguration
            {
                Hmrc = new HmrcConfiguration
                {
                    BaseUrl = ExpectedBaseUrl,
                    ClientId = ExpectedClientId,
                    Scope = ExpectedScope,
                    ClientSecret = ExpectedClientSecret
                }
            };

            _logger = new Mock<ILogger>();
            _httpClientWrapper = new Mock<IHttpClientWrapper>();
            _httpClientWrapper.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(JsonConvert.SerializeObject(new HmrcTokenResponse()));

            _hmrcService = new HmrcService(_logger.Object, _configuration, _httpClientWrapper.Object);
        }

        [Test]
        public void ThenTheAuthUrlIsGeneratedFromTheStoredConfigValues()
        {
            //Arrange
            var redirectUrl = "http://mytestUrl.to.redirectto?a=564kjg";
            var urlFriendlyRedirectUrl = HttpUtility.UrlEncode(redirectUrl);

            //Assert
            var actual = _hmrcService.GenerateAuthRedirectUrl(redirectUrl);

            //Assert
            Assert.AreEqual($"{ExpectedBaseUrl}authorize?response_type=code&client_id={ExpectedClientId}&scope={ExpectedScope}&redirect_uri={urlFriendlyRedirectUrl}", actual);
        }

        [Test]
        public async Task ThenTheCodeIsExchangedForTheAccessToken()
        {
            //Arrange
            var redirectUrl = "http://mytestUrl.to.redirectto?a=564kjg";
            var urlFriendlyRedirectUrl = HttpUtility.UrlEncode(redirectUrl);
            var code = "ghj567";

            //Act
            var actual = await _hmrcService.GetAuthenticationToken(redirectUrl,code);

            //Assert
            _httpClientWrapper.Verify(x => x.SendMessage("", $"token?client_secret={ExpectedClientSecret}&client_id={ExpectedClientId}&grant_type=authorization_code&redirect_uri={urlFriendlyRedirectUrl}&code={code}"), Times.Once);
            Assert.IsAssignableFrom<HmrcTokenResponse>(actual);

        }
    }
}