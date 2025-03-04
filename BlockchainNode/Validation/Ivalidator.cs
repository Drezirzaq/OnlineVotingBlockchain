using System.ComponentModel.DataAnnotations;

namespace MainBlockchain
{
    public interface IValidator
    {
        public bool Validate();
    }
}