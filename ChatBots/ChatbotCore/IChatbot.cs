namespace ChatbotCore
{
    public interface IChatbot
    {
        void RecognizeCmd(string cmdString);

        string CreateResponse(string newsString);
        void Respond(string responseString);
    }
}