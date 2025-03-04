namespace MainBlockchain
{
    public static class ValidationHandler
    {
        public static ValidatorFactory validatorFactory;
        public static bool IsValid(IValidatable validatable)
        {
            var validators = validatorFactory.Create(validatable);
            foreach (var validator in validators)
            {
                if (validator.Validate() == false)
                    return false;
            }
            return true;
        }
    }
}