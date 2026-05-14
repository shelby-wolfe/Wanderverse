using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Wanderverse.Data;

namespace Wanderverse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly Database _database;

        public GameController()
        {
            _database = new Database("game.db");  
            _database.InitializeDatabase();  
        }

        // POST api/game/createplayer
        [HttpPost("createplayer")]
        public IActionResult CreatePlayer([FromBody] string username)
        {
            try
            {
                _database.AddPlayer(username);
                return Ok($"Player '{username}' created successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating player: {ex.Message}");
            }
        }

        // POST api/game/moveplayer
        [HttpPost("moveplayer")]
        public IActionResult MovePlayer([FromBody] PlayerMoveRequest moveRequest)
        {
            try
            {
                
                if (moveRequest.PlayerId <= 0 || moveRequest.LocationId <= 0)
                {
                    return BadRequest("Invalid PlayerId or LocationId.");
                }

                _database.RecordPlayerMove(moveRequest.PlayerId, moveRequest.LocationId);
                return Ok($"Player {moveRequest.PlayerId} moved to location {moveRequest.LocationId}.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error recording move: {ex.Message}");
            }
        }

        // GET api/game/playerpaths/{playerId}
        [HttpGet("playerpaths/{playerId}")]
        public IActionResult GetPlayerPaths(int playerId)
        {
            try
            {
                // Fetch player paths and return as response
                var paths = _database.GetPlayerPaths(playerId);
                return Ok(paths);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching player paths: {ex.Message}");
            }
        }
    }

    // Helper class for move request body (used in the MovePlayer endpoint)
    public class PlayerMoveRequest
    {
        public int PlayerId { get; set; }
        public int LocationId { get; set; }
    }
}