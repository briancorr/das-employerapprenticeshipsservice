﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Application.Commands.CreateAccount;
using SFA.DAS.EAS.Application.Messages;
using SFA.DAS.EAS.Application.Validation;
using SFA.DAS.EAS.Domain;
using SFA.DAS.EAS.Domain.Data;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.Messaging;

namespace SFA.DAS.EAS.Application.UnitTests.Commands.CreateAccountCommandTests
{
    public class WhenICallCreateAccount
    {
        private Mock<IAccountRepository> _accountRepository;
        private Mock<IUserRepository> _userRepository;
        private CreateAccountCommandHandler _handler;
        private Mock<IMessagePublisher> _messagePublisher;
        private Mock<IMediator> _mediator;
        private Mock<IValidator<CreateAccountCommand>> _validator;
        private Mock<IHashingService> _hashingService;
        private const long ExpectedAccountId = 12343322;
        private const long ExpectedLegalEntityId = 2222;
        private const string ExpectedHashString = "123ADF23";

        [SetUp]
        public void Arrange()
        {
            _accountRepository = new Mock<IAccountRepository>();
            _accountRepository.Setup(x => x.GetPayeSchemesByAccountId(ExpectedAccountId)).ReturnsAsync(new List<PayeView> { new PayeView { LegalEntityId = ExpectedLegalEntityId } });
            _accountRepository.Setup(x => x.CreateAccount(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ExpectedAccountId);

            _userRepository = new Mock<IUserRepository>();
            _userRepository.Setup(x => x.GetByUserRef(It.IsAny<string>())).ReturnsAsync(new User());

            _messagePublisher = new Mock<IMessagePublisher>();
            _mediator = new Mock<IMediator>();
            _validator = new Mock<IValidator<CreateAccountCommand>>();
            _validator.Setup(x => x.ValidateAsync(It.IsAny<CreateAccountCommand>())).ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

            _hashingService = new Mock<IHashingService>();
            _hashingService.Setup(x => x.HashValue(ExpectedAccountId)).Returns(ExpectedHashString);

            _handler = new CreateAccountCommandHandler(_accountRepository.Object, _userRepository.Object, _messagePublisher.Object, _mediator.Object, _validator.Object, _hashingService.Object);
        }

        [Test]
        public async Task ThenIfThereAreMoreThanOneEmprefPassedThenTheyAreAddedToTheAccount()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand { EmployerRef = "123/abc,456/123", AccessToken = "123rd", RefreshToken = "45YT" };

            //Act
            await _handler.Handle(createAccountCommand);

            //Assert
            _accountRepository.Verify(x=>x.CreateAccount(It.IsAny<long>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<DateTime>(),"123/abc", "123rd", "45YT"), Times.Once);
            _accountRepository.Verify(x => x.AddPayeToAccount(It.Is<Paye>(c => c.AccountId.Equals(ExpectedAccountId) && c.EmpRef.Equals("456/123") && c.AccessToken.Equals("123rd") && c.RefreshToken.Equals("45YT"))), Times.Once);
        }

        [Test]
        public async Task ThenTheIdHashingServiceIsCalledAfterTheAccountIsCreated()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand { EmployerRef = "123/abc,456/123", AccessToken = "123rd", RefreshToken = "45YT" };

            //Act
            await _handler.Handle(createAccountCommand);

            //Assert
            _hashingService.Verify(x => x.HashValue(ExpectedAccountId), Times.Once);
        }

        [Test]
        public async Task ThenTheAccountIsUpdatedWithTheHashedId()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand { EmployerRef = "123/abc,456/123", AccessToken = "123rd", RefreshToken = "45YT" };

            //Act
            await _handler.Handle(createAccountCommand);

            //Assert
            _accountRepository.Verify(x => x.SetHashedId(ExpectedHashString, ExpectedAccountId), Times.Once);
        }

        [Test]
        public async Task ThenTheHashedIdIsReturnedInTheResponse()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand { EmployerRef = "123/abc,456/123", AccessToken = "123rd", RefreshToken = "45YT" };

            //Act
            var actual = await _handler.Handle(createAccountCommand);

            //Assert
            Assert.IsAssignableFrom<CreateAccountCommandResponse>(actual);
            Assert.AreEqual(ExpectedHashString, actual.HashedAccountId);
        }

        [Test]
        public void ThenTheValidatorIsCalledAndAInvalidRequestExceptionIsThrownWhenInvalid()
        {
            //Assert
            _validator.Setup(x => x.ValidateAsync(It.IsAny<CreateAccountCommand>())).ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

            //Act
            Assert.ThrowsAsync<InvalidRequestException>(async () => await _handler.Handle(new CreateAccountCommand()));

            //Assert
            _validator.Verify(x => x.ValidateAsync(It.IsAny<CreateAccountCommand>()), Times.Once);
        }

        [Test]
        public async Task WillCallRepositoryToCreateNewAccount()
        {
            const int accountId = 23;

            var user = new User()
            {
                Id = 33
            };

            var cmd = new CreateAccountCommand
            {
                ExternalUserId = Guid.NewGuid().ToString(),
                CompanyNumber = "QWERTY",
                CompanyName = "Qwerty Corp",
                CompanyRegisteredAddress = "Innovation Centre, Coventry, CV1 2TT",
                CompanyDateOfIncorporation = DateTime.Today.AddDays(-1000),
                EmployerRef = "120/QWERTY",
                AccessToken = Guid.NewGuid().ToString(),
                RefreshToken = Guid.NewGuid().ToString()
            };

            _userRepository.Setup(x => x.GetByUserRef(cmd.ExternalUserId)).ReturnsAsync(user);
            _accountRepository.Setup(x => x.CreateAccount(user.Id, cmd.CompanyNumber, cmd.CompanyName, cmd.CompanyRegisteredAddress, cmd.CompanyDateOfIncorporation, cmd.EmployerRef, cmd.AccessToken, cmd.RefreshToken)).ReturnsAsync(accountId);

            await _handler.Handle(cmd);

            _accountRepository.Verify(x => x.CreateAccount(user.Id, cmd.CompanyNumber, cmd.CompanyName, cmd.CompanyRegisteredAddress, cmd.CompanyDateOfIncorporation, cmd.EmployerRef, cmd.AccessToken, cmd.RefreshToken));
            _messagePublisher.Verify(x => x.PublishAsync(It.Is<EmployerRefreshLevyQueueMessage>(c => c.AccountId == accountId)), Times.Once());
        }

    }
}
