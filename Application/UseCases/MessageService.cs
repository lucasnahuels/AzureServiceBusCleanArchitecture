using Core.Entities;
using Core.Interfaces;
using Infrastructure.Repositories;

namespace Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly MessageRepository _messageRepository;

        public MessageService(MessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task SendMessageAsync(Message message)
        {
            await _messageRepository.AddMessageAsync(message);
        }

        public async Task<IEnumerable<Message>> ReceiveMessagesAsync()
        {
            return await _messageRepository.GetMessagesAsync();
        }
    }
}
