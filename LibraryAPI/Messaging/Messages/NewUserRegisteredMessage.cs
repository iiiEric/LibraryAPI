namespace LibraryAPI.Messaging.Messages
{
    public class NewUserRegisteredMessage
    {
        public string Email { get; set; } = default!;
        public string Subject { get; set; } = "Welcome to the Library API";
        public string Body { get; set; } = "Congratulations! You now have access to the API. Please contact the administrator to obtain your token.";
    }
}
