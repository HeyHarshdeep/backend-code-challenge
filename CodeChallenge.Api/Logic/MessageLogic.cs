using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic;

public class MessageLogic : IMessageLogic
{
    private readonly IMessageRepository _repository;

    public MessageLogic(IMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
    {
        var errors = ValidateMessage(request.Title, request.Content);
        if (errors.Count > 0)
            return new ValidationError(errors);

        // Check for unique title
        var existing = await _repository.GetByTitleAsync(organizationId, request.Title);
        if (existing is not null)
            return new Conflict("Title must be unique within the organization.");

        var message = new Message
        {
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content,
            IsActive = true
        };

        var created = await _repository.CreateAsync(message);
        return new Created<Message>(created);
    }


    public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
    {
        var existing = await _repository.GetByIdAsync(organizationId, id);
        if (existing is null)
            return new NotFound("Message not found.");

        if (!existing.IsActive)
            return new ValidationError(new Dictionary<string, string[]> {
                { "IsActive", new[] { "Cannot update inactive messages." } }
            });

        var errors = ValidateMessage(request.Title, request.Content);
        if (errors.Count > 0)
            return new ValidationError(errors);

        var titleOwner = await _repository.GetByTitleAsync(organizationId, request.Title);
        if (titleOwner is not null && titleOwner.Id != id)
            return new Conflict("Title must be unique within the organization.");

        existing.Title = request.Title;
        existing.Content = request.Content;
        existing.IsActive = request.IsActive;

        var updated = await _repository.UpdateAsync(existing);
        if (updated is null)
            return new NotFound("Message not found.");

        return new Updated();
    }


    public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
    {
        var existing = await _repository.GetByIdAsync(organizationId, id);
        if (existing is null)
            return new NotFound("Message not found.");

        if (!existing.IsActive)
            return new ValidationError(new Dictionary<string, string[]> {
                { "IsActive", new[] { "Cannot delete inactive messages." } }
            });

        var deleted = await _repository.DeleteAsync(organizationId, id);
        if (!deleted)
            return new NotFound("Message not found.");

        return new Deleted();
    }


    public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
    {
        return await _repository.GetByIdAsync(organizationId, id);
    }

    public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
    {
        return await _repository.GetAllByOrganizationAsync(organizationId);
    }

    #region Private methods
    
    private Dictionary<string, string[]> ValidateMessage(string title, string content)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(title) || title.Length < 3 || title.Length > 200)
            errors["Title"] = new[] { "Title is required and must be between 3 and 200 characters." };

        if (string.IsNullOrWhiteSpace(content) || content.Length < 10 || content.Length > 1000)
            errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };

        return errors;
    }
    #endregion
}
