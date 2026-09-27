namespace SenaViva.API.Errors;

public class NotFoundException(string message) : Exception(message);
