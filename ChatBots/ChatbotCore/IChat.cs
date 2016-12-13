namespace ChatbotCore
{
    public interface IChat
    {
        void HandleIncomingMessage(string message);
        void sendMessage(string message);
    }
}
