﻿using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Domain.Interfaces;

namespace SFA.DAS.EAS.Application.Queries.GetGatewayToken
{
    public class GetGatewayTokenHandler : IAsyncRequestHandler<GetGatewayTokenQuery, GetGatewayTokenQueryResponse>
    {
        private readonly IHmrcService _hmrcService;

        public GetGatewayTokenHandler(IHmrcService hmrcService)
        {
            _hmrcService = hmrcService;
        }

        public async Task<GetGatewayTokenQueryResponse> Handle(GetGatewayTokenQuery message)
        {
            var response =  await _hmrcService.GetAuthenticationToken(message.RedirectUrl, message.AccessCode);

            return new GetGatewayTokenQueryResponse
            {
                HmrcTokenResponse = response
            };
        }
    }
}