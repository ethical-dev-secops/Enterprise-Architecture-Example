using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using BankAccountManagementSystem.Application.Common.Behaviours;
using BankAccountManagementSystem.Application.SavingsAccounts.CreateSavingsAccount;
using FluentAssertions;
using FluentValidation;
using Intent.RoslynWeaver.Attributes;
using MediatR;
using NSubstitute;
using Xunit;

[assembly: DefaultIntentManaged(Mode.Fully)]
[assembly: IntentTemplate("Intent.Application.MediatR.CRUD.Tests.FluentValidation.FluentValidationTest", Version = "1.0")]

namespace BankAccountManagementSystem.Application.Tests.SavingsAccounts
{
    public class CreateSavingsAccountCommandValidatorTests
    {
        public static IEnumerable<object[]> GetSuccessfulResultTestData()
        {
            var fixture = new Fixture();
            var testCommand = fixture.Create<CreateSavingsAccountCommand>();
            testCommand.CurrencyIsoCode = $"{string.Join(string.Empty, fixture.CreateMany<char>(3))}";
            yield return new object[] { testCommand };
        }

        [Theory]
        [MemberData(nameof(GetSuccessfulResultTestData))]
        public async Task Validate_WithValidCommand_PassesValidation(CreateSavingsAccountCommand testCommand)
        {
            // Arrange
            var validator = GetValidationBehaviour();
            var expectedId = new Fixture().Create<long>();
            // Act
            var result = await validator.Handle(testCommand, () => Task.FromResult(expectedId), CancellationToken.None);

            // Assert
            result.Should().Be(expectedId);
        }

        public static IEnumerable<object[]> GetFailedResultTestData()
        {
            var fixture = new Fixture();
            fixture.Customize<CreateSavingsAccountCommand>(comp => comp.With(x => x.CurrencyIsoCode, () => default));
            var testCommand = fixture.Create<CreateSavingsAccountCommand>();
            yield return new object[] { testCommand, "CurrencyIsoCode", "not be empty" };

            fixture = new Fixture();
            fixture.Customize<CreateSavingsAccountCommand>(comp => comp.With(x => x.CurrencyIsoCode, () => $"{string.Join(string.Empty, fixture.CreateMany<char>(4))}"));
            testCommand = fixture.Create<CreateSavingsAccountCommand>();
            yield return new object[] { testCommand, "CurrencyIsoCode", "must be 3 characters or fewer" };
        }

        [Theory]
        [MemberData(nameof(GetFailedResultTestData))]
        public async Task Validate_WithInvalidCommand_FailsValidation(
            CreateSavingsAccountCommand testCommand,
            string expectedPropertyName,
            string expectedPhrase)
        {
            // Arrange
            var validator = GetValidationBehaviour();
            var expectedId = new Fixture().Create<long>();
            // Act
            var act = async () => await validator.Handle(testCommand, () => Task.FromResult(expectedId), CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<ValidationException>().Result
            .Which.Errors.Should().Contain(x => x.PropertyName == expectedPropertyName && x.ErrorMessage.Contains(expectedPhrase));
        }

        private ValidationBehaviour<CreateSavingsAccountCommand, long> GetValidationBehaviour()
        {
            return new ValidationBehaviour<CreateSavingsAccountCommand, long>(new[] { new CreateSavingsAccountCommandValidator() });
        }
    }
}