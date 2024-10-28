using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ArticlesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/articles
        [HttpGet]
        public async Task<IActionResult> GetArticles()
        {
            var articles = await _dbContext.Articles.ToListAsync();
            return Ok(articles);
        }

        // GET: api/articles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticle(int id)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound("Article not found");
            }
            return Ok(article);
        }

        // POST: api/articles
        [HttpPost]
        public async Task<IActionResult> CreateArticle(Article article)
        {
            _dbContext.Articles.Add(article);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetArticle), new { id = article.id }, article);
        }

        // PUT: api/articles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, Article article)
        {
            if (id != article.id)
            {
                return BadRequest("Article ID mismatch");
            }

            _dbContext.Entry(article).State = EntityState.Modified;

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
