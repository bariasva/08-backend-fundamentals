using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ArticlesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/articles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticleReadDto>>> GetArticles()
        {
            var articles = await _dbContext.Articles
                .Include(a => a.Company) // Assuming there's a navigation property
                .Select(a => new ArticleReadDto
                {
                    Id = a.id,
                    Name = a.name,
                    Value = a.value,
                    CompanyId = a.companyId,
                    CompanyName = a.Company.name // Include company name
                })
                .ToListAsync();

            return Ok(articles);
        }

        // GET: api/articles/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleReadDto>> GetArticle(int id)
        {
            var article = await _dbContext.Articles
                .Include(a => a.Company)
                .Where(a => a.id == id)
                .Select(a => new ArticleReadDto
                {
                    Id = a.id,
                    Name = a.name,
                    Value = a.value,
                    CompanyId = a.companyId,
                    CompanyName = a.Company.name // Include company name
                })
                .FirstOrDefaultAsync();

            if (article == null)
            {
                return NotFound("Article not found");
            }

            return Ok(article);
        }

        // POST: api/articles
        [HttpPost]
        public async Task<ActionResult<ArticleReadDto>> CreateArticle([FromBody] ArticleCreateDto articleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = new Article
            {
                name = articleDto.Name,
                value = articleDto.Value,
                companyId = articleDto.CompanyId
            };

            _dbContext.Articles.Add(article);
            await _dbContext.SaveChangesAsync();

            var articleReadDto = new ArticleReadDto
            {
                Id = article.id,
                Name = article.name,
                Value = article.value,
                CompanyId = article.companyId,
                CompanyName = (await _dbContext.Companies.FindAsync(article.companyId))?.name
            };

            return CreatedAtAction(nameof(GetArticle), new { id = article.id }, articleReadDto);
        }

        // PUT: api/articles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] ArticleUpdateDto articleDto)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound("Article not found");
            }

            article.name = articleDto.Name;
            article.value = articleDto.Value;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
                {
                    return NotFound("Article not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/articles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound("Article not found");
            }

            _dbContext.Articles.Remove(article);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool ArticleExists(int id)
        {
            return _dbContext.Articles.Any(e => e.id == id);
        }
    }

}
