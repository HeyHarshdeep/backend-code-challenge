using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> _logger;
    private readonly IMessageLogic _messageLogic;


    public MessagesController(ILogger<MessagesController> logger, IMessageLogic messageLogic)
    {
        _logger = logger;
        _messageLogic = messageLogic; 
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
    {
        var messages = await _messageLogic.GetAllMessagesAsync(organizationId);
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        var message = await _messageLogic.GetMessageAsync(organizationId, id);
        if (message is null)
            return NotFound();

        return Ok(message);
    }

    [HttpPost]
    public async Task<ActionResult> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        var result = await _messageLogic.CreateMessageAsync(organizationId, request);


        if (result is Created<Message> created)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { organizationId, id = created.Value.Id },
                created.Value
            );
        }

        if (result is ValidationError ve)
            return BadRequest(ve.Errors);

        if (result is Conflict c)
            return Conflict(c.Message);

        return StatusCode(500);
    }



    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        var result = await _messageLogic.UpdateMessageAsync(organizationId, id, request);

        if (result is Updated)
            return NoContent();

        if (result is NotFound nf)
            return NotFound(nf.Message);

        if (result is Conflict c)
            return Conflict(c.Message);

        if (result is ValidationError ve)
            return BadRequest(ve.Errors);

        return StatusCode(500);
    }


    // DELETE an existing message
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        var result = await _messageLogic.DeleteMessageAsync(organizationId, id);

        if (result is Deleted)
            return NoContent();

        if (result is NotFound nf)
            return NotFound(nf.Message);

        if (result is ValidationError ve)
            return BadRequest(ve.Errors);

        return StatusCode(500);
    }
}
