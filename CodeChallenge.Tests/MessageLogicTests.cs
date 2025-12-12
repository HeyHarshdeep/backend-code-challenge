using Xunit;
using Moq;
using FluentAssertions;
using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Tests;

public class MessageLogicTests
{
    private readonly Mock<IMessageRepository> _repoMock;
    private readonly MessageLogic _logic;

    public MessageLogicTests()
    {
        _repoMock = new Mock<IMessageRepository>();
        _logic = new MessageLogic(_repoMock.Object);
    }


    // CREATE TESTS
    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var request = new CreateMessageRequest
        {
            Title = "My Title",
            Content = "This is valid content for creation."
        };

        // No existing message with same title
        _repoMock.Setup(r => r.GetByTitleAsync(orgId, request.Title))
                 .ReturnsAsync((Message?)null);

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                 .ReturnsAsync((Message m) => m);

        // Act
        var result = await _logic.CreateMessageAsync(orgId, request);

        // Assert
        result.Should().BeOfType<Created<Message>>();
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenTitleAlreadyExists()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var request = new CreateMessageRequest
        {
            Title = "Existing",
            Content = "Some valid content here."
        };

        _repoMock.Setup(r => r.GetByTitleAsync(orgId, request.Title))
                 .ReturnsAsync(new Message { Title = request.Title, OrganizationId = orgId });

        // Act
        var result = await _logic.CreateMessageAsync(orgId, request);

        // Assert
        result.Should().BeOfType<Conflict>();
    }

    [Fact]
    public async Task Create_ShouldReturnValidationError_WhenContentTooShort()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var request = new CreateMessageRequest
        {
            Title = "Valid Title",
            Content = "short"
        };

        // Act
        var result = await _logic.CreateMessageAsync(orgId, request);

        // Assert
        result.Should().BeOfType<ValidationError>();
    }


    // UPDATE TESTS

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenMessageDoesNotExist()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(orgId, id))
                 .ReturnsAsync((Message?)null);

        var request = new UpdateMessageRequest
        {
            Title = "New Title",
            Content = "Updated content that is fine.",
            IsActive = true
        };

        // Act
        var result = await _logic.UpdateMessageAsync(orgId, id, request);

        // Assert
        result.Should().BeOfType<NotFound>();
    }

    [Fact]
    public async Task Update_ShouldReturnValidationError_WhenMessageIsInactive()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var inactive = new Message
        {
            Id = id,
            OrganizationId = orgId,
            Title = "Old",
            Content = "Some content",
            IsActive = false
        };

        _repoMock.Setup(r => r.GetByIdAsync(orgId, id))
                 .ReturnsAsync(inactive);

        var request = new UpdateMessageRequest
        {
            Title = "New Title",
            Content = "Updated valid text.",
            IsActive = false
        };

        // Act
        var result = await _logic.UpdateMessageAsync(orgId, id, request);

        // Assert
        result.Should().BeOfType<ValidationError>();
    }


    // DELETE TESTS
    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenMessageDoesNotExist()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(orgId, id))
                 .ReturnsAsync((Message?)null);

        // Act
        var result = await _logic.DeleteMessageAsync(orgId, id);

        // Assert
        result.Should().BeOfType<NotFound>();
    }
}
