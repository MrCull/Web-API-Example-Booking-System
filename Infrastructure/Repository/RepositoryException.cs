namespace Infrastructure.Repository;

public class RepositoryException : Exception
{
    public RepositoryException(string message) : base(message)
    {
    }
}

public class EntityNotExistsException : RepositoryException
{
    public EntityNotExistsException(string message) : base(message)
    {
    }
}


