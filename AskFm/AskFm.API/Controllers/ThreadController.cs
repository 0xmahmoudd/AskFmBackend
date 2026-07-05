using AskFm.BLL.DTO;
using AskFm.BLL.Services;
using AskFm.BLL.Services.UserIdentityService;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AskFm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ThreadController : ControllerBase
{
    private readonly ILogger<ThreadController> _logger;
    private readonly IUserService _userService;
    private readonly IThreadService _threadService;

    public ThreadController(
        ILogger<ThreadController> logger,
        IUserService userService,
        IThreadService threadService)
    {
        _logger = logger;
        _userService = userService;
        _threadService = threadService;
    }


    // POST api/threads/ - Ask a question
    [HttpPost]
    [Route("thread")]
    public async Task<IActionResult> AskQuestion(CreateThreadDto createThreadDto)
    {
        // Asker Id -> Current User
        var user = await _userService.GetCurrentUserAsync();
        if (!user.success)
        {
            return BadRequest(user.Errors);
        }
        var userId = user.Data.Id;

        var result = await _threadService.AddThread(userId, createThreadDto);

        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);

    }


    // get all threads for user by user id
    [HttpGet]
    [Route("thread/{id}")]
    public async Task<IActionResult> GetAllThreads([FromRoute] int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var threads = await _threadService.GetAllThreads(id, page, pageSize);
        if (!threads.success)
        {
            return BadRequest(threads.Errors);
        }
        return Ok(threads.Data);
    }

    // GET api/threads/{id} - Getting the Thread with id = {id}
    [HttpGet]
    [Route("threads/{id}")]
    public async Task<IActionResult> GetThreadWithId([FromRoute] int id)
    {
        var thread = await _threadService.GetThreadById(id);
        if (!thread.success)
        {
            return BadRequest(thread.Errors);
        }
        return Ok(thread.Data);
    }


    // PUT api/threads/{id}/answer - Add an Answer on the thread with id = {id}
    [HttpPut]
    [Route("threads/{id}/answer")]
    public async Task<IActionResult> AnswerQuestion([FromRoute] int id, [FromBody] AnswerThreadDto answerDto)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (!user.success)
        {
            return BadRequest(user.Errors);
        }

        var result = await _threadService.AnswerThread(id, user.Data.Id, answerDto);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    // GET api/threads - Get all threads (with pagination)
    [HttpGet]
    [Route("threads")]
    public async Task<IActionResult> GetThreads([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var threads = await _threadService.GetThreads(page, pageSize);
        if (!threads.success)
        {
            return BadRequest(threads.Errors);
        }
        return Ok(threads.Data);
    }

    // DELETE api/threads/{id} - Delete a thread
    [HttpDelete]
    [Route("threads/{id}")]
    public async Task<IActionResult> DeleteThread([FromRoute] int id)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (!user.success)
        {
            return BadRequest(user.Errors);
        }

        var result = await _threadService.DeleteThread(id, user.Data.Id);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "Thread deleted successfully" });
    }

    // GET api/threads/feed - Get personalized feed for current user
    [HttpGet]
    [Route("threads/feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (!user.success)
        {
            return BadRequest(user.Errors);
        }

        var feed = await _threadService.GetFeed(user.Data.Id, page, pageSize);
        if (!feed.success)
        {
            return BadRequest(feed.Errors);
        }

        return Ok(feed.Data);
    }

    // POST api/threads/{id}/save - Save a thread
    [HttpPost]
    [Route("threads/{id}/save")]
    public async Task<IActionResult> SaveThread([FromRoute] int id)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (!user.success)
        {
            return BadRequest(user.Errors);
        }

        var result = await _threadService.SaveThread(id, user.Data.Id);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "Thread saved successfully" });
    }

    // DELETE api/threads/{id}/save - Unsave a thread
    [HttpDelete]
    [Route("threads/{id}/save")]
    public async Task<IActionResult> UnsaveThread([FromRoute] int id)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (!user.success)
        {
            return BadRequest(user.Errors);
        }

        var result = await _threadService.UnsaveThread(id, user.Data.Id);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "Thread unsaved successfully" });
    }

    // GET api/threads/saved - Get all saved threads for current user
    [HttpGet]
    [Route("threads/saved")]
    public async Task<IActionResult> GetSavedThreads([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (!user.success)
        {
            return BadRequest(user.Errors);
        }

        var threads = await _threadService.GetSavedThreads(user.Data.Id, page, pageSize);
        if (!threads.success)
        {
            return BadRequest(threads.Errors);
        }

        return Ok(threads.Data);
    }
}
