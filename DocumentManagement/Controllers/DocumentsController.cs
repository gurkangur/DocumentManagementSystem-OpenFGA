using DocumentManagement.Data;
using DocumentManagement.Models;
using DocumentManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly DocumentDbContext _context;
        private readonly OpenFgaService _fgaService;

        public DocumentsController(DocumentDbContext context, OpenFgaService fgaService)
        {
            _context = context;
            _fgaService = fgaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocuments([FromHeader] Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();
            return await _context.Documents.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(Guid id, [FromHeader] Guid userId)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
                return NotFound();

            bool hasPermission = await _fgaService.CheckUserPermission(userId.ToString(), id.ToString(), Permission.Read);
            if (!hasPermission)
                return Forbid();

            return document;
        }

        [HttpPost]
        public async Task<ActionResult<Document>> CreateDocument(Document document, [FromHeader] Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            document.Id = Guid.NewGuid();
            document.CreatedAt = DateTime.UtcNow;
            document.ModifiedAt = DateTime.UtcNow;
            document.OwnerId = userId;

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            await _fgaService.AddDocumentOwner(document.Id.ToString(), userId.ToString());

            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(Guid id, Document document, [FromHeader] Guid userId)
        {
            if (id != document.Id)
                return BadRequest();

            bool hasPermission = await _fgaService.CheckUserPermission(userId.ToString(), id.ToString(), Permission.Write);
            if (!hasPermission)
                return Forbid();

            var existingDocument = await _context.Documents.FindAsync(id);
            if (existingDocument == null)
                return NotFound();

            existingDocument.Name = document.Name;
            existingDocument.Content = document.Content;
            existingDocument.ModifiedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(Guid id, [FromHeader] Guid userId)
        {
            bool hasPermission = await _fgaService.CheckUserPermission(userId.ToString(), id.ToString(), Permission.Delete);
            if (!hasPermission)
                return Forbid();

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareDocument(Guid id, [FromBody] ShareDocumentRequest request, [FromHeader] Guid userId)
        {
            bool hasPermission = await _fgaService.CheckUserPermission(userId.ToString(), id.ToString(), Permission.Share);
            if (!hasPermission)
                return Forbid();

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            var targetUser = await _context.Users.FindAsync(request.TargetUserId);
            if (targetUser == null)
                return BadRequest("Target user not found");

            await _fgaService.ShareDocumentWithUser(id.ToString(), request.TargetUserId.ToString(), request.Permission);

            return Ok();
        }

        private bool DocumentExists(Guid id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
    public class ShareDocumentRequest
    {
        public Guid TargetUserId { get; set; }
        public required string Permission { get; set; }
    }
}
