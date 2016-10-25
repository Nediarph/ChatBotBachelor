namespace ChatbotCore
{
    public interface IChatbot
    {
        void RecognizeCmd(string cmdString);
        void Respond(string responseString);
    }
}