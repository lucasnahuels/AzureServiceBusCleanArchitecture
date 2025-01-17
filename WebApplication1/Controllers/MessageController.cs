using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            await _messageService.SendMessageAsync(message);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _messageService.ReceiveMessagesAsync();
            return Ok(messages);
        }
    }
}
