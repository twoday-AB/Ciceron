using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sendsign;
using Sendsign.JsonDocuments;

namespace sendsign_console_application
{
    class Program
    {
        public static string Sender;
        public static string CustomerKey;

        public static string Prompt(string prompt, Nullable<ConsoleColor> promptColor = ConsoleColor.DarkGray)
        {
            Console.ForegroundColor = promptColor ?? Console.ForegroundColor;
            Console.Write(prompt + ": ");
            Console.ResetColor();
            return Console.ReadLine();
        }

        public static SendsignSendRequest PromptSendsignRequestBase()
        {
            SendsignSendRequest request = new SendsignSendRequest();
            
            request.Subject = Prompt("Email subject");
            request.Body = Prompt("Email body");
            request.MessageType = Prompt("Message type (for example: AVTAL)");
            request.CustomerKey = CustomerKey;
            request.Sender = Sender;

            int ttf = 0;
            int.TryParse(Prompt("Time to live (hours, default is 7 days)"), out ttf);

            if (request.TimeToLiveHours > 0)
            {
                request.TimeToLiveHours = ttf;
            }

            return request;
        }

        public static SendsignRecipient PromptCreateRecipient()
        {
            SendsignRecipient recipient = new SendsignRecipient();
            
            while (recipient.Type == null || recipient.Type == "" || (recipient.Type != "internal" && recipient.Type != "external"))
                recipient.Type = Prompt("Message type (internal or external)");
            
            if (recipient.Type == "internal")
            {
                while (recipient.Ssn == null || recipient.Ssn == "")
                    recipient.Ssn = Prompt("Reciever social security number");

                recipient.Mail = Prompt("Reciever email");
                recipient.Name = Prompt("Reciever name");
            }
            else
            {
                while (recipient.Mail == null || recipient.Mail == "")
                    recipient.Mail = Prompt("Reciever email");

                while (recipient.Name == null || recipient.Name == "") 
                    recipient.Name = Prompt("Reciever name");

                recipient.Ssn = Prompt("Reciever social security number");
            }

            recipient.Sms = Prompt("Phone number (sms)");
            return recipient;
        }

        public static List<SendsignRequest> PromptMinimalSendsignRequest(ref SendsignOngoingMessages messages)
        {
            SelectionMenu selectCancelMenu = new SelectionMenu(messages.GetMessages().ToList(), "Select message");
            List<SendsignRequest> items = new List<SendsignRequest>();

            do
            {
                int index = selectCancelMenu.Prompt();
                if (index == -1)
                {
                    break;
                }

                items.Add(new SendsignRequest()
                {
                    MessageId = messages.GetMessages()[index],
                    CustomerKey = CustomerKey,
                    Sender = Sender
                });
            } 
            while (messages.GetMessages().Count > items.Count && 
                    Prompt("Another? (y/N)").ToLower().Equals("y"));

            return items;
        }

        public static SendsignAttachment PromptSendsignRequestAttachment()
        {
            return SendsignAttachment.LoadFile(Prompt("Path to PDF file") ?? "");
        }

