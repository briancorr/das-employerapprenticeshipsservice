﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerApprenticeshipsService.Domain;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Data;

namespace SFA.DAS.EmployerApprenticeshipsService.Application.Commands.RemoveTeamMember
{
    public class RemoveTeamMemberCommandHandler : AsyncRequestHandler<RemoveTeamMemberCommand>
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly RemoveTeamMemberCommandValidator _validator;

        public RemoveTeamMemberCommandHandler(IMembershipRepository membershipRepository)
        {
            if (membershipRepository == null)
                throw new ArgumentNullException(nameof(membershipRepository));
            _membershipRepository = membershipRepository;
            _validator = new RemoveTeamMemberCommandValidator();
        }

        protected override async Task HandleCore(RemoveTeamMemberCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
                throw new InvalidRequestException(validationResult.ValidationDictionary);

            var owner = await _membershipRepository.GetCaller(message.AccountId, message.ExternalUserId);

            if (owner == null)
                throw new InvalidRequestException(new Dictionary<string, string> { { "Membership", "User is not a member of this Account" } });
            if ((Role)owner.RoleId != Role.Owner)
                throw new InvalidRequestException(new Dictionary<string, string> { { "Membership", "User is not an Owner" } });

            var existing = await _membershipRepository.Get(message.UserId, message.AccountId);

            if (existing == null)
                throw new InvalidRequestException(new Dictionary<string, string> { { "Membership", "User is not a member of this team" } });

            if (message.UserId == owner.UserId)
                throw new InvalidRequestException(new Dictionary<string, string> { { "Membership", "You cannot remove yourself" } });

            await _membershipRepository.Remove(message.UserId, message.AccountId);
        }
    }
}