using System.Text.Json;

namespace Sendsign
{
    class SendsignOngoingMessages : IDisposable
    {
        private IList<string> messageIds = [];
        private string filepath = "";
        
        public SendsignOngoingMessages(string filepath)
        {
            this.filepath = filepath;
            if (File.Exists(filepath))
            {
                messageIds = JsonSerializer.Deserialize<IList<string>>(File.ReadAllText(filepath));
            }
        }

        public void AddMessage(string messageId)
        {
            messageIds.Add(messageId);
        }

        public void Save()
        {
            File.WriteAllText(filepath, JsonSerializer.Serialize(messageIds));
        }

        public void Dispose()
        {
            Save();
        }

        public IList<string> GetMessages()
        {
            return messageIds;
        }
    }
}