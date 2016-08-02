using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using NLog;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Interfaces;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Models.HmrcLevy;

namespace SFA.DAS.EmployerApprenticeshipsService.Infrastructure.Services
{
    public class HmrcService : IHmrcService
    {
        private readonly ILogger _logger;
        private readonly EmployerApprenticeshipsServiceConfiguration _configuration;
        private readonly IHttpClientWrapper _httpClientWrapper;

        public HmrcService(ILogger logger, EmployerApprenticeshipsServiceConfiguration configuration, IHttpClientWrapper httpClientWrapper)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientWrapper = httpClientWrapper;
        }

        public string GenerateAuthRedirectUrl(string redirectUrl)
        {

            var urlFriendlyRedirectUrl = HttpUtility.UrlEncode(redirectUrl);

            return $"{_configuration.Hmrc.BaseUrl}authorize?response_type=code&client_id={_configuration.Hmrc.ClientId}&scope={_configuration.Hmrc.Scope}&redirect_uri={urlFriendlyRedirectUrl}";
            
        }

        public async Task<HmrcTokenResponse> GetAuthenticationToken(string redirectUrl, string accessCode)
        {
            var urlFriendlyRedirectUrl = HttpUtility.UrlEncode(redirectUrl);

            var url = $"token?client_secret={_configuration.Hmrc.ClientSecret}&client_id={_configuration.Hmrc.ClientId}&grant_type=authorization_code&redirect_uri={urlFriendlyRedirectUrl}&code={accessCode}";

            var response = await _httpClientWrapper.SendMessage("", url);

            return JsonConvert.DeserializeObject<HmrcTokenResponse>(response);

        }
    }
}