        public static void OpenMenu(SendsignAPICaller apiCaller)
        {
            SendsignOngoingMessages messages = new SendsignOngoingMessages("messages.json");

            SelectionMenu menu = new SelectionMenu([
                "Create signing request", 
                "Fetch request status", 
                "Cancel sign request",
                "Remind signer",
                "Update TTL (Time To Live)",
                "Change customer key",
                "Change sender"
            ], "Sendsign DEMO");

            int result = menu.Prompt();

            switch (result)
            {
                case 0:
                    {
                        SendsignSendRequest request = PromptSendsignRequestBase();
                        do
                        {
                            request.Recipients.Add(PromptCreateRecipient());
                        } 
                        while (Prompt("Add another recipient? (y/N)").ToLower().Equals("y"));

                        do
                        {
                            request.Attachments.Add(PromptSendsignRequestAttachment());
                        } 
                        while (Prompt("Add another attachment? (y/N)").ToLower().Equals("y"));

                        var response = apiCaller.Send(request);
                        if (response.MessageSent != null)
                        {
                            messages.AddMessage(response.MessageSent);
                            messages.Save();
                        }
                        else
                        {
                            Console.WriteLine(response.Error);
                        }

                        break;
                    }
                case 1:
                    {
                        var items = PromptMinimalSendsignRequest(ref messages);
                        try
                        {
                            var fetchResponse = apiCaller.Fetch(items);
                            Console.WriteLine(JsonSerializer.Serialize(fetchResponse, new JsonSerializerOptions()
                            {
                                WriteIndented = true,
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                            }));
                        } 
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                        }

                        break;
                    }
                case 2:
                    {
                        SelectionMenu selectCancelMenu = new SelectionMenu(messages.GetMessages().ToList(), "Select message");
                        SendsignRequest request = new SendsignRequest();

                        int index = selectCancelMenu.Prompt();
                        if (index == -1)
                        {
                            break;
                        }

                        request.MessageId = messages.GetMessages()[index];
                        request.CustomerKey = CustomerKey;
                        request.Sender = Sender;

                        try
                        {
                            var fetchResponse = apiCaller.CancelSignRequest(request);

                            Console.WriteLine(JsonSerializer.Serialize(fetchResponse, new JsonSerializerOptions()
                            {
                                WriteIndented = true,
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                            }));
                        } 
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                        }

                        break;
                    }
                case 3:
                    {
                        SelectionMenu selectCancelMenu = new SelectionMenu(messages.GetMessages().ToList(), "Select message");
                        SendsignRequest request = new SendsignRequest();

                        int index = selectCancelMenu.Prompt();
                        if (index == -1)
                        {
                            break;
                        }

                        request.MessageId = messages.GetMessages()[index];
                        request.CustomerKey = CustomerKey;
                        request.Sender = Sender;

                        try
                        {
                            var fetchResponse = apiCaller.SendsignRemindSigner(new List<SendsignRequest>() {request});

                            Console.WriteLine(JsonSerializer.Serialize(fetchResponse, new JsonSerializerOptions()
                            {
                                WriteIndented = true,
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                            }));
                        } 
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                        }

                        break;
                    }
                case 4:
                    {
                        SelectionMenu selectCancelMenu = new SelectionMenu(messages.GetMessages().ToList(), "Select message");
                        SendsignUpdateTTLRequest request = new SendsignUpdateTTLRequest();

                        int index = selectCancelMenu.Prompt();
                        if (index == -1)
                        {
                            break;
                        }

                        request.MessageId = messages.GetMessages()[index];
                        request.CustomerKey = CustomerKey;
                        request.Sender = Sender;
                        request.TTL = Prompt("New time to live (hours)");

                        try
                        {
                            var fetchResponse = apiCaller.UpdateTTLForMessage(request);

                            Console.WriteLine(JsonSerializer.Serialize(fetchResponse, new JsonSerializerOptions()
                            {
                                WriteIndented = true,
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                            }));
                        } 
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                        }

                        break;
                    }
                case 5:
                    {
                        CustomerKey = Prompt("Customer key");
                        break;
                    }
                case 6:
                    {
                        Sender = Prompt("Sender");
                        break;
                    }
                default:
                    break;
            }
        }

        public static int Main(string[] args)
        {
            SendsignConfiguration configuration = SendsignConfiguration.Load("appsettings.json");
            Sender = configuration.Sender ?? 
                Prompt("Customer key (provided by Twoday)");
            CustomerKey = configuration.CustomerKey ??
                Prompt("Sender (personal number / SSN)");

            if (configuration.Url == null || configuration.Url == "")
            {
                Console.Error.WriteLine("Url is empty or undefined in appsettings.json");
                return -1;
            }
            
            while (true)
            {
                OpenMenu(new SendsignAPICaller(configuration.Url));
                Console.WriteLine();
            }
        }
    }
}