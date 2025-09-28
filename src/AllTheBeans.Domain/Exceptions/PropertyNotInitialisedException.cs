namespace AllTheBeans.Domain.Exceptions;

public class PropertyNotInitialisedException(string propertyName) 
    : Exception($"{propertyName} is not initialised")
{
}
