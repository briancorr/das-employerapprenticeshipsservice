﻿using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Application.Queries.GetUserAccounts;
using SFA.DAS.EAS.Application.Queries.GetUserInvitations;
using SFA.DAS.EAS.Application.Queries.GetUsers;
using SFA.DAS.EAS.Web.Models;

namespace SFA.DAS.EAS.Web.Orchestrators
{
    public class HomeOrchestrator : IOrchestrator
    {
        private readonly IMediator _mediator;

        //Required for running tests
        public HomeOrchestrator()
        {

        }

        public HomeOrchestrator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public virtual async Task<SignInUserViewModel> GetUsers()
        {
            var actual = await _mediator.SendAsync(new GetUsersQuery());

            return new SignInUserViewModel
            {
                AvailableUsers = actual.Users.Select(x =>
                                                new SignInUserModel
                                                {
                                                    Email = x.Email,
                                                    FirstName = x.FirstName,
                                                    LastName = x.LastName,
                                                    UserId = x.UserRef
                                                }).ToList()
            };
        }

        public virtual async Task<OrchestratorResponse<UserAccountsViewModel>> GetUserAccounts(string userId)
        {
            var getUserAccountsQueryResponse = await _mediator.SendAsync(new GetUserAccountsQuery
            {
                UserId = userId
            });
            var getUserInvitationsResponse = await _mediator.SendAsync(new GetNumberOfUserInvitationsQuery
            {
                UserId = userId
            });
            return new OrchestratorResponse<UserAccountsViewModel>
            {
                Data = new UserAccountsViewModel
                {
                    Accounts = getUserAccountsQueryResponse.Accounts,
                    Invitations = getUserInvitationsResponse.NumberOfInvites
                }
            };
        }
    }
}