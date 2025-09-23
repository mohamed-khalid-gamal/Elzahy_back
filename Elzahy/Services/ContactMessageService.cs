using Microsoft.EntityFrameworkCore;
using Elzahy.Data;
using Elzahy.DTOs;
using Elzahy.Models;

namespace Elzahy.Services
{
    public interface IContactMessageService
    {
        Task<ApiResponse<ContactMessageDto>> CreateContactMessageAsync(CreateContactMessageRequestDto request);
        Task<ApiResponse<ContactMessageDto>> GetContactMessageAsync(Guid id);
        Task<ApiResponse<PagedResponse<ContactMessageDto>>> GetContactMessagesAsync(ContactMessageFilterDto filter);
        Task<ApiResponse<ContactMessageDto>> UpdateContactMessageAsync(Guid id, UpdateContactMessageRequestDto request);
        Task<ApiResponse<bool>> DeleteContactMessageAsync(Guid id);
        Task<ApiResponse<bool>> MarkAsReadAsync(Guid id);
        Task<ApiResponse<bool>> MarkAsRepliedAsync(Guid id);
    }

    public class ContactMessageService : IContactMessageService
    {
        private readonly AppDbContext _context;

        public ContactMessageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<ContactMessageDto>> CreateContactMessageAsync(CreateContactMessageRequestDto request)
        {
            try
            {
                var contactMessage = new ContactMessage
                {
                    FullName = request.FullName,
                    EmailAddress = request.EmailAddress.ToLowerInvariant(),
                    Subject = request.Subject,
                    Message = request.Message,
                    PhoneNumber = request.PhoneNumber,
                    Company = request.Company
                };

                _context.ContactMessages.Add(contactMessage);
                await _context.SaveChangesAsync();

                return ApiResponse<ContactMessageDto>.Success(ContactMessageDto.FromContactMessage(contactMessage));
            }
            catch (Exception ex)
            {
                return ApiResponse<ContactMessageDto>.Failure($"Failed to create contact message: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<ContactMessageDto>> GetContactMessageAsync(Guid id)
        {
            try
            {
                var contactMessage = await _context.ContactMessages.FirstOrDefaultAsync(cm => cm.Id == id);

                if (contactMessage == null)
                {
                    return ApiResponse<ContactMessageDto>.Failure("Contact message not found", 4004);
                }

                return ApiResponse<ContactMessageDto>.Success(ContactMessageDto.FromContactMessage(contactMessage));
            }
            catch (Exception ex)
            {
                return ApiResponse<ContactMessageDto>.Failure($"Failed to get contact message: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<PagedResponse<ContactMessageDto>>> GetContactMessagesAsync(ContactMessageFilterDto filter)
        {
            try
            {
                var query = _context.ContactMessages.AsQueryable();

                // Apply filters
                if (filter.FromDate.HasValue)
                    query = query.Where(cm => cm.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(cm => cm.CreatedAt <= filter.ToDate.Value);

                if (filter.IsRead.HasValue)
                    query = query.Where(cm => cm.IsRead == filter.IsRead.Value);

                if (filter.IsReplied.HasValue)
                    query = query.Where(cm => cm.IsReplied == filter.IsReplied.Value);

                // Apply sorting
                query = filter.SortBy?.ToLowerInvariant() switch
                {
                    "subject" => filter.SortDescending ? 
                        query.OrderByDescending(cm => cm.Subject) : 
                        query.OrderBy(cm => cm.Subject),
                    "fullname" => filter.SortDescending ? 
                        query.OrderByDescending(cm => cm.FullName) : 
                        query.OrderBy(cm => cm.FullName),
                    "createdat" or _ => filter.SortDescending ? 
                        query.OrderByDescending(cm => cm.CreatedAt) : 
                        query.OrderBy(cm => cm.CreatedAt)
                };

                var totalCount = await query.CountAsync();

                var contactMessages = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var contactMessageDtos = contactMessages.Select(ContactMessageDto.FromContactMessage).ToList();

                var pagedResponse = new PagedResponse<ContactMessageDto>
                {
                    Data = contactMessageDtos,
                    TotalCount = totalCount,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize
                };

                return ApiResponse<PagedResponse<ContactMessageDto>>.Success(pagedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<ContactMessageDto>>.Failure($"Failed to get contact messages: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<ContactMessageDto>> UpdateContactMessageAsync(Guid id, UpdateContactMessageRequestDto request)
        {
            try
            {
                var contactMessage = await _context.ContactMessages.FirstOrDefaultAsync(cm => cm.Id == id);

                if (contactMessage == null)
                {
                    return ApiResponse<ContactMessageDto>.Failure("Contact message not found", 4004);
                }

                if (request.IsRead.HasValue)
                {
                    contactMessage.IsRead = request.IsRead.Value;
                    if (request.IsRead.Value && !contactMessage.ReadAt.HasValue)
                        contactMessage.ReadAt = DateTime.UtcNow;
                }

                if (request.IsReplied.HasValue)
                {
                    contactMessage.IsReplied = request.IsReplied.Value;
                    if (request.IsReplied.Value && !contactMessage.RepliedAt.HasValue)
                        contactMessage.RepliedAt = DateTime.UtcNow;
                }

                if (request.AdminNotes != null)
                    contactMessage.AdminNotes = request.AdminNotes;

                await _context.SaveChangesAsync();

                return ApiResponse<ContactMessageDto>.Success(ContactMessageDto.FromContactMessage(contactMessage));
            }
            catch (Exception ex)
            {
                return ApiResponse<ContactMessageDto>.Failure($"Failed to update contact message: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> DeleteContactMessageAsync(Guid id)
        {
            try
            {
                var contactMessage = await _context.ContactMessages.FirstOrDefaultAsync(cm => cm.Id == id);
                if (contactMessage == null)
                {
                    return ApiResponse<bool>.Failure("Contact message not found", 4004);
                }

                _context.ContactMessages.Remove(contactMessage);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure($"Failed to delete contact message: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid id)
        {
            try
            {
                var contactMessage = await _context.ContactMessages.FirstOrDefaultAsync(cm => cm.Id == id);
                if (contactMessage == null)
                {
                    return ApiResponse<bool>.Failure("Contact message not found", 4004);
                }

                contactMessage.IsRead = true;
                contactMessage.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure($"Failed to mark message as read: {ex.Message}", 5000);
            }
        }

        public async Task<ApiResponse<bool>> MarkAsRepliedAsync(Guid id)
        {
            try
            {
                var contactMessage = await _context.ContactMessages.FirstOrDefaultAsync(cm => cm.Id == id);
                if (contactMessage == null)
                {
                    return ApiResponse<bool>.Failure("Contact message not found", 4004);
                }

                contactMessage.IsReplied = true;
                contactMessage.RepliedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure($"Failed to mark message as replied: {ex.Message}", 5000);
            }
        }
    }
}