using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elzahy.DTOs;
using Elzahy.Services;

namespace Elzahy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IContactMessageService _contactMessageService;

        public ContactController(IContactMessageService contactMessageService)
        {
            _contactMessageService = contactMessageService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ContactMessageDto>>> CreateContactMessage([FromBody] CreateContactMessageRequestDto request)
        {
            var result = await _contactMessageService.CreateContactMessageAsync(request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return CreatedAtAction(nameof(GetContactMessage), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ContactMessageDto>>> GetContactMessage(Guid id)
        {
            var result = await _contactMessageService.GetContactMessageAsync(id);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ContactMessageDto>>>> GetContactMessages([FromQuery] ContactMessageFilterDto filter)
        {
            var result = await _contactMessageService.GetContactMessagesAsync(filter);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ContactMessageDto>>> UpdateContactMessage(Guid id, [FromBody] UpdateContactMessageRequestDto request)
        {
            var result = await _contactMessageService.UpdateContactMessageAsync(id, request);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("{id}/mark-read")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(Guid id)
        {
            var result = await _contactMessageService.MarkAsReadAsync(id);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("{id}/mark-replied")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAsReplied(Guid id)
        {
            var result = await _contactMessageService.MarkAsRepliedAsync(id);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteContactMessage(Guid id)
        {
            var result = await _contactMessageService.DeleteContactMessageAsync(id);
            
            if (!result.Ok)
                return BadRequest(result);
                
            return Ok(result);
        }
    }
}