﻿using System.Linq;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerApprenticeshipsService.Application.Messages;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetUserAccounts;
using SFA.DAS.EmployerApprenticeshipsService.Domain;
using SFA.DAS.EmployerApprenticeshipsService.Web.AcceptanceTests.DbCleanup;
using SFA.DAS.EmployerApprenticeshipsService.Web.AcceptanceTests.DependencyResolution;
using SFA.DAS.EmployerApprenticeshipsService.Web.Models;
using SFA.DAS.EmployerApprenticeshipsService.Web.Orchestrators;
using SFA.DAS.Messaging;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.AcceptanceTests.Steps.InviteMember
{
    [Binding, Explicit]
    public class InviteMemberSteps
    {
        private IContainer _container;
        private string _externalUserId;
        private int _accountId;
        private Mock<IMessagePublisher> _messagePublisher;
        private ICleanDatabase _cleanDownDb;


        [BeforeScenario]
        public void Arrange()
        {
            _messagePublisher = new Mock<IMessagePublisher>();

            _container = IoC.CreateContainer(_messagePublisher);

            _cleanDownDb = _container.GetInstance<ICleanDatabase>();
            _cleanDownDb.Execute().Wait();
        }

        [AfterScenario]
        public void TearDown()
        {
            _cleanDownDb.Execute().Wait();

            _container.Dispose();
        }
        
        [Given(@"I am an account owner")]
        public void GivenIAmAnAccountOwner()
        {
            var user = GetExistingUserAccount();
            _externalUserId = user.UserId;

            CreateDasAccount(user);
        }

        [When(@"I invite a team member with email address ""(.*)"" and name ""(.*)""")]
        public void WhenIInviteATeamMemberWithEmailAddressAndName(string email, string name)
        {
            SetAccountIdForUser();

            CreateInvitationForGivenEmailAndName(email, name);
        }

        [Then(@"A user invite is ""(.*)""")]
        public void ThenAUserInviteIsWithPendingStatus(string createdStatus)
        {
            
            var orcehstrator = _container.GetInstance<EmployerTeamOrchestrator>();
            var teamMembers = orcehstrator.GetTeamMembers(_accountId, _externalUserId).Result;

            if (createdStatus.ToLower() == "created")
            {
                Assert.AreEqual(2,teamMembers.TeamMembers.Count);
                //Check to make sure an email has been sent
                _messagePublisher.Verify(x=>x.PublishAsync(It.IsAny<SendNotificationQueueMessage>()), Times.Once);

            }
            else
            {
                Assert.AreEqual(1, teamMembers.TeamMembers.Count);
                //Check to make sure an email has not been sent
                _messagePublisher.Verify(x => x.PublishAsync(It.IsAny<SendNotificationQueueMessage>()), Times.Never);
            }
        }

        private void CreateInvitationForGivenEmailAndName(string email, string name)
        {
            var orcehstrator = _container.GetInstance<InvitationOrchestrator>();
            orcehstrator.CreateInvitation(new InviteTeamMemberViewModel
            {
                AccountId = _accountId,
                Email = email,
                Name = name,
                Role = Role.Transactor
            }, _externalUserId).Wait();
        }

        private void SetAccountIdForUser()
        {
            var mediator = _container.GetInstance<IMediator>();
            var getUserAccountsQueryResponse = mediator.SendAsync(new GetUserAccountsQuery {UserId = _externalUserId}).Result;

            _accountId = getUserAccountsQueryResponse.Accounts.AccountList.FirstOrDefault().Id;
        }

        private SignInUserModel GetExistingUserAccount()
        {
            var homeOrchestrator = _container.GetInstance<HomeOrchestrator>();
            var user = homeOrchestrator.GetUsers().Result.AvailableUsers.FirstOrDefault();
            return user;
        }

        private void CreateDasAccount(SignInUserModel user)
        {
            var accountOrchestrator = _container.GetInstance<EmployerAccountOrchestrator>();

            accountOrchestrator.CreateAccount(new CreateAccountModel
            {
                CompanyName = "TestCompany",
                CompanyNumber = "123456",
                EmployerRef = "123/ABC123",
                UserId = user.UserId
            }).Wait();
        }
    }
}