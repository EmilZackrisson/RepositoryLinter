namespace RepositoryLinter.Exceptions;

public class CheckFailedException(string message) : Exception(message);