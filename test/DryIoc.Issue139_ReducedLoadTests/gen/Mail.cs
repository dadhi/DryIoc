namespace Mail
{
    public class EmailAddress
    {
        public EmailAddress(
            string arg0,
            AddressSourceType arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly AddressSourceType field1;
    }


    public class AddressSourceType
    {
    }
}