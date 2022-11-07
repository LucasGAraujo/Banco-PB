using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Banco_PB.Data;
using Banco_PB.Models;

namespace Banco_PB.Controllers
{
    [Route("api/[controller]")] //mudarRota
    [ApiController]
    public class ContasApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContasApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ContasApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Conta>>> GetConta()
        {
            return await _context.Conta.ToListAsync();
        }

        // GET: api/ContasApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Conta>> GetConta(int id)
        {
            var conta = await _context.Conta.FindAsync(id);

            if (conta == null)
            {
                return NotFound();
            }

            return conta;
        }

        // PUT: api/ContasApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConta(int id, Conta conta)
        {
            if (id != conta.Id)
            {
                return BadRequest();
            }

            _context.Entry(conta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ContasApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Conta>> PostConta(Conta conta)
        {
            _context.Conta.Add(conta);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetConta", new { id = conta.Id }, conta);
        }

        // DELETE: api/ContasApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConta(int id)
        {
            var conta = await _context.Conta.FindAsync(id);
            if (conta == null)
            {
                return NotFound();
            }

            _context.Conta.Remove(conta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContaExists(int id)
        {
            return _context.Conta.Any(e => e.Id == id);
        }
    }
}
