using Core.Entities;

namespace Core.Interfaces
{
    public interface IMessageService
    {
        Task SendMessageAsync(Message message);
        Task<IEnumerable<Message>> ReceiveMessagesAsync();
    }
}
