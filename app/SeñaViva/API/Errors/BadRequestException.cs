namespace SenaViva.API.Errors;

public class BadRequestException(string message) : Exception(message);
