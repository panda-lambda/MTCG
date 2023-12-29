using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Utilities
{
    public class PackageNotFoundException : Exception
    {
        public PackageNotFoundException(string message) : base(message) { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
    public class InsufficientCoinsException : Exception
    {
        public InsufficientCoinsException(string message) : base(message) { }
    }
    public class NoAvailableCardsException : Exception
    {
        public NoAvailableCardsException(string message) : base(message) { }
    }
    public class InternalServerErrorException : Exception
    {
        public InternalServerErrorException(string message) : base(message) { }
    }
    //public class InsufficientCoinsException : Exception
    //{
    //    public InsufficientCoinsException(string message) : base(message) { }
    //}
    //public class InsufficientCoinsException : Exception
    //{
    //    public InsufficientCoinsException(string message) : base(message) { }
    //}
    //public class InsufficientCoinsException : Exception
    //{
    //    public InsufficientCoinsException(string message) : base(message) { }
    //}
}
