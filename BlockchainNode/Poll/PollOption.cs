namespace MainBlockchain
{
    public class PollOption
    {
        public string Id { get; private set; }
        public string Option { get; private set; }
        public PollOption(string option, string id)
        {
            Id = id;
            Option = option;
        }
    }
}