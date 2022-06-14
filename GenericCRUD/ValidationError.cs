namespace GenericCRUD
{
    public class ValidationError
    {
        public string Field { get; set; }
        public string Reason { get; set; }

        public ValidationError(string field, string reason)
        {
            Field = field;
            Reason = reason;
        }
    }
}