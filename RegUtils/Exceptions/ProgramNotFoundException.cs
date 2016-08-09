namespace RegUtils.Exceptions
{
    public class ProgramNotFoundException : RuntimeException
    {
        public ProgramNotFoundException() { }
        public ProgramNotFoundException(string message) : base(message) { }
    }
}
