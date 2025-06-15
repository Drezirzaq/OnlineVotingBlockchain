using System.Security.Cryptography;
using System.Text;

namespace MainBlockchain
{
    public class ConfirmParticipation : Transaction
    {
        public string PollId { get; set; }
        public string Sh { get; set; }

        protected override string CombineData() => $"{PollId}{Sh}";
    }
}
